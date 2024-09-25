## 0.0.5
* Minimal: OPT: override FindFirstCharacterToEncodeUtf8. Use SearchValues for net8+.
* Minimal: OPT: FindFirstCharacterToEncode: detect and skip legal surrogates instead of making encoder handle all surrogates.
* Minimal: OPT: After SearchValues, if the first disallowed char is ascii, we can return immediately (we know this is a char that needs escaping)

## 0.0.4
* Minimal: Use upper case hex as default (Shared and ctors). It is defacto standard, it seems.
* Minimal: add ctor args: shortEscapes, lowerCaseHex, extraAsciiEscapeChars
* Minimal: FindFirstCharacterToEncode: use SearchValues on net8+ to skip leading non-escaped ascii chars faster
* Maximal: add ctor args: shortEscapes, lowerCaseHex

## 0.0.3
* Minimal: increase performance by used code from dotnet runtime.
* Both: add bool parameter in ctor, lowerCaseHex. Default is true (also true via the Shared-instances).
* Both: Handle invalid chars (partial surrogates). They will now be replaced with ReplacementChar, just like the other encoders do. This is a side effect of using code from dotnet runtime.

## 0.0.2
* Minimal: FIX: encoded \n,\t,\r etc. (control characters that has a two-character sequence) fully escaped (\u000#)
* Maximal: FIX: encoded \n incorrectly (\u000: instead of \u000a)

## 0.0.1
* First release.

