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
							  new MinimalJsonEncoder()
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

			const string max = "{\"\\u0074\\u0065\\u0078\\u0074\":\"\\u0048\\u0065\\u006c\\u006c\\u006f\\u002c\\u0020\\u0057\\u006f\\u0072\\u006c\\u0064\\u0021\"}";
			Assert.AreEqual(max, actual);
		}

		[TestMethod]
		public void TestMax2()
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("text", "Hello, World!");

			string actual = JsonSerializer.Serialize(data, new JsonSerializerOptions { Encoder = new MaximalJsonEncoder() });

			const string max = "{\"\\u0074\\u0065\\u0078\\u0074\":\"\\u0048\\u0065\\u006c\\u006c\\u006f\\u002c\\u0020\\u0057\\u006f\\u0072\\u006c\\u0064\\u0021\"}";

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

			const string max = "\"\\u00e6\\u00f8\\u00e5\\u000a\\u000d\\u0009\\u0008\\u005c\\u002f\\u000c\"";

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
			const string res = "\"\\u0055\\u002b\\u0031\\u0030\\u0034\\u0033\\u0037\\u0020\\u0028\\ud801\\udc37\\u0029\\u0020\\u0074\\u006f\\u0020\\u0055\\u0054\\u0046\\u002d\\u0031\\u0036\\u003a\\u0020\"";
			Assert.AreEqual(res, json);
			var back = JsonSerializer.Deserialize<string>(json);
			Assert.AreEqual(s, back);
		}

		[TestMethod]
		public void SurrogatePairMaxAlone()
		{
			const string s = "𐐷";
			string json = JsonSerializer.Serialize(s, new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared });
			const string res = "\"\\ud801\\udc37\"";
			Assert.AreEqual(res, json);
			var back = JsonSerializer.Deserialize<string>(json);
			Assert.AreEqual(s, back);
		}
	}
}