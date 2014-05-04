using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
	public enum Operand
	{
		NONE = 0,
		NEXT = 1,
		PUSH = 2,
		POP = 3,
		PEEK = 4,
		R = 5,
		STRING = 6,
		F = 7,
	}

	public struct Instruction
    {
        public InstructionSet Opcode;
        public Operand FirstOperand;
        public Operand SecondOperand;
        public Operand ThirdOperand;
		public String Annotation;

		public Instruction(
			InstructionSet Opcode, 
			Operand FirstOperand, 
			Operand SecondOperand, 
			Operand ThirdOperand,
			String Annotation = null)
		{
			this.Opcode = Opcode;
			this.FirstOperand = FirstOperand;
			this.SecondOperand = SecondOperand;
			this.ThirdOperand = ThirdOperand;
			this.Annotation = Annotation;
		}

        public static Instruction? TryParse(String parseFrom)
        {
            try
            {
                return Parse(parseFrom);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Instruction Parse(String parseFrom)
        {
            var r = new Instruction();

			var annotationPoint = parseFrom.IndexOf('#');
			if (annotationPoint >= 0)
			{
				r.Annotation = parseFrom.Substring(annotationPoint + 1);
				parseFrom = parseFrom.Substring(0, annotationPoint);
			}

            var parts = parseFrom.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            r.Opcode = (Enum.Parse(typeof(InstructionSet), parts[0]) as InstructionSet?).Value;
            r.FirstOperand = r.SecondOperand = r.ThirdOperand = Operand.NONE;
            if (parts.Length >= 2) r.FirstOperand = (Enum.Parse(typeof(Operand), parts[1]) as Operand?).Value;
            if (parts.Length >= 3) r.SecondOperand = (Enum.Parse(typeof(Operand), parts[2]) as Operand?).Value;
            if (parts.Length >= 4) r.ThirdOperand = (Enum.Parse(typeof(Operand), parts[3]) as Operand?).Value;
            return r;
        }

		private String fittab(String s, int l)
		{
			//if (s.Length > l) return s.Substring(0, l);
			if (s.Length == l) return s;
			else return s + new String(' ', l - s.Length);
		}

        public override string ToString()
        {
			return fittab(Opcode.ToString(), 24) + " "
				+ FirstOperand.ToString() + " "
				+ SecondOperand.ToString() + " "
				+ fittab(ThirdOperand.ToString(), 5) +
				(String.IsNullOrEmpty(Annotation) ? "" : (" #" + Annotation));
        }
    }
}
