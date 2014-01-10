using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ninbot
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
				EmitExpressionTree(ParseExpression(state, operators, true), operators, into);
				into.AddInstruction("SET_VARIABLE NEXT POP", lValue.Token.Value.Value);
			}
			else if (lValue.Type == ExpressionBlockTypes.BinaryOperation && lValue.Token.Value.Value == ".")
			{
				if (lValue.SubExpressions[1].Type != ExpressionBlockTypes.Token ||
					lValue.SubExpressions[1].Token.Value.Type != TokenType.Identifier)
					throw new CompileError("Expected a member name", lValue.SubExpressions[1].Token.Value);
				EmitExpressionTree(lValue.SubExpressions[0], operators, into);
				EmitExpressionTree(ParseExpression(state, operators, true), operators, into);
				into.AddInstruction("SET_MEMBER POP NEXT POP", lValue.SubExpressions[1].Token.Value.Value);
			}
			else
				throw new CompileError("Expected an lvalue", lValueLine.Tokens[0]);
		}

		private static void BuildStatement(
			Iterator<Construct> state, 
			OperatorSettings operators,
			VirtualMachine.InstructionList into)
		{
			var construct = state.Next();
			if (construct is Block)
			{
				BuildBlock(construct as Block, operators, into);
				state.Advance();
			}
			else
			{
				into.Add(VirtualMachine.Annotation.Create(
					String.Join(" ", (construct as Line).Tokens.Select((t) => { return t.Value; }))));
				var lineState = (construct as Line).GetIterator();
				if (lineState.Next().Type != TokenType.Identifier) throw new CompileError("Expected identifier", state.Next());

				if (lineState.Next().Value == "let")
				{
					BuildLetStatement(lineState, operators, into);
					state.Advance();
				}
				else
				{
					//If it's not any special statement, it must be a function call.
					EmitExpressionTree(ParseExpression(lineState, operators, true), operators, into);
					into.AddInstruction("MOVE POP NONE"); //Cleanup stack
					state.Advance();
				}
			}
		}

		private enum ExpressionBlockTypes
		{
			Token,
			FunctionCall,
			BinaryOperation,
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
		}

		private static ExpressionBlock ParseBinaryOperations(
			ExpressionBlock input,
			OperatorSettings operators,
			int currentPrecedence
			)
		{
			if (input.Type != ExpressionBlockTypes.FunctionCall) return input;
			if (!operators.precedence.ContainsKey(currentPrecedence)) return input;

			var operatorTokens = operators.precedence[currentPrecedence];
			var rhs = ExpressionBlock.Create(ExpressionBlockTypes.FunctionCall, null);
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

			var lhs = ExpressionBlock.Create(ExpressionBlockTypes.FunctionCall, null);
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
			bool root = false)
		{
			var r = ExpressionBlock.Create(ExpressionBlockTypes.FunctionCall, null);
			Token? lastToken = null;

			while (!state.AtEnd())
			{
				lastToken = state.Next();
				if (state.Next().Type == TokenType.OpenParen)
				{
					state.Advance();
					r.SubExpressions.Add(ParseExpression(state, operators));
				}
				else if (state.Next().Type == TokenType.CloseParen)
				{
					state.Advance();
					return ParseBinaryOperations(r, operators, 0);
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
			VirtualMachine.InstructionList into)
		{
			if (expressionBlock.Type == ExpressionBlockTypes.Token)
			{
				var internalToken = expressionBlock.Token.Value;
				if (internalToken.Type == TokenType.Identifier)
					into.AddInstruction("LOOKUP NEXT", internalToken.Value);
				else if (internalToken.Type == TokenType.String)
					into.AddInstruction("MOVE NEXT PUSH", internalToken.Value);
				else if (internalToken.Type == TokenType.Number)
					into.AddInstruction("MOVE NEXT PUSH", Convert.ToSingle(internalToken.Value));
				else
					throw new CompileError("Unable to translate token type", internalToken);
			}
			else if (expressionBlock.Type == ExpressionBlockTypes.BinaryOperation)
			{
				foreach (var subExpression in expressionBlock.SubExpressions)
					EmitExpressionTree(subExpression, operators, into);
				var @operator = operators.FindOperator(expressionBlock.Token.Value.Value);
				if (@operator.HasValue)
					into.AddInstruction(@operator.Value.instruction.ToString() + " POP POP PUSH");
				else
					throw new CompileError("Undefined operator", expressionBlock.Token.Value);
			}
			else if (expressionBlock.Type == ExpressionBlockTypes.FunctionCall)
			{
				into.AddInstruction("EMPTY_LIST PUSH");
				foreach (var subExpression in expressionBlock.SubExpressions)
				{
					EmitExpressionTree(subExpression, operators, into);
					into.AddInstruction("APPEND POP PEEK PEEK");
				}
				into.AddInstruction("INVOKE POP");
				into.AddInstruction("MOVE R PUSH"); 
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

				if (headerState.Next().Type != TokenType.Identifier)
					throw new CompileError("Expected identifier", headerState.Next());
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

				r.Instructions = new VirtualMachine.InstructionList();
				if (block.Children.Count == 2)
				{
					BuildBlock(block.Children[1] as Block, operators, r.Instructions);
					r.Instructions = r.Instructions[0] as VirtualMachine.InstructionList;
				}

				return r;
			}
			catch (CompileError ce)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new CompileError(e.Message + e.StackTrace, block);
			}
		}

        public static List<Declaration> Build(Iterator<Token> Stream, OperatorSettings operators)
        {
			var nodeIterator = DeclarationIterator.Create(BlockIterator.Create(LineIterator.Create(Stream)), operators);
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
