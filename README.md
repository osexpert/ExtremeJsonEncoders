# ExtremeJsonEncoders
For System.Text.Json. A MinimalJsonEncoder, that only escape what the RFC require. Also a MaximalJsonEncoder, for fun:-)

```
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine(JsonSerializer.Serialize("\r\n\t\\abcæøå𠮟る𐐷\""));
// "\r\n\t\\abc\u00E6\u00F8\u00E5\uD842\uDF9F\u308B\uD801\uDC37\u0022"
Console.WriteLine(JsonSerializer.Serialize("\r\n\t\\abcæøå𠮟る𐐷\"", new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
// "\r\n\t\\abcæøå\uD842\uDF9Fる\uD801\uDC37\""
Console.WriteLine(JsonSerializer.Serialize("\r\n\t\\abcæøå𠮟る𐐷\"", new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared }));
// "\u000d\u000a\u0009\u005c\u0061\u0062\u0063\u00e6\u00f8\u00e5\ud842\udf9f\u308b\ud801\udc37\u0022"
Console.WriteLine(JsonSerializer.Serialize("\r\n\t\\abcæøå𠮟る𐐷\"", new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared }));
// "\r\n\t\\abcæøå𠮟る𐐷\""
```

Warning: even thou MinimalJsonEncoder produces the smallest json, it is approx 4 times slower than UnsafeRelaxedJsonEscaping (heavily optimized).
So its probably best to use UnsafeRelaxedJsonEscaping unless there is specific requirement for minimally escaped json.

References:
* UnsafeRelaxedJsonEscaping escapes too much #86463: https://github.com/dotnet/runtime/issues/86463
* Default JSON escaping is biased against other languages #86805: https://github.com/dotnet/runtime/issues/86805
* RFC: https://datatracker.ietf.org/doc/html/rfc8259#section-7
