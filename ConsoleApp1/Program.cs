﻿using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text;
using ExtremeJsonEncoders;

namespace ConsoleApp1
{
    class TestJsonObject
    {
        public TestJsonObject(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    internal class Program
    {
        static int Main(string[] args)
        {
			Console.OutputEncoding = Encoding.UTF8;
			Console.WriteLine(JsonSerializer.Serialize("\r\n\t\\abcæøå𠮟る𐐷\""));
			// "\r\n\t\\abc\u00E6\u00F8\u00E5\uD842\uDF9F\u308B\uD801\uDC37\u0022"
			Console.WriteLine(JsonSerializer.Serialize("\r\n\t\\abcæøå𠮟る𐐷\"", new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
			// "\r\n\t\\abcæøå\uD842\uDF9Fる\uD801\uDC37\""
			Console.WriteLine(JsonSerializer.Serialize("\r\n\t\\abcæøå𠮟る𐐷\"", new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared }));
			// "\u000d\u000a\u0009\u005c\u0061\u0062\u0063\u00e6\u00f8\u00e5\ud842\udf9f\u308b\ud801\udc37\u0022"
			Console.WriteLine(JsonSerializer.Serialize("\r\n\t\\abcæøå𠮟る𐐷\"", new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared }));
			// "\r\n\t\\abcæøå𠮟る𐐷\""


			Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("text", "Hello, World!");

            string actual = JsonSerializer.Serialize(data, new JsonSerializerOptions { Encoder = new MaximalJsonEncoder() });
            Console.WriteLine(actual);

            const string max = "{\"\\u0074\\u0065\\u0078\\u0074\":\"\\u0048\\u0065\\u006c\\u006c\\u006f\\u002c\\u0020\\u0057\\u006f\\u0072\\u006c\\u0064\\u0021\"}";
            if (actual != max)
                throw new Exception();

			
            return 0;
        }

        static int mintest()
        { 
            Console.OutputEncoding = Encoding.UTF8;

            string source = "abc　　𠮟るdef";
            string source2 = "abc\u3000\u3000\ud842\udf9f\u308bdef";

            if (source != source2)
            {
                Console.Error.WriteLine("Strings mismatch (check source file encoding).");
                return 1;
            }

            string expected = "{\"Value\":\"" + source + "\"}";

            TestJsonObject test = new TestJsonObject(source);
            string actual = JsonSerializer.Serialize(test, new JsonSerializerOptions
            {
                Encoder = 
                //MinimalJsonEncoder.Shared
                              JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }) ;

            if (expected != actual)
            {
                //JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                // Expected: {"Value":"abc　　𠮟るdef"}, Actual {"Value":"abc\u3000\u3000\uD842\uDF9Fるdef"}

                // default
                //  Expected: {"Value":"abc　　𠮟るdef"}, Actual {"Value":"abc\u3000\u3000\uD842\uDF9F\u308Bdef"}


                Console.Error.WriteLine($"Unexpected output. Expected: {expected}, Actual {actual}");
                return 1;
            }

            // minimal
            return 0;
        }
    }
}
