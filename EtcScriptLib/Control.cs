using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public enum ControlBlockType
	{
		NoBlock,
		OptionalBlock,
		RequiredBlock
	}

	public class Control
	{
		public List<DeclarationTerm> DeclarationTerms;
		public Func<List<Ast.Node>, Ast.Node, Ast.Node> TransformationFunction;
		public ControlBlockType BlockType = ControlBlockType.RequiredBlock;

		public static Control Create(
			Declaration Declaration,
			ControlBlockType BlockType,
			Func<List<Ast.Node>, Ast.Node, Ast.Node> TransformationFunction)
		{
			return new Control
			{
				DeclarationTerms = Declaration.Terms,
				TransformationFunction = TransformationFunction,
				BlockType = BlockType
			};
		}
	}
}
