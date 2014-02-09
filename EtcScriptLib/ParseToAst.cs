using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
    public class ParseToAst
    {
		private static Ast.Let BuildLetStatement(
			Iterator<Token> state,
			OperatorSettings operators)
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

			var r = new Ast.Let();
			r.LHS = ParseExpression(lValueLine.GetIterator(), operators, true);
			r.Value = ParseExpression(state, operators, true);
			return r;
		}

		private static Ast.If BuildIf(
			Iterator<Token> state,
			OperatorSettings operators,
			Ast.Block thenBlock)
		{
			state.Advance();  //Skip 'if'

			var r = new Ast.If();
			r.Header = ParseExpression(state, operators, true);
			r.ThenBlock = thenBlock;
			return r;
		}

		private static Ast.Statement BuildForeachStatement(
			Iterator<Token> state,
			OperatorSettings operators,
			Ast.Node body)
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
				var source = ParseExpression(state, operators, true);
				var r = new Ast.ForeachIn();
				r.VariableName = valueName;
				r.Source = source;
				r.Body = body;
				return r;
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

				var r = new Ast.ForeachFrom();
				r.VariableName = valueName;
				r.Min = low;
				r.Max = high;
				r.Body = body;
				return r;
			}
			else
				throw new CompileError("Unknown range type '" + state.Next().Value + "'", state.Next());
		}

		private static Ast.Statement BuildStatement(
			Iterator<Construct> state,
			OperatorSettings operators,
			bool AllowElse)
		{
			var construct = state.Next();
			if (construct is Block)
				throw new CompileError("Unadorned block illegal", construct);

			var lineState = (construct as Line).GetIterator();
			if (lineState.Next().Type != TokenType.Identifier) throw new CompileError("Expected identifier", state.Next());
			Ast.Statement r = null;

			if (lineState.Next().Value == "let")
			{
				r = BuildLetStatement(lineState, operators);
				state.Advance();
			}
			else if (lineState.Next().Value == "foreach")
			{
				state.Advance();
				if (state.AtEnd() || !(state.Next() is Block)) throw new CompileError("Expected block after foreach", construct);
				var body = BuildBlock(state.Next() as Block, operators);
				r = BuildForeachStatement(lineState, operators, body);
				state.Advance();
			}
			else if (lineState.Next().Value == "if" || lineState.Next().Value == "else")
			{
				if (lineState.Next().Value == "else")
				{
					if (!AllowElse) throw new CompileError("Else clause not allowed here", construct);
					lineState.Advance(); //Skip the else bit.
				}

				state.Advance();
				if (state.AtEnd() || !(state.Next() is Block)) throw new CompileError("Expected block after if or else", construct);
				var thenBlock = BuildBlock(state.Next() as Block, operators);

				if (!lineState.AtEnd() && lineState.Next().Value == "if")
					r = BuildIf(lineState, operators, thenBlock);
				else //We followed an else clause here.
				{
					r = new Ast.BlockStatement();
					(r as Ast.BlockStatement).Statements = thenBlock.Statements;
				}
				state.Advance();

				//Check for optional else
				if (!state.AtEnd() && (state.Next() is Line) && (state.Next() as Line).Tokens[0].Value == "else")
				{
					var elseBlock = BuildStatement(state, operators, true);
					(r as Ast.If).ElseBlock = new Ast.Block();
					if (elseBlock is Ast.BlockStatement)
						(r as Ast.If).ElseBlock.Statements = (elseBlock as Ast.BlockStatement).Statements;
					else
						(r as Ast.If).ElseBlock.Statements.Add(elseBlock);
				}
			}
			else if (lineState.Next().Value == "return")
			{
				lineState.Advance();
				r = new Ast.Return();
				if (!lineState.AtEnd()) (r as Ast.Return).Value = ParseExpression(lineState, operators, true);
				state.Advance();
			}
			else
			{
				//If it's not any special statement, it must be a function call.
				r = ParseExpression(lineState, operators, true) as Ast.FunctionCall;
				if (r == null) throw new CompileError("Expected function call.", construct);
				state.Advance();
			}

			return r;
		}

		private static Ast.Node ParseBinaryOperations(
			List<Ast.Node> input,
			OperatorSettings operators,
			int currentPrecedence,
			bool root = false
			)
		{
			if (input.Count == 1) return input[0];
			if (!operators.precedence.ContainsKey(currentPrecedence))
			{
				if (root)
				{
					var r_ = new Ast.FunctionCall();
					r_.Parameters = input;
					return r_;
				}
				throw new CompileError("Consecutive tokens without operator", null);
			}

			var operatorTokens = operators.precedence[currentPrecedence];
			var rhs = new List<Ast.Node>();
			var foundOperator = false;

			var i = input.Count - 1;
			for (; i >= 0; --i)
			{
				var currentToken = input[i];
				if (currentToken is Ast.Token && 
					operatorTokens.Count((o) => {
						return o.token == (currentToken as Ast.Token).Name.Value; }) >= 1)
				{
					foundOperator = true;
					break;
				}
				else
				{
					rhs.Insert(0, currentToken);
					continue;
				}
			}

			if (!foundOperator) return ParseBinaryOperations(input, operators, currentPrecedence + 1, root);

			var operatorToken = (input[i] as Ast.Token).Name;

			var lhs = new List<Ast.Node>();
			for (int x = 0; x < i ; ++x)
				lhs.Add(input[x]);

			if (lhs.Count == 0 || rhs.Count == 0) 
				throw new CompileError("Missing operand", operatorToken);

			var r = new Ast.BinaryOperator();

			if (lhs.Count == 1)
				r.LHS = lhs[0];
			else
				r.LHS = ParseBinaryOperations(lhs, operators, currentPrecedence);

			if (rhs.Count == 1)
				r.RHS = rhs[0];
			else
				r.RHS = ParseBinaryOperations(rhs, operators, currentPrecedence + 1);
			
			return r;
		}

		private static Ast.Node ParseExpression(
			Iterator<Token> state, 
			OperatorSettings operators, 
			bool root = false,
			TokenType openedWith = TokenType.EndOfFile)
		{
			Token? lastToken = null;
			var subExpressions = new List<Ast.Node>();

			while (!state.AtEnd())
			{
				lastToken = state.Next();
				if (state.Next().Type == TokenType.Dot)
				{
					if (subExpressions.Count == 0)
						throw new CompileError("Can't access member of nothing", state.Next());
					state.Advance();
					if (state.AtEnd() || state.Next().Type != TokenType.Identifier)
						throw new CompileError("Expected identifier", state.AtEnd() ? lastToken.Value : state.Next());
					var newNode = new Ast.MemberAccess();
					newNode.Object = subExpressions[subExpressions.Count - 1];
					newNode.Name = state.Next().Value;
					subExpressions[subExpressions.Count - 1] = newNode;
					state.Advance();
				}
				else if (state.Next().Type == TokenType.OpenParen)
				{
					state.Advance();
					subExpressions.Add(ParseExpression(state, operators, false, TokenType.OpenParen));
				}
				else if (state.Next().Type == TokenType.OpenBracket)
				{
					state.Advance();
					var containedExpression = ParseExpression(state, operators, false, TokenType.OpenBracket);
					if (containedExpression is Ast.FunctionCall)
						subExpressions.Add(containedExpression);
					else
					{
						var wrap = new Ast.FunctionCall();
						wrap.Parameters.Add(containedExpression);
						subExpressions.Add(wrap);
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
					var finalResult = ParseBinaryOperations(subExpressions, operators, 0);
					return finalResult;
				}
				else
				{
					var token = new Ast.Token();
					token.Name = lastToken.Value;
					subExpressions.Add(token);
					state.Advance();
				}
			}

			if (!root) throw new CompileError("Unexpected end of statement in parenthetical", state.Next());
			
			if (subExpressions.Count == 1) return subExpressions[0];
			else return ParseBinaryOperations(subExpressions, operators, 0, root);
		}

		private static Ast.Block BuildBlock(
			Block block, 
			OperatorSettings operators)
		{
			var r = new Ast.Block();
			var state = block.GetIterator();
			while (!state.AtEnd())
				r.Statements.Add(BuildStatement(state, operators, false));
			return r;
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

				if (block.Children.Count == 2)
					r.Body = BuildBlock(block.Children[1] as Block, operators);
				else
					r.Body = new Ast.Block();

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
			nodeIterator.ParseToAst = true;
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
