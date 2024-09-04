using System.Linq;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlTypes;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Text.Json;
using System.Text.Encodings.Web;


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
	}
}