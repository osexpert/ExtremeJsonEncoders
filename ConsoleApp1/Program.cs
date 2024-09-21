using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text;
using ExtremeJsonEncoders;
using System.Diagnostics;

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
			//StringBuilder sb2 = new();
			//char? prev = null;
			//for (int i = 0; i <= char.MaxValue; i++)
			//{
			//	char c = (char)i;
			//	if (char.IsSurrogate(c))
			//	{
			//		sb2.Append(c);

			//		if (c == prev + 1)
			//		{
			//			//Console.WriteLine($"...");
			//		}
			//		else
			//		{
			//			Console.WriteLine($"Surrogate: {(int)c:x}");
			//		}


			//		prev = c;
			//	}
			//}

			//Console.WriteLine($"Surrogate: {(int)prev!.Value:x}");

			//		Surrogate: d800
			//      Surrogate: dfff

			//		https://www.cl.cam.ac.uk/~mgk25/ucs/examples/UTF-8-test.txt
			//		https://github.com/bits/UTF-8-Unicode-Test-Documents/blob/master/UTF-8_sequence_unseparated/utf8_sequence_0-0x2ffff_including-unassigned_including-unprintable-replaced_unseparated.txt
			//
			//
			//var file = @"d:\ascii only test.txt";
			//var file = @"d:\UTF-8-test.txt";
			var file = @"d:\only ø.txt";
			//@"d:\utf8_sequence_0-0x2ffff_including-unassigned_including-unprintable-replaced_unseparated.txt"
			//@"d:\ascii test separated by non.txt"

			//var file = @"d:\utf8_sequence_0-0x2ffff_including-unassigned_including-unprintable-replaced_unseparated.txt";
			//var file = @"d:\ascii test separated by non.txt";

			Console.WriteLine("file: " + file);

			var txt = File.ReadAllText(file
				,
				Encoding.UTF8);// d:\utf8_sequence_0-0x2ffff_including-unassigned_including-unprintable-replaced_unseparated.txt", Encoding.UTF8);//


			//			StringBuilder sb = new();

			//		string gg = "\u323AF";

			var warm1 = JsonSerializer.Serialize(txt, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });
			var warm2 = JsonSerializer.Serialize(txt, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

			Console.OutputEncoding = Encoding.UTF8;
			var s = new Stopwatch();


			s.Restart();
			for (int i = 0; i < 1000; i++)
			{
				var serr = JsonSerializer.Serialize(txt, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });
				if (JsonSerializer.Deserialize<string>(serr) != txt)
					throw new Exception();
			}
			s.Stop();
			Console.WriteLine("min " + s.ElapsedMilliseconds);

			//s.Restart();
			//for (int i = 0; i < 1000; i++)
			//{
			//	var serr = JsonSerializer.Serialize(txt);
			//	if (JsonSerializer.Deserialize<string>(serr) != txt)
			//		throw new Exception();
			//}
			//s.Stop();
			//Console.WriteLine("std " + s.ElapsedMilliseconds);

			s.Restart();
			for (int i = 0; i < 1000; i++)
			{
				var serr = JsonSerializer.Serialize(txt, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
				if (JsonSerializer.Deserialize<string>(serr) != txt)
					throw new Exception();
			}
			s.Stop();
			Console.WriteLine("unrelx " + s.ElapsedMilliseconds);

			s.Restart();
			for (int i = 0; i < 1000; i++)
			{
				var serr = JsonSerializer.Serialize(txt, new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared });
				if (JsonSerializer.Deserialize<string>(serr) != txt)
					throw new Exception();
			}
			s.Stop();
			Console.WriteLine("min " + s.ElapsedMilliseconds);

			s.Restart();
			for (int i = 0; i < 1000; i++)
			{
				var serr = JsonSerializer.Serialize(txt, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
				if (JsonSerializer.Deserialize<string>(serr) != txt)
					throw new Exception();
			}
			s.Stop();
			Console.WriteLine("unrelx " + s.ElapsedMilliseconds);

			//s.Restart();
			//for (int i = 0; i < 100; i++)
			//{
			//	var serr = JsonSerializer.Serialize(txt, new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared });
			//	if (JsonSerializer.Deserialize<string>(serr) != txt)
			//		throw new Exception();
			//}
			//s.Stop();
			//Console.WriteLine("max " + s.ElapsedMilliseconds);


			return 0;

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

            string actual = JsonSerializer.Serialize(data, new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared });
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
