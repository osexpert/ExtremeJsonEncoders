# ExtremeJsonEncoders
For System.Text.Json. A MinimalJsonEncoder, that only escape what the RFC require. Also a MaximalJsonEncoder, for fun:-)

```
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine(JsonSerializer.Serialize("abcæøå𠮟る"));
// "abc\u00E6\u00F8\u00E5\uD842\uDF9F\u308B"
Console.WriteLine(JsonSerializer.Serialize("abcæøå𠮟る", new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
// "abcæøå\uD842\uDF9Fる"
Console.WriteLine(JsonSerializer.Serialize("abcæøå𠮟る", new JsonSerializerOptions { Encoder = MaximalJsonEncoder.Shared }));
// "\u0061\u0062\u0063\u00e6\u00f8\u00e5\ud842\udf9f\u308b"
Console.WriteLine(JsonSerializer.Serialize("abcæøå𠮟る", new JsonSerializerOptions { Encoder = MinimalJsonEncoder.Shared }));
// "abcæøå𠮟る"
```

References:
* UnsafeRelaxedJsonEscaping escapes too much #86463: https://github.com/dotnet/runtime/issues/86463
* Default JSON escaping is biased against other languages #86805: https://github.com/dotnet/runtime/issues/86805
* RFC: https://datatracker.ietf.org/doc/html/rfc8259#section-7
