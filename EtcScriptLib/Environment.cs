using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class SystemVariable : Variable
	{
		public VirtualMachine.InvokeableFunction Implementation;

		public SystemVariable(String Name, Func<VirtualMachine.ExecutionContext, Object> Implementation, String Typename)
		{
			this.Implementation = new VirtualMachine.NativeFunction((c, l) => Implementation(c));
			this.StorageMethod = VariableStorageMethod.System;
			this.DeclaredTypeName = Typename;
			this.Name = Name;
		}
	}

	public class Environment
	{
		public ParseContext Context = Compile.GetDefaultParseContext();

		private List<Declaration> Functions = new List<Declaration>();

		public List<Declaration> Build(String script, Func<String, ErrorStrategy> OnError, bool DelayEmission = false)
		{
			return Compile.Build(script, Context, OnError, DelayEmission);
		}

		public Declaration CompileString(String script)
		{
			var _script = "macro _@ { " + script + " }";
			var stream = new TokenStream(new Compile.StringIterator(_script), Context);
			var declaration = Parse.ParseMacroDeclaration(stream, Context);
			declaration.OwnerContextID = 0;
			var into = new VirtualMachine.InstructionList();
			Context.ID = 0;
			declaration.Body.EmitInstructions(declaration.DeclarationScope, into);
			return declaration;
		}

		public void AddSystemMacro(String Header, Func<VirtualMachine.ExecutionContext, List<Object>, Object> Implementation)
		{
			var decl = Declaration.Parse(Header);
			var argumentCount = decl.Terms.Count(t => t.Type == DeclarationTermType.Term);
			decl.Body.CacheSystemImplementation(argumentCount, Implementation);
			decl.Type = DeclarationType.System;
			Context.ActiveScope.Macros.Add(decl);
		}

		public void AddSystemVariable(String Name, String ResultTypeName, Func<VirtualMachine.ExecutionContext, Object> Implementation)
		{
			var variable = new SystemVariable(Name, Implementation, ResultTypeName);
			Context.ActiveScope.Variables.Add(variable);
		}

		public void AddControl(Control Control)
		{
			Context.AddControl(Control);
		}

		public static Environment CreateStandardEnvironment()
		{
			var environment = new EtcScriptLib.Environment();
			return environment;
		}

		/// <summary>
		/// This creates and compiles a thunk function of the form 'x _0 _1 ... _n { consider [Name _0 _1 ... _n]; }'
		///   which can then be invoked to consider the specified rule.
		/// </summary>
		/// <param name="Name"></param>
		/// <param name="ArgumentCount"></param>
		/// <returns></returns>
		public VirtualMachine.InvokeableFunction GenerateBasicConsiderRuleImplementation(String Name, int ArgumentCount)
		{
			Context.PushNewScope(ScopeType.Function);
			var argumentList = new List<Ast.Node>();
			argumentList.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = Name }));
			int argumentIndexBase = -2 - ArgumentCount;
			for (int i = 0; i < ArgumentCount; ++i)
			{
				Context.ActiveScope.Variables.Add(new Variable
				{
					Name = "_" + i.ToString(),
					Offset = argumentIndexBase + i
				});
				argumentList.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "_" + i.ToString() }));
			}
			var staticInvokation = new Ast.StaticInvokation(new Token(), argumentList);
			var considerRule = new Ast.ConsiderRule(new Token(), staticInvokation);
			var block = new Ast.BlockStatement(new Token());
			block.Statements.Add(considerRule);
			var lambdaBlock = new LambdaBlock(block);
			Context.ID = 0;
			lambdaBlock.EmitInstructions(Context.ActiveScope, new VirtualMachine.InstructionList());
			var r = lambdaBlock.GetBasicInvokable(argumentList.Count - 1);
			Context.PopScope();

			
			return r;
		}
	}
}
