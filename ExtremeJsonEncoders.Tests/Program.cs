using System.Linq;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlTypes;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Text.Json;
using System.Text.Encodings.Web;
using Microsoft.Testing.Platform.Extensions.Messages;



namespace ExtremeJsonEncoders.Tests
{
	[TestClass]
	public class UnitTest1
	{
		class TestJsonObject
		{
			public TestJsonObject(string value)
			{
				Value = value;
			}

			public string Value { get; }
		}

		[TestMethod]
		public void TestMin()
		{
			string source = "abc　　𠮟るdef";
			string source2 = "abc\u3000\u3000\ud842\udf9f\u308bdef";

			Assert.AreEqual(source, source2);
			//{
			//	Console.Error.WriteLine("Strings mismatch (check source file encoding).");
			//	return 1;
			//}

			string expected = "{\"Value\":\"" + source + "\"}";

			TestJsonObject test = new TestJsonObject(source);
			string actual = JsonSerializer.Serialize(test, new JsonSerializerOptions
			{
				Encoder =
							  MinimalJsonEncoder.Shared
				//JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			});

			Assert.AreEqual(expected, actual);
			//if (expected != actual)
			//{
			//	//JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			//	// Expected: {"Value":"abc　　𠮟るdef"}, Actual {"Value":"abc\u3000\u3000\uD842\uDF9Fるdef"}

			//	// default
			//	//  Expected: {"Value":"abc　　𠮟るdef"}, Actual {"Value":"abc\u3000\u3000\uD842\uDF9F\u308Bdef"}


			//	Console.Error.WriteLine($"Unexpected output. Expected: {expected}, Actual {actual}");
			//	return 1;
			//}
		}

		[TestMethod]
		public void TestMin2()
		{
			string source = "abc　　𠮟るdef";
			string source2 = "abc\u3000\u3000\ud842\udf9f\u308bdef";

			Assert.AreEqual(source, source2);
			//{
			//	Console.Error.WriteLine("Strings mismatch (check source file encoding).");
			//	return 1;
			//}

			string expected = "{\"Value\":\"" + source + "\"}";

			TestJsonObject test = new TestJsonObject(source);
			string actual = JsonSerializer.Serialize(test, new JsonSerializerOptions
			{
				Encoder =
							  MinimalJsonEncoder.Shared
				//JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			});

			Assert.AreEqual(expected, actual);
			//if (expected != actual)
			//{
			//	//JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			//	// Expected: {"Value":"abc　　𠮟るdef"}, Actual {"Value":"abc\u3000\u3000\uD842\uDF9Fるdef"}

			//	// default
			//	//  Expected: {"Value":"abc　　𠮟るdef"}, Actual {"Value":"abc\u3000\u3000\uD842\uDF9F\u308Bdef"}


			//	Console.Error.WriteLine($"Unexpected output. Expected: {expected}, Actual {actual}");
			//	return 1;
			//}
		}


		[TestMethod]
		public void TestMax()
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("text", "Hello, World!");

			string actual = JsonSerializer.Serialize(data, new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared });

			const string max = "{\"\\u0074\\u0065\\u0078\\u0074\":\"\\u0048\\u0065\\u006C\\u006C\\u006F\\u002C\\u0020\\u0057\\u006F\\u0072\\u006C\\u0064\\u0021\"}";
			Assert.AreEqual(max, actual);
		}

		[TestMethod]
		public void TestMax2()
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("text", "Hello, World!");

			string actual = JsonSerializer.Serialize(data, new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared });

			const string max = "{\"\\u0074\\u0065\\u0078\\u0074\":\"\\u0048\\u0065\\u006C\\u006C\\u006F\\u002C\\u0020\\u0057\\u006F\\u0072\\u006C\\u0064\\u0021\"}";

			Assert.AreEqual(max, actual);
		}

		[TestMethod]
		public void TestMinNewlineTabEtc()
		{
			const string str = "æøå\n\r\t\b\\/\f";

			string actual = JsonSerializer.Serialize(str, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });

			const string min = "\"æøå\\n\\r\\t\\b\\\\/\\f\"";

			Assert.AreEqual(min, actual);

			var back = JsonSerializer.Deserialize<string>(actual);
			Assert.AreEqual(str, back);
		}

		[TestMethod]
		public void TestMaxNewlineTabEtc()
		{
			const string str = "æøå\n\r\t\b\\/\f";

			string actual = JsonSerializer.Serialize(str, new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared });

			const string max = "\"\\u00E6\\u00F8\\u00E5\\u000A\\u000D\\u0009\\u0008\\u005C\\u002F\\u000C\"";

			Assert.AreEqual(max, actual);

			var back = JsonSerializer.Deserialize<string>(actual);
			Assert.AreEqual(str, back);
		}

		[TestMethod]
		public void SurrogatePairMin()
		{
			const string s = "U+10437 (𐐷) to UTF-16: ";
			string json = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });
			const string res = "\"U+10437 (𐐷) to UTF-16: \"";
			Assert.AreEqual(res, json);
			var back = JsonSerializer.Deserialize<string>(json);
			Assert.AreEqual(s, back);
		}

		[TestMethod]
		public void SurrogatePairMax()
		{
			const string s = "U+10437 (𐐷) to UTF-16: ";
			string json = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared });
			const string res = "\"\\u0055\\u002B\\u0031\\u0030\\u0034\\u0033\\u0037\\u0020\\u0028\\uD801\\uDC37\\u0029\\u0020\\u0074\\u006F\\u0020\\u0055\\u0054\\u0046\\u002D\\u0031\\u0036\\u003A\\u0020\"";
			Assert.AreEqual(res, json);
			var back = JsonSerializer.Deserialize<string>(json);
			Assert.AreEqual(s, back);
		}

		[TestMethod]
		public void SurrogatePairMaxAlone()
		{
			const string s = "𐐷";
			string json = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared });
			const string res = "\"\\uD801\\uDC37\"";
			Assert.AreEqual(res, json);
			var back = JsonSerializer.Deserialize<string>(json);
			Assert.AreEqual(s, back);
		}

		[TestMethod]
		public void BrokenSurrogat_OnlyLow()
		{
			//[9]: 55297 '\ud801'
			//[10]: 56375 '\udc37'
			const string s = "\ud801";
			string json = JsonSerializer.Serialize(s, new JsonSerializerOptions());
			const string res = "\"\\uFFFD\""; // replacement char
			Assert.AreEqual(res, json);
			var back = JsonSerializer.Deserialize<string>(json);
			Assert.AreNotEqual(s, back);

			//const string s = "\ud801";
			string json2 = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
			//const string res = "\"\\ud801\\udc37\"";
			Assert.AreEqual(res, json2);
			var back2 = JsonSerializer.Deserialize<string>(json);
			Assert.AreNotEqual(s, back2);

			//const string s = "\ud801";
			string json3 = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared });
			const string resLower = "\"\\uFFFD\"";
			Assert.AreEqual(resLower, json3);
			var back3 = JsonSerializer.Deserialize<string>(json);
			Assert.AreNotEqual(s, back3);

			//const string s = "\ud801";
			string json4 = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });
			const string resReal = "\"\uFFFD\""; // real unescaped replacement char
			Assert.AreEqual(resReal, json4);
			var back4 = JsonSerializer.Deserialize<string>(json);
			Assert.AreNotEqual(s, back4);

		}

		[TestMethod]
		public void BrokenSurrogat_OnlyHigh()
		{
			//[9]: 55297 '\ud801'
			//[10]: 56375 '\udc37'
			const string s = "\udc37";
			string json = JsonSerializer.Serialize(s, new JsonSerializerOptions());
			const string res = "\"\\uFFFD\""; // replacement char
			Assert.AreEqual(res, json);
			var back = JsonSerializer.Deserialize<string>(json);
			Assert.AreNotEqual(s, back);

			//const string s = "\ud801";
			string json2 = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
			//const string res = "\"\\ud801\\udc37\"";
			Assert.AreEqual(res, json2);
			var back2 = JsonSerializer.Deserialize<string>(json);
			Assert.AreNotEqual(s, back2);

			const string resLower = "\"\\uFFFD\"";

			//const string s = "\ud801";
			string json3 = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared });
			//const string res = "\"\\ud801\\udc37\"";
			Assert.AreEqual(resLower, json3);
			var back3 = JsonSerializer.Deserialize<string>(json);
			Assert.AreNotEqual(s, back3);

			//const string s = "\ud801";
			string json4 = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });
			const string resReal = "\"\uFFFD\""; // real unescaped replacement char
			Assert.AreEqual(resReal, json4);
			var back4 = JsonSerializer.Deserialize<string>(json);
			Assert.AreNotEqual(s, back4);

		}

		[TestMethod]
		public void TestMinSlash()
		{
			const string str = "/";

			string actual = JsonSerializer.Serialize(str, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });

			const string min = "\"/\"";

			Assert.AreEqual(min, actual);

			var back = JsonSerializer.Deserialize<string>(actual);
			Assert.AreEqual(str, back);
		}

		[TestMethod]
		public void TestLastFindFirst()
		{
			const string str = "aaaaæ";

			string actual = JsonSerializer.Serialize(str, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });

			const string min = "\"aaaaæ\"";

			Assert.AreEqual(min, actual);

			var back = JsonSerializer.Deserialize<string>(actual);
			Assert.AreEqual(str, back);
		}



		[TestMethod]
		public void InvalidChar()
		{
			for (int i = 0; i <= char.MaxValue; i++)
			{
				char c = (char)i;
				string s = new string(c, 1);
				string json = JsonSerializer.Serialize(s, new JsonSerializerOptions());

				//const string rep = "\"\\uFFFD\""; // replacement char
				const string repReal = "\uFFFD"; // real unescaped replacement char

													 //				Assert.AreEqual(res, json);
				var back = JsonSerializer.Deserialize<string>(json);
				if (char.IsSurrogate(c))
				{
					Assert.AreEqual(repReal, back);
				}
				else
				{
					Assert.AreEqual(s, back);
				}

				json = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
	//			Assert.AreEqual(res, json2);
				back = JsonSerializer.Deserialize<string>(json);
				if (char.IsSurrogate(c))
				{
					Assert.AreEqual(repReal, back);
				}
				else
				{
					Assert.AreEqual(s, back);
				}

				json = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared });
				//const string repLower = "\"\\uFFFD\"";
			//	Assert.AreEqual(resLower, json3);
				back = JsonSerializer.Deserialize<string>(json);
				if (char.IsSurrogate(c))
				{
					Assert.AreEqual(repReal, back);
				}
				else
				{
					Assert.AreEqual(s, back);
				}

				json = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });
				
	//			Assert.AreEqual(resReal, json4);
				back = JsonSerializer.Deserialize<string>(json);
				
				if (char.IsSurrogate(c))
				{
					Assert.AreEqual(repReal, back);
				}
				else
				{
					Assert.AreEqual(s, back);
				}
			}

		}


		[TestMethod]
		public void DelChar()
		{
			//[9]: 55297 '\ud801'
			//[10]: 56375 '\udc37'
			const string s = "\u007f";
			string json = JsonSerializer.Serialize(s, new JsonSerializerOptions());
			const string res = "\"\\u007F\"";
			Assert.AreEqual(res, json);
			var back = JsonSerializer.Deserialize<string>(json);
			Assert.AreEqual(s, back);

			//const string s = "\ud801";
			string json2 = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
			//const string res = "\"\\ud801\\udc37\"";
			Assert.AreEqual(res, json2);
			var back2 = JsonSerializer.Deserialize<string>(json2);
			Assert.AreEqual(s, back2);

			const string resLower = "\"\\u007F\"";

			//const string s = "\ud801";
			string json3 = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared });
			//const string res = "\"\\ud801\\udc37\"";
			Assert.AreEqual(resLower, json3);
			var back3 = JsonSerializer.Deserialize<string>(json3);
			Assert.AreEqual(s, back3);

			//const string s = "\ud801";
			string json4 = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });
			const string resReal = "\"\u007F\""; // real unescaped replacement char
			Assert.AreEqual(resReal, json4);
			var back4 = JsonSerializer.Deserialize<string>(json4);
			Assert.AreEqual(s, back4);

		}

		[TestMethod]
		public void DelChar2()
		{
			const string s = "æøå\u007f";
			string json4 = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });
			const string resReal = "\"æøå\u007F\""; // real unescaped replacement char
			Assert.AreEqual(resReal, json4);
			var back4 = JsonSerializer.Deserialize<string>(json4);
			Assert.AreEqual(s, back4);

		}

		

		[TestMethod]
		public void MinExtraChar()
		{
			const string s = "abc<";
			string json4 = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = new MinimalJsonEncoder(extraAsciiEscapeChars: new[] { '<' }) });
			Assert.AreEqual("\"abc\\u003C\"", json4);
			var back4 = JsonSerializer.Deserialize<string>(json4);
			Assert.AreEqual(s, back4);
		}

		[TestMethod]
		public void MaxWithShortEsc()
		{
			const string s = "a\r\n";
			string json4 = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = new MaximalJsonEncoder(shortEscapes: true) });
			Assert.AreEqual("\"\\u0061\\r\\n\"", json4);
			var back4 = JsonSerializer.Deserialize<string>(json4);
			Assert.AreEqual(s, back4);
		}

		[TestMethod]
		public void MinHtmlScript()
		{
			string data = "</script>";
			string json4 = JsonSerializer.Serialize(data, new JsonSerializerOptions { Encoder = new MinimalJsonEncoder(extraAsciiEscapeChars: new[] { '<' }) });
			Assert.AreEqual("\"\\u003C/script>\"", json4);
			var back4 = JsonSerializer.Deserialize<string>(json4);
			Assert.AreEqual(data, back4);
		}


		[TestMethod]
		public void MinEmo()
		{
			string data = "📲";
			string json4 = JsonSerializer.Serialize(data, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });
			Assert.AreEqual("\"📲\"", json4);
			var back4 = JsonSerializer.Deserialize<string>(json4);
			Assert.AreEqual(data, back4);
		}


		[TestMethod]
		public void PropNames()
		{
			var o = new { abcるdef〱æøå = "abcるdef〱æøå" };
			string json = JsonSerializer.Serialize(o, new JsonSerializerOptions());
			Assert.AreEqual("{\"abc\\u308Bdef\\u3031\\u00E6\\u00F8\\u00E5\":\"abc\\u308Bdef\\u3031\\u00E6\\u00F8\\u00E5\"}", json);

			string json2 = JsonSerializer.Serialize(o, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
			Assert.AreEqual("{\"abcるdef〱æøå\":\"abcるdef〱æøå\"}", json2);

			string json3 = JsonSerializer.Serialize(o, new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared });
			Assert.AreEqual("{\"\\u0061\\u0062\\u0063\\u308B\\u0064\\u0065\\u0066\\u3031\\u00E6\\u00F8\\u00E5\":\"\\u0061\\u0062\\u0063\\u308B\\u0064\\u0065\\u0066\\u3031\\u00E6\\u00F8\\u00E5\"}", json3);

			string json4 = JsonSerializer.Serialize(o, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });
			Assert.AreEqual("{\"abcるdef〱æøå\":\"abcるdef〱æøå\"}", json4); // same as relaxed

		}

	}
}

