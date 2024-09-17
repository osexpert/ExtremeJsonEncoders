// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

//namespace System.Text.Encodings.Web
namespace ExtremeJsonEncoders
{
	//internal unsafe struct AsciiBoolMap
	//{
	//	private const int BufferSize = 128;
	//	internal fixed bool Buffer[BufferSize];

	//	internal void MakeEscapeMap(char[]? extraAsciiEscapeChars)
	//	{
	//		Buffer['"'] = true;
	//		Buffer['\\'] = true;

	//		// control chars
	//		for (int i = 0; i < 0x20; i++)
	//			Buffer[i] = true;

	//		if (extraAsciiEscapeChars != null)
	//			for (int i = 0; i < extraAsciiEscapeChars.Length; i++)
	//				Buffer[extraAsciiEscapeChars[i]] = true;
		
	//	}

	//	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	//	internal bool MustEscape(char c)
	//	{
	//		if (c > 127)
	//			return false;

	//		return Buffer[c];
	//	}
	//}


	/// <summary>
	/// A lookup map that maps individual ASCII chars to a single byte.
	/// Storing a 0 byte indicates that no mapping exists for this input.
	/// </summary>
	internal unsafe struct AsciiByteMap
	{
		private const int BufferSize = 128;
		private fixed byte Buffer[BufferSize];

		internal void InsertAsciiChar(char key, byte value)
		{
			Debug.Assert(key < BufferSize);
			Debug.Assert(value != 0);

			if (key < BufferSize)
			{
				Buffer[key] = value;
			}
		}

		/// <summary>
		/// Returns false if <paramref name="key"/> is non-ASCII or if it
		/// maps to a zero value.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal readonly bool TryLookup(Rune key, out byte value)
		{
			if (key.IsAscii)
			{
				byte entry = Buffer[(uint)key.Value];
				if (entry != 0)
				{
					value = entry;
					return true;
				}
			}

			value = default;
			return false;
		}
	}
}
