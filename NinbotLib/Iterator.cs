using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ninbot
{
	public interface Iterator<T>
	{
		T Next();
		void Advance();
		bool AtEnd();
	}
}
