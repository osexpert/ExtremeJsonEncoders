using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ExtremeJsonEncoders
{
	internal interface IMustEscapeChar
	{
		bool MustEscapeChar(char c);
	}
}