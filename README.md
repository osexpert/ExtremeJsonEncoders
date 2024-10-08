# ExtremeJsonEncoders
For System.Text.Json. A MinimalJsonEncoder, that only escape what the RFC require. Also a MaximalJsonEncoder, for fun:-)

```
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine(JsonSerializer.Serialize("\r\n\t\\abcæøå𠮟る𐐷\""));
// "\r\n\t\\abc\u00E6\u00F8\u00E5\uD842\uDF9F\u308B\uD801\uDC37\u0022"
Console.WriteLine(JsonSerializer.Serialize("\r\n\t\\abcæøå𠮟る𐐷\"", new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
// "\r\n\t\\abcæøå\uD842\uDF9Fる\uD801\uDC37\""
Console.WriteLine(JsonSerializer.Serialize("\r\n\t\\abcæøå𠮟る𐐷\"", new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared }));
// "\u000D\u000A\u0009\u005C\u0061\u0062\u0063\u00E6\u00F8\u00E5\uD842\uDF9F\u308B\uD801\uDC37\u0022"
Console.WriteLine(JsonSerializer.Serialize("\r\n\t\\abcæøå𠮟る𐐷\"", new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared }));
// "\r\n\t\\abcæøå𠮟る𐐷\""
```

Performance (since 0.0.3):
When there is a lot of ascii and little need to escape, MinimalJsonEncoder is comparable to UnsafeRelaxedJsonEscaping.
When a lot of non-ascii, MinimalJsonEncoder can use 1/2 the time of UnsafeRelaxedJsonEscaping and 1/3 of the time of default encoder.

References:
* UnsafeRelaxedJsonEscaping escapes too much #86463: https://github.com/dotnet/runtime/issues/86463
* Default JSON escaping is biased against other languages #86805: https://github.com/dotnet/runtime/issues/86805
* [API Proposal]: UnicodeJsonEncoder: https://github.com/dotnet/runtime/issues/87153
* RFC: https://datatracker.ietf.org/doc/html/rfc8259#section-7
