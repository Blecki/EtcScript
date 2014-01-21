using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class OperatorSettings
	{
		public struct Operator
		{
			public String token;
			public VirtualMachine.InstructionSet instruction;
		}

		public Dictionary<int, List<Operator>> precedence = new Dictionary<int, List<Operator>>();
		public List<String> operatorStrings = new List<String>();

		public void AddOperator(int precedence, String token, VirtualMachine.InstructionSet instruction)
		{
			if (!this.precedence.ContainsKey(precedence))
				this.precedence.Add(precedence, new List<Operator>());
			this.precedence[precedence].Add(new Operator { token = token, instruction = instruction });

			var i = 0;
			for (; i < operatorStrings.Count && operatorStrings[i].Length > token.Length; ++i)
				;
			operatorStrings.Insert(i, token);
		}

		public Operator? FindOperator(String token)
		{
			foreach (var p in precedence)
				foreach (var op in p.Value)
					if (op.token == token) return op;
			return null;
		}
	}
}
