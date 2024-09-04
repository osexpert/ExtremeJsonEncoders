# ExtremeJsonEncoders
For System.Text.Json. A MinimalJsonEncoder, that only escape what the RFC require. Also a MaximalJsonEncoder, for fun:-)

References:
* UnsafeRelaxedJsonEscaping escapes too much #86463: https://github.com/dotnet/runtime/issues/86463
* Default JSON escaping is biased against other languages #86805: https://github.com/dotnet/runtime/issues/86805
* RFC: https://datatracker.ietf.org/doc/html/rfc8259#section-7