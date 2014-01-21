using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public interface Iterator<T>
	{
		T Next();
		void Advance();
		bool AtEnd();
	}
}
