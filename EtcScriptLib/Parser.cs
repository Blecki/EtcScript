using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
    public class Parser
    {
		private static void BuildLetStatement(
			Iterator<Token> state,
			OperatorSettings operators,
			VirtualMachine.InstructionList into)
		{
			var start = state.Next();

			state.Advance();
			var lValueLine = new Line(0);

			while (!state.AtEnd())
			{
				if (state.Next().Value == "=") break;
				lValueLine.Tokens.Add(state.Next());
				state.Advance();
			}

			if (state.AtEnd()) throw new CompileError("Expected =", start);

			state.Advance();

			var lValue = ParseExpression(lValueLine.GetIterator(), operators, true);
			if (lValue.Type == ExpressionBlockTypes.Token && lValue.Token.Value.Type == TokenType.Identifier)
			{
				EmitExpressionTree(ParseExpression(state, operators, true), operators, into, "R");
				into.AddInstruction("SET_VARIABLE R NEXT", lValue.Token.Value.Value);
			}
			else if (lValue.Type == ExpressionBlockTypes.MemberAccess)
			{
				EmitExpressionTree(lValue.SubExpressions[0], operators, into, "PUSH");
				EmitExpressionTree(ParseExpression(state, operators, true), operators, into, "R");
				into.AddInstruction("SET_MEMBER R NEXT POP", lValue.Token.Value.Value);
			}
			else
				throw new CompileError("Expected an lvalue", lValueLine.Tokens[0]);
		}

		private static void BuildIfHeader(
			Iterator<Token> state,
			OperatorSettings operators,
			VirtualMachine.InstructionList into)
		{
			state.Advance();  //Skip 'if'

			EmitExpressionTree(ParseExpression(state, operators, true), operators, into, "R");
			into.AddInstruction("IF_FALSE R",
				"SKIP NEXT", 2); //Should be followed immediately by then and else branches.
		}

		private static void BuildForeachStatement(
			Iterator<Token> state,
			OperatorSettings operators,
			VirtualMachine.InstructionList body,
			VirtualMachine.InstructionList into)
		{
			var start = state.Next();
			state.Advance();

			if (state.AtEnd() || state.Next().Type != TokenType.Identifier) 
				throw new CompileError("Expected identifier after foreach", start);

			var valueName = state.Next().Value;

			state.Advance();

			if (state.AtEnd() || state.Next().Type != TokenType.Identifier)
				throw new CompileError("Expected in or from keyword", start);

			if (state.Next().Value == "in")
			{
				state.Advance();
				//Go ahead and emit the expression that provides the list
				EmitExpressionTree(ParseExpression(state, operators, true), operators, into, "R");
				into.AddInstruction(
					"SET_VARIABLE R NEXT", "__list@" + valueName, //Store the list of items in scope
					"LENGTH R R",
					"SET_VARIABLE R NEXT", "__total@" + valueName, //Total elements in list
					"SET_VARIABLE NEXT NEXT", 0, "__counter@" + valueName,
					"BRANCH PUSH NEXT",       //LOOP BRANCH
					new VirtualMachine.InstructionList(
						"LOOKUP NEXT R", "__total@" + valueName,
						"LOOKUP NEXT PUSH", "__counter@" + valueName,
						"GREATER_EQUAL POP R R",
						"IF_TRUE R",  //If counter is 0, stop looping.
						"BREAK POP",
						"LOOKUP NEXT R", "__list@" + valueName,
						"LOOKUP NEXT PUSH", "__counter@" + valueName,
						"INDEX POP R R",
						"SET_VARIABLE R NEXT", valueName,
						body,
						"LOOKUP NEXT R", "__counter@" + valueName, //Increment the counter
						"INCREMENT R R",
						"SET_VARIABLE R NEXT", "__counter@" + valueName,
						"CONTINUE POP")
						);
			}
			else if (state.Next().Value == "from")
			{
				state.Advance();
				if (state.AtEnd() || state.Next().Type != TokenType.Number)
					throw new CompileError("Expected low value", start);
				var low = Int32.Parse(state.Next().Value);
				state.Advance();
				if (state.AtEnd() || state.Next().Value != "to")
					throw new CompileError("Expected to", start);
				state.Advance();
				if (state.AtEnd() || state.Next().Type != TokenType.Number)
					throw new CompileError("Expected high value", start);
				var high = Int32.Parse(state.Next().Value);
				state.Advance();

				into.AddInstruction(
					"SET_VARIABLE NEXT NEXT", high, "__total@" + valueName, //Total elements in list
					"SET_VARIABLE NEXT NEXT", low, "__counter@" + valueName,
					"BRANCH PUSH NEXT",       //LOOP BRANCH
					new VirtualMachine.InstructionList(
						"LOOKUP NEXT R", "__total@" + valueName,
						"LOOKUP NEXT PUSH", "__counter@" + valueName,
						"GREATER POP R R",
						"IF_TRUE R",  //If counter is >= total, stop looping.
						"BREAK POP",
						"LOOKUP NEXT R", "__counter@" + valueName,
						"SET_VARIABLE R NEXT", valueName,
						body,
						"LOOKUP NEXT R", "__counter@" + valueName, //Increment the counter
						"INCREMENT R R",
						"SET_VARIABLE R NEXT", "__counter@" + valueName,
						"CONTINUE POP")
						);
			}
			else
				throw new CompileError("Unknown range type '" + state.Next().Value + "'", state.Next());
			
		}

		private static void BuildStatement(
			Iterator<Construct> state, 
			OperatorSettings operators,
			VirtualMachine.InstructionList into,
			bool allowElseBeforeIf = false)
		{
			var construct = state.Next();
			if (construct is Block)
			{
				BuildBlock(construct as Block, operators, into);
				state.Advance();
			}
			else
			{
				var annotation = "";
				for (int i = 0; i < (construct as Line).IndentionLevel; ++i)
					annotation += " →  ";
				into.Add(VirtualMachine.Annotation.Create(
					annotation +
					String.Join(" ", (construct as Line).Tokens.Select((t) => { return t.Value; }))));
				var lineState = (construct as Line).GetIterator();
				if (lineState.Next().Type != TokenType.Identifier) throw new CompileError("Expected identifier", state.Next());

				if (lineState.Next().Value == "let")
				{
					BuildLetStatement(lineState, operators, into);
					state.Advance();
				}
				else if (lineState.Next().Value == "foreach")
				{
					state.Advance();
					if (state.AtEnd() || !(state.Next() is Block))
						throw new CompileError("Expected block after foreach", construct);
					var body = new VirtualMachine.InstructionList();
					BuildBlock(state.Next() as Block, operators, body);
					body = new VirtualMachine.InPlace(body[0] as VirtualMachine.InstructionList);
					BuildForeachStatement(lineState, operators, body, into);
					state.Advance();
				}
				else if (lineState.Next().Value == "if" || lineState.Next().Value == "else")
				{
					if (lineState.Next().Value == "else")
					{
						if (allowElseBeforeIf) lineState.Advance();
						else throw new CompileError("Else not allowed here", lineState.Next());
					}

					BuildIfHeader(lineState, operators, into);
					state.Advance();
					if (state.AtEnd() || !(state.Next() is Block))
						into.AddInstruction("MOVE PEEK PEEK"); //No block?
					else
					{
						into.AddInstruction("BRANCH PUSH NEXT");
						var thenBlock = new VirtualMachine.InstructionList();
						BuildBlock(state.Next() as Block, operators, thenBlock);
						(thenBlock[0] as VirtualMachine.InstructionList).AddInstruction("BREAK POP");
						into.Add(thenBlock[0]);
						into.AddInstruction("SKIP NEXT", 1); //Skip else branch
						state.Advance();
					}

					if (state.AtEnd() || !(state.Next() is Line) || (state.Next() as Line).Tokens[0].Value != "else")
						into.AddInstruction("MOVE PEEK PEEK"); //No else block
					else
					{
						into.AddInstruction("BRANCH PUSH NEXT");

						construct = state.Next();
						lineState = (construct as Line).GetIterator();
						lineState.Advance(); //Assume begins with 'else'.

						var elseBlock = new VirtualMachine.InstructionList();
						if (!lineState.AtEnd() && lineState.Next().Value == "if") //else if!
							BuildStatement(state, operators, elseBlock, true);
						else
						{
							state.Advance();
							if (state.AtEnd() || !(state.Next() is Block))
							{ }
							else
							{
								var temp = new VirtualMachine.InstructionList();
								BuildBlock(state.Next() as Block, operators, temp);
								elseBlock.AddRange(temp[0] as VirtualMachine.InstructionList);
							}
						}

						elseBlock.AddInstruction("BREAK POP");
						into.Add(elseBlock);
						state.Advance();
					}
				}
				else if (lineState.Next().Value == "return")
				{
					lineState.Advance();
					if (!lineState.AtEnd())
						EmitExpressionTree(ParseExpression(lineState, operators, true), operators, into, "R");
					into.AddInstruction("LOOKUP NEXT PUSH", "@stack-size", "RESTORE_STACK POP", "BREAK POP");
					state.Advance();
				}
				else
				{
					//If it's not any special statement, it must be a function call.
					var expression = ParseExpression(lineState, operators, true);
					if (expression.Type == ExpressionBlockTypes.Unresolved) expression.Type = ExpressionBlockTypes.FunctionCall;
					EmitExpressionTree(expression, operators, into, "R");
					state.Advance();
				}
			}
		}

		private enum ExpressionBlockTypes
		{
			Unresolved,
			Token,
			FunctionCall,
			BinaryOperation,
			MemberAccess,
		}

		private struct ExpressionBlock
		{
			public ExpressionBlockTypes Type;
			public Token? Token;
			public List<ExpressionBlock> SubExpressions;

			public static ExpressionBlock Create(ExpressionBlockTypes Type, Token? Token)
			{
				return new ExpressionBlock
				{
					Type = Type,
					Token = Token,
					SubExpressions = new List<ExpressionBlock>()
				};
			}

			public Token? FirstValidToken()
			{
				if (Token.HasValue) return Token;
				else foreach (var child in SubExpressions)
					{
						var t = child.FirstValidToken();
						if (t.HasValue) return t;
					}
				return null;
			}
		}

		private static ExpressionBlock ParseBinaryOperations(
			ExpressionBlock input,
			OperatorSettings operators,
			int currentPrecedence
			)
		{
			if (input.Type != ExpressionBlockTypes.Unresolved) return input;
			if (!operators.precedence.ContainsKey(currentPrecedence)) return input;

			var operatorTokens = operators.precedence[currentPrecedence];
			var rhs = ExpressionBlock.Create(ExpressionBlockTypes.Unresolved, null);
			var foundOperator = false;

			var i = input.SubExpressions.Count - 1;
			for (; i >= 0; --i)
			{
				var currentToken = input.SubExpressions[i];
				if (currentToken.Type == ExpressionBlockTypes.Token && 
					operatorTokens.Count((o) => {
						return o.token == currentToken.Token.Value.Value; }) >= 1)
				{
					foundOperator = true;
					break;
				}
				else
				{
					rhs.SubExpressions.Insert(0, currentToken);
					continue;
				}
			}

			if (!foundOperator) return ParseBinaryOperations(input, operators, currentPrecedence + 1);

			var operatorToken = input.SubExpressions[i].Token.Value;

			var lhs = ExpressionBlock.Create(ExpressionBlockTypes.Unresolved, null);
			for (int x = 0; x < i ; ++x)
				lhs.SubExpressions.Add(input.SubExpressions[x]);

			if (lhs.SubExpressions.Count == 0 || rhs.SubExpressions.Count == 0) 
				throw new CompileError("Missing operand", operatorToken);
			
			var r = ExpressionBlock.Create(ExpressionBlockTypes.BinaryOperation, operatorToken);

			if (lhs.SubExpressions.Count == 1)
				r.SubExpressions.Add(lhs.SubExpressions[0]);
			else
				r.SubExpressions.Add(ParseBinaryOperations(lhs, operators, currentPrecedence));

			if (rhs.SubExpressions.Count == 1)
				r.SubExpressions.Add(rhs.SubExpressions[0]);
			else
				r.SubExpressions.Add(ParseBinaryOperations(rhs, operators, currentPrecedence + 1));


			return r;
		}

		private static ExpressionBlock ParseExpression(
			Iterator<Token> state, 
			OperatorSettings operators, 
			bool root = false,
			TokenType openedWith = TokenType.EndOfFile)
		{
			var r = ExpressionBlock.Create(ExpressionBlockTypes.Unresolved, null);
			Token? lastToken = null;

			while (!state.AtEnd())
			{
				lastToken = state.Next();
				if (state.Next().Type == TokenType.Dot)
				{
					if (r.SubExpressions.Count == 0)
						throw new CompileError("Can't access member of nothing", state.Next());
					state.Advance();
					if (state.AtEnd() || state.Next().Type != TokenType.Identifier)
						throw new CompileError("Expected identifier", state.AtEnd() ? lastToken.Value : state.Next());
					var newExpression = ExpressionBlock.Create(ExpressionBlockTypes.MemberAccess, state.Next());
					newExpression.SubExpressions.Add(r.SubExpressions[r.SubExpressions.Count - 1]);
					r.SubExpressions[r.SubExpressions.Count - 1] = newExpression;
					state.Advance();
				}
				else if (state.Next().Type == TokenType.OpenParen)
				{
					state.Advance();
					var containedExpression = ParseExpression(state, operators, false, TokenType.OpenParen);
					r.SubExpressions.Add(containedExpression);
				}
				else if (state.Next().Type == TokenType.OpenBracket)
				{
					state.Advance();
					var containedExpression = ParseExpression(state, operators, false, TokenType.OpenBracket);
					if (containedExpression.Type == ExpressionBlockTypes.Unresolved)
					{
						containedExpression.Type = ExpressionBlockTypes.FunctionCall;
						r.SubExpressions.Add(containedExpression);
					}
					else
					{
						var wrap = ExpressionBlock.Create(ExpressionBlockTypes.FunctionCall, null);
						wrap.SubExpressions.Add(containedExpression);
						r.SubExpressions.Add(wrap);
					}
				}
				else if (state.Next().Type == TokenType.CloseParen || state.Next().Type == TokenType.CloseBracket)
				{
					if (openedWith == TokenType.OpenParen && state.Next().Type != TokenType.CloseParen)
						throw new CompileError("Mismatched parenthethis", state.Next());
					else if (openedWith == TokenType.OpenBracket && state.Next().Type != TokenType.CloseBracket)
						throw new CompileError("Mismatched brackets", state.Next());
					else if (openedWith == TokenType.EndOfFile)
						throw new CompileError("Unexpected close paren or bracket", state.Next());

					state.Advance();
					var finalResult = ParseBinaryOperations(r, operators, 0);
					if (finalResult.SubExpressions.Count == 1) return finalResult.SubExpressions[0];
					return finalResult;
				}
				else
				{
					r.SubExpressions.Add(ExpressionBlock.Create(ExpressionBlockTypes.Token, lastToken));
					state.Advance();
				}
			}

			if (!root) throw new CompileError("Unexpected end of statement in parenthetical", state.Next());
			
			if (r.SubExpressions.Count == 1) return r.SubExpressions[0];
			else return ParseBinaryOperations(r, operators, 0);
		}

		private static void EmitExpressionTree(
			ExpressionBlock expressionBlock,
			OperatorSettings operators,
			VirtualMachine.InstructionList into,
			String destinationOperand)
		{
			if (expressionBlock.Type == ExpressionBlockTypes.Unresolved)
			{
				throw new CompileError("Multiple segments without an intervening operator. Did you mean to invoke?",
					expressionBlock.FirstValidToken() ?? new Token());
			}
			else if (expressionBlock.Type == ExpressionBlockTypes.Token)
			{
				var internalToken = expressionBlock.Token.Value;
				if (internalToken.Type == TokenType.Identifier)
					into.AddInstruction("LOOKUP NEXT " + destinationOperand, internalToken.Value);
				else if (internalToken.Type == TokenType.String)
					into.AddInstruction("MOVE NEXT " + destinationOperand, internalToken.Value);
				else if (internalToken.Type == TokenType.Number)
					into.AddInstruction("MOVE NEXT " + destinationOperand, Convert.ToSingle(internalToken.Value));
				else
					throw new CompileError("Unable to translate token type", internalToken);
			}
			else if (expressionBlock.Type == ExpressionBlockTypes.MemberAccess)
			{
				EmitExpressionTree(expressionBlock.SubExpressions[0], operators, into, "PUSH");
				into.AddInstruction("LOOKUP_MEMBER NEXT POP " + destinationOperand, expressionBlock.Token.Value.Value);
			}
			else if (expressionBlock.Type == ExpressionBlockTypes.BinaryOperation)
			{
				foreach (var subExpression in expressionBlock.SubExpressions)
					EmitExpressionTree(subExpression, operators, into, "PUSH");
				var @operator = operators.FindOperator(expressionBlock.Token.Value.Value);
				if (@operator.HasValue)
					into.AddInstruction(@operator.Value.instruction.ToString() + " POP POP " + destinationOperand);
				else
					throw new CompileError("Undefined operator", expressionBlock.Token.Value);
			}
			else if (expressionBlock.Type == ExpressionBlockTypes.FunctionCall)
			{
				into.AddInstruction("EMPTY_LIST PUSH");
				foreach (var subExpression in expressionBlock.SubExpressions)
				{
					EmitExpressionTree(subExpression, operators, into, "R");
					into.AddInstruction("APPEND R PEEK PEEK");
				}
				into.AddInstruction("INVOKE POP");
				if (destinationOperand != "R")
					into.AddInstruction("MOVE R " + destinationOperand); 
			}
			else
				throw new CompileError("Unknown expression block type", null);
		}

		private static void BuildBlock(
			Block block, 
			OperatorSettings operators,
			VirtualMachine.InstructionList into)
		{
			var internalList = new VirtualMachine.InstructionList();
			var state = block.GetIterator();
			while (!state.AtEnd())
				BuildStatement(state, operators, internalList);
			into.Add(internalList);
		}

		internal static Declaration BuildDeclaration(Block block, OperatorSettings operators)
		{
			try
			{
				var r = new Declaration();

				//if (block.Children.Count != 2) throw new CompileError("Malformed declaration", block);
				var header = block.Children[0] as Line;
				var headerState = header.GetIterator();

				if (headerState.Next().Type != TokenType.Identifier)
					throw new CompileError("Expected identifier", headerState.Next());
				r.Type = headerState.Next().Value;
				headerState.Advance();

				if (headerState.AtEnd() || headerState.Next().Type != TokenType.Identifier)
					throw new CompileError("Expected identifier", headerState.AtEnd() ? header.Tokens[0] : headerState.Next());
				r.Name = headerState.Next().Value;
				headerState.Advance();

				r.Arguments = new List<string>();
				while (!headerState.AtEnd())
				{
					if (headerState.Next().Type != TokenType.Identifier)
						throw new CompileError("Expected identifier", headerState.Next());
					r.Arguments.Add(headerState.Next().Value);
					headerState.Advance();
				}

				var bodyInstructions = new VirtualMachine.InstructionList();
				if (block.Children.Count == 2)
					BuildBlock(block.Children[1] as Block, operators, bodyInstructions);
				else
					bodyInstructions.Add(new VirtualMachine.InstructionList());

				r.Instructions = new VirtualMachine.InstructionList(
					"MARK_STACK R",
					"SET_VARIABLE R NEXT", "@stack-size",
					new VirtualMachine.InPlace(bodyInstructions[0] as VirtualMachine.InstructionList),
					//"LOOKUP NEXT R", "@stack-size", //If we made it back here, stack is clean.
					//"RESTORE_STACK R",
					"BREAK POP");
			
				return r;
			}
			catch (CompileError ce)
			{
				throw ce;
			}
			catch (Exception e)
			{
				throw new CompileError(e.Message + e.StackTrace, block);
			}
		}

        public static List<Declaration> Build(
			Iterator<Token> Stream, 
			OperatorSettings operators,
			Func<String,ErrorStrategy> OnError)
        {
			var nodeIterator = DeclarationIterator.Create(
				BlockIterator.Create(LineIterator.Create(Stream)), operators, OnError);
			var r = new List<Declaration>();
			while (!nodeIterator.AtEnd())
			{
				r.Add(nodeIterator.Next());
				nodeIterator.Advance();
			}
			return r;
        }
    }
}
