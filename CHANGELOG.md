## 0.0.4
* Use upper case hex as default (Shared and ctors). It is defacto standard, it seems.
* Minimal: add ctor options: shortEscapes, lowerCaseHex, extraAsciiEscapeChars
* Maximal: add ctor options: shortEscapes, lowerCaseHex
* FindFirstCharacterToEncode: user SearchValues on net8+ to skip leading non-escaped ascii chars faster

## 0.0.3
* MinimalJsonEncoder: increase performance by used code from dotnet runtime.
* Both: add bool parameter in ctor, lowerCaseHex. Default is true (also true via the Shared-instances).
* Handle invalid chars (partial surrogates). They will now be replaced with Rune.ReplacementChar, just like the other encoders do. This is a side effect of using code from dotnet runtime.

## 0.0.2
* FIX: MinimalJsonEncoder encoded \n,\t,\r,etc. (control characters that has a two-character sequence) fully escaped (\u000#)
* FIX: MaximalJsonEncoder encoded \n incorrectly (\u000: instead of \u000a)

## 0.0.1
* First release.

