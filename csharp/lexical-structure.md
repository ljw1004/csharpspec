# Lexical structure

## Programs

A C# *__program__* consists of one or more *__source files__*, known formally as *__compilation units__* (§*__9.1__*). A source file is an ordered sequence of Unicode characters. Source files typically have a one-to-one correspondence with files in a file system, but this correspondence is not required. For maximal portability, it is recommended that files in a file system be encoded with the UTF-8 encoding.

Conceptually speaking, a program is compiled using three steps:

1. Transformation, which converts a file from a particular character repertoire and encoding scheme into a sequence of Unicode characters.
2. Lexical analysis, which translates a stream of Unicode input characters into a stream of tokens.
3. Syntactic analysis, which translates the stream of tokens into executable code.

## Grammars

This specification presents the syntax of the C# programming language using two grammars. The *__lexical grammar__* (§2.2.2) defines how Unicode characters are combined to form line terminators, white space, comments, tokens, and pre-processing directives. The *__syntactic grammar__* (§2.2.3) defines how the tokens resulting from the lexical grammar are combined to form C# programs.

### Grammar notation

The lexical and syntactic grammars are presented using *__grammar productions__*. Each grammar production defines a non-terminal symbol and the possible expansions of that non-terminal symbol into sequences of non-terminal or terminal symbols. In grammar productions, *non-terminal* symbols are shown in italic type, and `terminal` symbols are shown in a fixed-width font.

The first line of a grammar production is the name of the non-terminal symbol being defined, followed by a colon. Each successive indented line contains a possible expansion of the non-terminal given as a sequence of non-terminal or terminal symbols. For example, the production:

<pre>
<i>while-statement:</i>
   <b>while   (</b>   <i>boolean-expression</i>   <b>)</b>   <i>embedded-statement</i>
</pre>

defines a *while-statement* to consist of the token `while`, followed by the token "`(`", followed by a *boolean-expression*, followed by the token "`)`", followed by an *embedded-statement*.

When there is more than one possible expansion of a non-terminal symbol, the alternatives are listed on separate lines. For example, the production:

<i>statement-list:
statement
statement-list   statement</i>

defines a *statement-list* to either consist of a *statement* or consist of a *statement-list* followed by a *statement*. In other words, the definition is recursive and specifies that a statement list consists of one or more statements.

A subscripted suffix "<sub>opt</sub>" is used to indicate an optional symbol. The production:

<i>block:
   __{__   statement-list<sub>opt</sub>   __}__</i>

is shorthand for:

<i>block:
   __{__   __}__
   __{__   statement-list      __}__</i>

and defines a *block* to consist of an optional *statement-list* enclosed in "`{`" and "`}`" tokens.

Alternatives are normally listed on separate lines, though in cases where there are many alternatives, the phrase "one of" may precede a list of expansions given on a single line. This is simply shorthand for listing each of the alternatives on a separate line. For example, the production:

<i>real-type-suffix:  one of
   __F  f  D  d  M  m__</i>

is shorthand for:

<i>real-type-suffix:
   __F__   __
f__   __
D__   __
d__   __
M__   __
m__</i>

### Lexical grammar

The lexical grammar of C# is presented in §2.3, §2.4, and §2.5. The terminal symbols of the lexical grammar are the characters of the Unicode character set, and the lexical grammar specifies how characters are combined to form tokens (§2.4), white space (§2.3.3), comments (§2.3.2), and pre-processing directives (§2.5).

Every source file in a C# program must conform to the *input* production of the lexical grammar (§2.3).

### Syntactic grammar

The syntactic grammar of C# is presented in the chapters and appendices that follow this chapter. The terminal symbols of the syntactic grammar are the tokens defined by the lexical grammar, and the syntactic grammar specifies how tokens are combined to form C# programs.

Every source file in a C# program must conform to the *compilation-unit* production of the syntactic grammar (§9.1).

## Lexical analysis

The *input* production defines the lexical structure of a C# source file. Each source file in a C# program must conform to this lexical grammar production.

<i>input:
input-section<sub>opt</sub></i>

<i>input-section:
input-section-part
input-section   input-section-part</i>

<i>input-section-part:
input-elements<sub>opt</sub>   new-line
pp-directive</i>

<i>input-elements:
input-element
input-elements   input-element</i>

<i>input-element:
whitespace
comment
token</i>

Five basic elements make up the lexical structure of a C# source file: Line terminators (§2.3.1), white space (§2.3.3), comments (§2.3.2), tokens (§2.4), and pre-processing directives (§2.5). Of these basic elements, only tokens are significant in the syntactic grammar of a C# program (§2.2.3).

The lexical processing of a C# source file consists of reducing the file into a sequence of tokens which becomes the input to the syntactic analysis. Line terminators, white space, and comments can serve to separate tokens, and pre-processing directives can cause sections of the source file to be skipped, but otherwise these lexical elements have no impact on the syntactic structure of a C# program.

When several lexical grammar productions match a sequence of characters in a source file, the lexical processing always forms the longest possible lexical element. For example, the character sequence `//` is processed as the beginning of a single-line comment because that lexical element is longer than a single `/` token.

### Line terminators

Line terminators divide the characters of a C# source file into lines.

<i>new-line:
Carriage return character (   __U+000D__)
Line feed character (   __U+000A__)
Carriage return character (   __U+000D__) followed by line feed character (   __U+000A__)
Next line character (   __U+0085__)
Line separator character (   __U+2028__)
Paragraph separator character (   __U+2029__)</i>

For compatibility with source code editing tools that add end-of-file markers, and to enable a source file to be viewed as a sequence of properly terminated lines, the following transformations are applied, in order, to every source file in a C# program:

-  If the last character of the source file is a Control-Z character (`U+001A`), this character is deleted.
-  A carriage-return character (`U+000D`) is added to the end of the source file if that source file is non-empty and if the last character of the source file is not a carriage return (`U+000D`), a line feed (`U+000A`), a line separator (`U+2028`), or a paragraph separator (`U+2029`).

### Comments

Two forms of comments are supported: single-line comments and delimited comments. *__Single-line comments__* start with the characters `//` and extend to the end of the source line. *__Delimited comments__* start with the characters `/*` and end with the characters `*/`. Delimited comments may span multiple lines.

<i>comment:
single-line-comment
delimited-comment</i>

<i>single-line-comment:
   __//__   input-characters<sub>opt</sub></i>

<i>input-characters:
input-character
input-characters   input-character</i>

<i>input-character:
Any Unicode character except a new-line-character</i>

<i>new-line-character:
Carriage return character (   __U+000D__)
Line feed character (   __U+000A__)
Next line character (   __U+0085__)
Line separator character (   __U+2028__)
Paragraph separator character (   __U+2029__)</i>

<i>delimited-comment:
   __/*__   delimited-comment-text<sub>opt</sub>   asterisks      __/__</i>

<i>delimited-comment-text:
delimited-comment-section
delimited-comment-text   delimited-comment-section</i>

<i>delimited-comment-section:
   __/__
asterisks<sub>opt</sub>   not-slash-or-asterisk</i>

<i>asterisks:
   __*__
asterisks      __*__</i>

<i>not-slash-or-asterisk:
Any Unicode character except    __/__ or    __*__</i>

Comments do not nest. The character sequences `/*` and `*/` have no special meaning within a `//` comment, and the character sequences `//` and `/*` have no special meaning within a delimited comment.

Comments are not processed within character and string literals.

The example

```csharp
/* Hello, world program
   This program writes "hello, world" to the console
*/
class Hello
{
    static void Main() {
        System.Console.WriteLine("hello, world");
    }
}
```

includes a delimited comment.

The example

```csharp
// Hello, world program
// This program writes "hello, world" to the console
//
class Hello // any name will do for this class
{
    static void Main() { // this method must be named "Main"
        System.Console.WriteLine("hello, world");
    }
}
```

shows several single-line comments.

### White space

White space is defined as any character with Unicode class Zs (which includes the space character) as well as the horizontal tab character, the vertical tab character, and the form feed character.

<i>whitespace:
Any character with Unicode class Zs
Horizontal tab character (   __U+0009__)
Vertical tab character (   __U+000B__)
Form feed character (   __U+000C__)</i>

## Tokens

There are several kinds of tokens: identifiers, keywords, literals, operators, and punctuators. White space and comments are not tokens, though they act as separators for tokens.

<i>token:
identifier
keyword
integer-literal
real-literal
character-literal
string-literal
operator-or-punctuator</i>

### Unicode character escape sequences

A Unicode character escape sequence represents a Unicode character. Unicode character escape sequences are processed in identifiers (§2.4.2), character literals (§2.4.4.4), and regular string literals (§2.4.4.5). A Unicode character escape is not processed in any other location (for example, to form an operator, punctuator, or keyword).

<i>unicode-escape-sequence:
   __\u__   hex-digit   hex-digit   hex-digit   hex-digit
   __\U__   hex-digit   hex-digit   hex-digit  hex-digit   hex-digit   hex-digit   hex-digit   hex-digit</i>

A Unicode escape sequence represents the single Unicode character formed by the hexadecimal number following the "`\u`" or "`\U`" characters. Since C# uses a 16-bit encoding of Unicode code points in characters and string values, a Unicode character in the range U+10000 to U+10FFFF is not permitted in a character literal and is represented using a Unicode surrogate pair in a string literal. Unicode characters with code points above 0x10FFFF are not supported.

Multiple translations are not performed. For instance, the string literal "`\u005Cu005C`" is equivalent to "`\u005C`" rather than "`\`". The Unicode value `\u005C` is the character "`\`".

The example

```csharp
class Class1
{
    static void Test(bool \u0066) {
        char c = '\u0066';
        if (\u0066)
            System.Console.WriteLine(c.ToString());
    }        
}
```

shows several uses of `\u0066`, which is the escape sequence for the letter "`f`". The program is equivalent to

```csharp
class Class1
{
    static void Test(bool f) {
        char c = 'f';
        if (f)
            System.Console.WriteLine(c.ToString());
    }        
}
```

### Identifiers

The rules for identifiers given in this section correspond exactly to those recommended by the Unicode Standard Annex 31, except that underscore is allowed as an initial character (as is traditional in the C programming language), Unicode escape sequences are permitted in identifiers, and the "`@`" character is allowed as a prefix to enable keywords to be used as identifiers.

<i>identifier:
available-identifier
   __@__   identifier-or-keyword</i>

<i>available-identifier:
An identifier-or-keyword that is not a keyword</i>

<i>identifier-or-keyword:
identifier-start-character   identifier-part-characters<sub>opt</sub></i>

<i>identifier-start-character:
letter-character
   _____ (the underscore character    __U+005F__)</i>

<i>identifier-part-characters:
identifier-part-character
identifier-part-characters   identifier-part-character</i>

<i>identifier-part-character:
letter-character
decimal-digit-character
connecting-character
combining-character
formatting-character</i>

<i>letter-character:
A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nl 
A unicode-escape-sequence representing a character of classes Lu, Ll, Lt, Lm, Lo, or Nl</i>

<i>combining-character:
A Unicode character of classes Mn or Mc 
A unicode-escape-sequence representing a character of classes Mn or Mc</i>

<i>decimal-digit-character:
A Unicode character of the class Nd 
A unicode-escape-sequence representing a character of the class Nd</i>

<i>connecting-character:  
A Unicode character of the class Pc
A unicode-escape-sequence representing a character of the class Pc</i>

<i>formatting-character:  
A Unicode character of the class Cf
A unicode-escape-sequence representing a character of the class Cf</i>

For information on the Unicode character classes mentioned above, see The Unicode Standard, Version 3.0, section 4.5.

Examples of valid identifiers include "`identifier1`", "`_identifier2`", and "`@if`".

An identifier in a conforming program must be in the canonical format defined by Unicode Normalization Form C, as defined by Unicode Standard Annex 15. The behavior when encountering an identifier not in Normalization Form C is implementation-defined; however, a diagnostic is not required.

The prefix "`@`" enables the use of keywords as identifiers, which is useful when interfacing with other programming languages. The character `@` is not actually part of the identifier, so the identifier might be seen in other languages as a normal identifier, without the prefix. An identifier with an `@` prefix is called a *__verbatim identifier__*. Use of the `@` prefix for identifiers that are not keywords is permitted, but strongly discouraged as a matter of style.

The example:

```csharp
class @class
{
    public static void @static(bool @bool) {
        if (@bool)
            System.Console.WriteLine("true");
        else
            System.Console.WriteLine("false");
    }    
}

class Class1
{
    static void M() {
        cl\u0061ss.st\u0061tic(true);
    }
}
```

defines a class named "`class`" with a static method named "`static`" that takes a parameter named "`bool`". Note that since Unicode escapes are not permitted in keywords, the token "`cl\u0061ss`" is an identifier, and is the same identifier as "`@class`".

Two identifiers are considered the same if they are identical after the following transformations are applied, in order:

-  The prefix "`@`", if used, is removed.
-  Each *unicode-escape-sequence* is transformed into its corresponding Unicode character.
-  Any *formatting-character* s are removed.

Identifiers containing two consecutive underscore characters (`U+005F`) are reserved for use by the implementation. For example, an implementation might provide extended keywords that begin with two underscores.

### Keywords

A *__keyword__* is an identifier-like sequence of characters that is reserved, and cannot be used as an identifier except when prefaced by the `@` character.

<i>keyword:  one of
   __abstract__   ____   __    as__   ____   ____   ____   __    base__   ____   ____   __    bool__   ____   ____   __    break__   __
byte__   ____   ____   __    case__   ____   ____   __    catch__   ____   ____   __    char__   ____   ____   __    checked__   __
class__   ____   ____   __    const__   ____   ____   __    continue__   ____   __    decimal__   ____   __    default__   __
delegate__   ____   __    do__   ____   ____   ____   __    double__   ____   __    else__   ____   ____   __    enum__   __
event__   ____   ____   __    explicit__   ____   __    extern__   ____   __    false__   ____   ____   __    finally__   __
fixed__   ____   ____   __    float__   ____   ____   __    for__   ____   ____   __    foreach__   ____   __    goto__   __
if__   ____   ____   ____   __    implicit__   ____   __    in__   ____   ____   ____   __    int__   ____   ____   __    interface__   __
internal__   ____   __    is__   ____   ____   ____   __    lock__   ____   ____   __    long__   ____   ____   __    namespace__   __
new__   ____   ____   __    null__   ____   ____   __    object__   ____   __    operator__   ____   __    out__   __
override__   ____   __    params__   ____   __    private__   ____   __    protected__   __    public__   __
readonly__   ____   __    ref__   ____   ____   __    return__   ____   __    sbyte__   ____   ____   __    sealed__   __
short__   ____   ____   __    sizeof__   ____   __    stackalloc__   __    static__   ____   __    string__   __
struct__   ____   __    switch__   ____   __    this__   ____   ____   __    throw__   ____   ____   __    true__   __
try__   ____   ____   __    typeof__   ____   __    uint__   ____   ____   __    ulong__   ____   ____   __    unchecked__   __
unsafe__   ____   __    ushort__   ____   __    using__   ____   ____   __    virtual__   ____   __    void__   __
volatile__   ____   __    while__</i>

In some places in the grammar, specific identifiers have special meaning, but are not keywords. Such identifiers are sometimes referred to as "contextual keywords". For example, within a property declaration, the "`get`" and "`set`" identifiers have special meaning (§10.7.2). An identifier other than `get` or `set` is never permitted in these locations, so this use does not conflict with a use of these words as identifiers. In other cases, such as with the identifier "`var`" in implicitly typed local variable declarations (§8.5.1), a contectual keyword can conflict with declared names. In such cases, the declared name takes precedence over the use of the identifier as a contextual keyword.

### Literals

A *__literal__* is a source code representation of a value.

<i>literal:
boolean-literal
integer-literal
real-literal
character-literal
string-literal
null-literal</i>

#### Boolean literals

There are two boolean literal values: `true` and `false`.

<i>boolean-literal:
   __true__   __
false__</i>

The type of a *boolean-literal* is `bool`.

#### Integer literals

Integer literals are used to write values of types `int`, `uint`, `long`, and `ulong`. Integer literals have two possible forms: decimal and hexadecimal.

<i>integer-literal:
decimal-integer-literal
hexadecimal-integer-literal</i>

<i>decimal-integer-literal:
decimal-digits   integer-type-suffix<sub>opt</sub></i>

<i>decimal-digits:
decimal-digit
decimal-digits   decimal-digit</i>

<i>decimal-digit:  one of
   __0  1  2  3  4  5  6  7  8  9__</i>

<i>integer-type-suffix:  one of
   __U  u  L  l  UL  Ul  uL  ul  LU  Lu  lU  lu__</i>

<i>hexadecimal-integer-literal:
   __0x__   hex-digits   integer-type-suffix<sub>opt</sub>
   __0X__   hex-digits   integer-type-suffix<sub>opt</sub></i>

<i>hex-digits:
hex-digit
hex-digits   hex-digit</i>

<i>hex-digit:  one of
   __0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F  a  b  c  d  e  f__</i>

The type of an integer literal is determined as follows:

-  If the literal has no suffix, it has the first of these types in which its value can be represented: `int`, `uint`, `long`, `ulong`.
-  If the literal is suffixed by `U` or `u`, it has the first of these types in which its value can be represented: `uint`, `ulong`.
-  If the literal is suffixed by `L` or `l`, it has the first of these types in which its value can be represented: `long`, `ulong`.
-  If the literal is suffixed by `UL`, `Ul`, `uL`, `ul`, `LU`, `Lu`, `lU`, or `lu`, it is of type `ulong`.

If the value represented by an integer literal is outside the range of the `ulong` type, a compile-time error occurs.

As a matter of style, it is suggested that "`L`" be used instead of "`l`" when writing literals of type `long`, since it is easy to confuse the letter "`l`" with the digit "`1`".

To permit the smallest possible `int` and `long` values to be written as decimal integer literals, the following two rules exist:

-  When a *decimal-integer-literal* with the value 2147483648 (2<sup>31</sup>) and no *integer-type-suffix* appears as the token immediately following a unary minus operator token (§7.7.2), the result is a constant of type `int` with the value ?2147483648 (?2<sup>31</sup>). In all other situations, such a *decimal-integer-literal* is of type `uint`.
-  When a *decimal-integer-literal* with the value 9223372036854775808 (2<sup>63</sup>) and no *integer-type-suffix* or the *integer-type-suffix*`L` or `l` appears as the token immediately following a unary minus operator token (§7.7.2), the result is a constant of type `long` with the value ?9223372036854775808 (?2<sup>63</sup>). In all other situations, such a *decimal-integer-literal* is of type `ulong`.

#### Real literals

Real literals are used to write values of types `float`, `double`, and `decimal`.

<i>real-literal:
decimal-digits      __.__   decimal-digits   exponent-part<sub>opt</sub>   real-type-suffix<sub>opt</sub>
   __.__   decimal-digits   exponent-part<sub>opt</sub>   real-type-suffix<sub>opt</sub>
decimal-digits   exponent-part   real-type-suffix<sub>opt</sub>
decimal-digits   real-type-suffix</i>

<i>exponent-part:
   __e__   sign<sub>opt</sub>   decimal-digits
   __E__   sign<sub>opt</sub>   decimal-digits</i>

<i>sign:  one of
   __+  -__</i>

<i>real-type-suffix:  one of
   __F  f  D  d  M  m__</i>

If no *real-type-suffix* is specified, the type of the real literal is `double`. Otherwise, the real type suffix determines the type of the real literal, as follows:

-  A real literal suffixed by `F` or `f` is of type `float`. For example, the literals `1f`, `1.5f`, `1e10f`, and `123.456F` are all of type `float`.
-  A real literal suffixed by `D` or `d` is of type `double`. For example, the literals `1d`, `1.5d`, `1e10d`, and `123.456D` are all of type `double`.
-  A real literal suffixed by `M` or `m` is of type `decimal`. For example, the literals `1m`, `1.5m`, `1e10m`, and `123.456M` are all of type `decimal`. This literal is converted to a `decimal` value by taking the exact value, and, if necessary, rounding to the nearest representable value using banker's rounding (§4.1.7). Any scale apparent in the literal is preserved unless the value is rounded or the value is zero (in which latter case the sign and scale will be 0). Hence, the literal `2.900m` will be parsed to form the decimal with sign `0`, coefficient `2900`, and scale `3`.

If the specified literal cannot be represented in the indicated type, a compile-time error occurs.

The value of a real literal of type `float` or `double` is determined by using the IEEE "round to nearest" mode.

Note that in a real literal, decimal digits are always required after the decimal point. For example, `1.3F` is a real literal but `1.F` is not.

#### Character literals

A character literal represents a single character, and usually consists of a character in quotes, as in `'a'`.

<i>character-literal:
   __'__   character      __'__</i>

<i>character:
single-character
simple-escape-sequence
hexadecimal-escape-sequence
unicode-escape-sequence</i>

<i>single-character:
 Any character except    __'__ (   __U+0027__),    __\__ (   __U+005C__), and new-line-character</i>

<i>simple-escape-sequence:  one of
   __\'  \"  \\  \0  \a  \b  \f  \n  \r  \t  \v__</i>

<i>hexadecimal-escape-sequence:
   __\x__   hex-digit   hex-digit<sub>opt</sub>   hex-digit<sub>opt</sub>   hex-digit<sub>opt</sub></i>

A character that follows a backslash character (`\`) in a *character *must be one of the following characters: `'`, `"`, `\`, `0`, `a`, `b`, `f`, `n`, `r`, `t`, `u`, `U`, `x`, `v`. Otherwise, a compile-time error occurs.

A hexadecimal escape sequence represents a single Unicode character, with the value formed by the hexadecimal number following "`\x`".

If the value represented by a character literal is greater than `U+FFFF`, a compile-time error occurs.

A Unicode character escape sequence (§2.4.1) in a character literal must be in the range `U+0000` to `U+FFFF`.

A simple escape sequence represents a Unicode character encoding, as described in the table below.


| __Escape sequence__ | __Character name__ | __Unicode encoding__ | 
| `\'` | Single quote | `0x0027` | 
| `\"` | Double quote | `0x0022` | 
| `\\` | Backslash | `0x005C` | 
| `\0` | Null | `0x0000` | 
| `\a` | Alert | `0x0007` | 
| `\b` | Backspace | `0x0008` | 
| `\f` | Form feed | `0x000C` | 
| `\n` | New line | `0x000A` | 
| `\r` | Carriage return | `0x000D` | 
| `\t` | Horizontal tab | `0x0009` | 
| `\v` | Vertical tab | `0x000B` | 

The type of a *character-literal* is `char`.

#### String literals

C# supports two forms of string literals: *__regular string literals__* and *__verbatim string literals__*.

A regular string literal consists of zero or more characters enclosed in double quotes, as in `"hello"`, and may include both simple escape sequences (such as `\t` for the tab character), and hexadecimal and Unicode escape sequences.

A verbatim string literal consists of an `@` character followed by a double-quote character, zero or more characters, and a closing double-quote character. A simple example is `@"hello"`. In a verbatim string literal, the characters between the delimiters are interpreted verbatim, the only exception being a *quote-escape-sequence*. In particular, simple escape sequences, and hexadecimal and Unicode escape sequences are not processed in verbatim string literals. A verbatim string literal may span multiple lines.

<i>string-literal:
regular-string-literal
verbatim-string-literal</i>

<i>regular-string-literal:
   __"__   regular-string-literal-characters<sub>opt</sub>   __"__</i>

<i>regular-string-literal-characters:
regular-string-literal-character
regular-string-literal-characters   regular-string-literal-character</i>

<i>regular-string-literal-character:
single-regular-string-literal-character
simple-escape-sequence
hexadecimal-escape-sequence
unicode-escape-sequence</i>

<i>single-regular-string-literal-character:
Any character except    __"__ (   __U+0022__),    __\__ (   __U+005C__), and new-line-character</i>

<i>verbatim-string-literal:
   __@"__   verbatim-string-literal-characters<sub>opt</sub>   __"__</i>

<i>verbatim-string-literal-characters:
verbatim-string-literal-character
verbatim-string-literal-characters   verbatim-string-literal-character</i>

<i>verbatim-string-literal-character:
single-verbatim-string-literal-character
quote-escape-sequence</i>

<i>single-verbatim-string-literal-character:
Any character except    __"__</i>

<i>quote-escape-sequence:
   __""__</i>

A character that follows a backslash character (`\`) in a *regular-string-literal-character* must be one of the following characters: `'`, `"`, `\`, `0`, `a`, `b`, `f`, `n`, `r`, `t`, `u`, `U`, `x`, `v`. Otherwise, a compile-time error occurs.

The example

```csharp
string a = "hello, world";                        // hello, world
string b = @"hello, world";                    // hello, world

string c = "hello \t world";                    // hello      world
string d = @"hello \t world";                    // hello \t world

string e = "Joe said \"Hello\" to me";        // Joe said "Hello" to me
string f = @"Joe said ""Hello"" to me";    // Joe said "Hello" to me

string g = "\\\\server\\share\\file.txt";    // \\server\share\file.txt
string h = @"\\server\share\file.txt";        // \\server\share\file.txt

string i = "one\r\ntwo\r\nthree";
string j = @"one
two
three";
```

shows a variety of string literals. The last string literal, `j`, is a verbatim string literal that spans multiple lines. The characters between the quotation marks, including white space such as new line characters, are preserved verbatim.

Since a hexadecimal escape sequence can have a variable number of hex digits, the string literal `"\x123"` contains a single character with hex value 123. To create a string containing the character with hex value 12 followed by the character 3, one could write `"\x00123"` or `"\x12"``+``"3"` instead.

The type of a *string-literal* is `string`.

Each string literal does not necessarily result in a new string instance. When two or more string literals that are equivalent according to the string equality operator (§7.10.7) appear in the same program, these string literals refer to the same string instance. For instance, the output produced by

```csharp
class Test
{
    static void Main() {
        object a = "hello";
        object b = "hello";
        System.Console.WriteLine(a == b);
    }
}
```

is `True` because the two literals refer to the same string instance.

#### The null literal

<i>null-literal:
   __null__</i>

The  *null-literal* can be implicitly converted to a reference type or nullable type.

### Operators and punctuators

There are several kinds of operators and punctuators. Operators are used in expressions to describe operations involving one or more operands. For example, the expression `a + b` uses the `+` operator to add the two operands `a` and `b`. Punctuators are for grouping and separating.

<i>operator-or-punctuator:  one of
   __{__   ____   __    }__   ____   __    [__   ____   __    ]__   ____   __    (__   ____   __    )__   ____   __    .__   ____   __    ,__   ____   __    :__   ____   __    ;__   __
+__   ____   __    -__   ____   __    *__   ____   __    /__   ____   __    %__   ____   __    &amp;__   ____   __    |__   ____   __    ^__   ____   __    !__   ____   __    ~__   __
=__   ____   __    &lt;__   ____   __    &gt;__   ____   __    ?__   ____   ____   __??__   ____   __    ::__   ____   __    ++__   ____   __    --__   ____   __    &amp;&amp;__   ____   __    ||__   __
-&gt;__   ____   __    ==__   ____   ____   __!=__   ____   ____   __&lt;=__   ____   __    &gt;=__   ____   __    +=__   ____   __    -=__   ____   __    *=__   ____   __    /=__   ____   __    %=__   __
&amp;=__   ____   __    |=__   ____   ____   __^=__   ____   ____   __&lt;&lt;__   ____   __    &lt;&lt;=__   __    =&gt;__</i>

<i>right-shift:
   __&gt;__|   __&gt;__</i>

<i>right-shift-assignment:
   __&gt;__|   __&gt;=__</i>

The vertical bar in the *right-shift* and *right-shift-assignment* productions are used to indicate that, unlike other productions in the syntactic grammar, no characters of any kind (not even whitespace) are allowed between the tokens. These productions are treated specially in order to enable the correct  handling of *type-parameter-list* s (§10.1.3).

## Pre-processing directives

The pre-processing directives provide the ability to conditionally skip sections of source files, to report error and warning conditions, and to delineate distinct regions of source code. The term "pre-processing directives" is used only for consistency with the C and C++ programming languages. In C#, there is no separate pre-processing step; pre-processing directives are processed as part of the lexical analysis phase.

<i>pp-directive:
pp-declaration
pp-conditional
pp-line
pp-diagnostic
pp-region 
pp-pragma</i>

The following pre-processing directives are available:

-  `#define` and `#undef`, which are used to define and undefine, respectively, conditional compilation symbols (§2.5.3).
-  `#if`, `#elif`, `#else`, and `#endif`, which are used to conditionally skip sections of source code (§2.5.4).
-  `#line`, which is used to control line numbers emitted for errors and warnings (§2.5.7).
-  `#error` and `#warning`, which are used to issue errors and warnings, respectively (§2.5.5).
-  `#region` and `#endregion`, which are used to explicitly mark sections of source code (§2.5.6).
-  `#pragma`, which is used to specify optional contextual information to the compiler (§2.5.8).

A pre-processing directive always occupies a separate line of source code and always begins with a `#` character and a pre-processing directive name. White space may occur before the `#` character and between the `#` character and the directive name.

A source line containing a `#define`, `#undef`, `#if`, `#elif`, `#else`, `#endif`, `#line`, or `#endregion` directive may end with a single-line comment. Delimited comments (the `/*``*/` style of comments) are not permitted on source lines containing pre-processing directives.

Pre-processing directives are not tokens and are not part of the syntactic grammar of C#. However, pre-processing directives can be used to include or exclude sequences of tokens and can in that way affect the meaning of a C# program. For example, when compiled, the program:

```csharp
#define A
#undef B

class C
{
#if A
    void F() {}
#else
    void G() {}
#endif

#if B
    void H() {}
#else
    void I() {}
#endif
}
```

results in the exact same sequence of tokens as the program:

```csharp
class C
{
    void F() {}
    void I() {}
}
```

Thus, whereas lexically, the two programs are quite different, syntactically, they are identical.

### Conditional compilation symbols

The conditional compilation functionality provided by the `#if`, `#elif`, `#else`, and `#endif` directives is controlled through pre-processing expressions (§2.5.2) and conditional compilation symbols.

<i>conditional-symbol:
Any identifier-or-keyword except    __true__ or    __false__</i>

A conditional compilation symbol has two possible states: *__defined__* or *__undefined__*. At the beginning of the lexical processing of a source file, a conditional compilation symbol is undefined unless it has been explicitly defined by an external mechanism (such as a command-line compiler option). When a `#define` directive is processed, the conditional compilation symbol named in that directive becomes defined in that source file. The symbol remains defined until an `#undef` directive for that same symbol is processed, or until the end of the source file is reached. An implication of this is that `#define` and `#undef` directives in one source file have no effect on other source files in the same program.

When referenced in a pre-processing expression, a defined conditional compilation symbol has the boolean value `true`, and an undefined conditional compilation symbol has the boolean value `false`. There is no requirement that conditional compilation symbols be explicitly declared before they are referenced in pre-processing expressions. Instead, undeclared symbols are simply undefined and thus have the value `false`.

The name space for conditional compilation symbols is distinct and separate from all other named entities in a C# program. Conditional compilation symbols can only be referenced in `#define` and `#undef` directives and in pre-processing expressions.

### Pre-processing expressions

Pre-processing expressions can occur in `#if` and `#elif` directives. The operators `!`, `==`, `!=`, `&amp;&amp;` and `||` are permitted in pre-processing expressions, and parentheses may be used for grouping.

<i>pp-expression:
whitespace<sub>opt</sub>   pp-or-expression   whitespace<sub>opt</sub></i>

<i>pp-or-expression:
pp-and-expression
pp-or-expression   whitespace<sub>opt</sub>   __||__   whitespace<sub>opt</sub>   pp-and-expression</i>

<i>pp-and-expression:
pp-equality-expression
pp-and-expression   whitespace<sub>opt</sub>   __&amp;&amp;__   whitespace<sub>opt</sub>   pp-equality-expression</i>

<i>pp-equality-expression:
pp-unary-expression
pp-equality-expression   whitespace<sub>opt</sub>   __==__   whitespace<sub>opt</sub>   pp-unary-expression
pp-equality-expression   whitespace<sub>opt</sub>   __!=__   whitespace<sub>opt</sub>   pp-unary-expression</i>

<i>pp-unary-expression:
pp-primary-expression
   __!__   whitespace<sub>opt</sub>   pp-unary-expression</i>

<i>pp-primary-expression:
   __true__
   __false__
conditional-symbol
   __(__   whitespace<sub>opt</sub>   pp-expression   whitespace<sub>opt</sub>   __)__</i>

When referenced in a pre-processing expression, a defined conditional compilation symbol has the boolean value `true`, and an undefined conditional compilation symbol has the boolean value `false`.

Evaluation of a pre-processing expression always yields a boolean value. The rules of evaluation for a pre-processing expression are the same as those for a constant expression (§7.19), except that the only user-defined entities that can be referenced are conditional compilation symbols.

### Declaration directives

The declaration directives are used to define or undefine conditional compilation symbols.

<i>pp-declaration:
whitespace<sub>opt</sub>   __#__   whitespace<sub>opt</sub>   __define__   whitespace   conditional-symbol   pp-new-line
whitespace<sub>opt</sub>   __#__   whitespace<sub>opt</sub>   __undef__   whitespace   conditional-symbol   pp-new-line</i>

<i>pp-new-line:
whitespace<sub>opt</sub>   single-line-comment<sub>opt</sub>   new-line</i>

The processing of a `#define` directive causes the given conditional compilation symbol to become defined, starting with the source line that follows the directive. Likewise, the processing of an `#undef` directive causes the given conditional compilation symbol to become undefined, starting with the source line that follows the directive.

Any `#define` and `#undef` directives in a source file must occur before the first *token* (§2.4) in the source file; otherwise a compile-time error occurs. In intuitive terms, `#define` and `#undef` directives must precede any "real code" in the source file.

The example:

```csharp
#define Enterprise

#if Professional || Enterprise
    #define Advanced
#endif

namespace Megacorp.Data
{
    #if Advanced
    class PivotTable {...}
    #endif
}
```

is valid because the `#define` directives precede the first token (the `namespace` keyword) in the source file.

The following example results in a compile-time error because a `#define` follows real code:

```csharp
#define A
namespace N
{
    #define B
    #if B
    class Class1 {}
    #endif
}
```

A `#define` may define a conditional compilation symbol that is already defined, without there being any intervening `#undef` for that symbol. The example below defines a conditional compilation symbol `A` and then defines it again.

```csharp
#define A
#define A
```

A `#undef` may "undefine" a conditional compilation symbol that is not defined. The example below defines a conditional compilation symbol `A` and then undefines it twice; although the second `#undef` has no effect, it is still valid.

```csharp
#define A
#undef A
#undef A
```

### Conditional compilation directives

The conditional compilation directives are used to conditionally include or exclude portions of a source file.

<i>pp-conditional:
pp-if-section   pp-elif-sections<sub>opt</sub>   pp-else-section<sub>opt</sub>   pp-endif</i>

<i>pp-if-section:
whitespace<sub>opt</sub>   __#__   whitespace<sub>opt</sub>   __if__   whitespace   pp-expression   pp-new-line   conditional-section<sub>opt</sub></i>

<i>pp-elif-sections:
pp-elif-section
pp-elif-sections   pp-elif-section</i>

<i>pp-elif-section:
whitespace<sub>opt</sub>   __#__   whitespace<sub>opt</sub>   __elif__   whitespace   pp-expression   pp-new-line   conditional-section<sub>opt</sub></i>

<i>pp-else-section:
whitespace<sub>opt</sub>   __#__   whitespace<sub>opt</sub>   __else__   pp-new-line   conditional-section<sub>opt</sub></i>

<i>pp-endif:
whitespace<sub>opt</sub>   __#__   whitespace<sub>opt</sub>   __endif__   pp-new-line</i>

<i>conditional-section:
input-section
skipped-section</i>

<i>skipped-section:
skipped-section-part
skipped-section   skipped-section-part</i>

<i>skipped-section-part:
skipped-characters<sub>opt</sub>   new-line
pp-directive</i>

<i>skipped-characters:
whitespace<sub>opt</sub>   not-number-sign   input-characters<sub>opt</sub></i>

<i>not-number-sign:
Any input-character except    __#__</i>

As indicated by the syntax, conditional compilation directives must be written as sets consisting of, in order, an `#if` directive, zero or more `#elif` directives, zero or one `#else` directive, and an `#endif` directive. Between the directives are conditional sections of source code. Each section is controlled by the immediately preceding directive. A conditional section may itself contain nested conditional compilation directives provided these directives form complete sets.

A *pp-conditional* selects at most one of the contained *conditional-section* s for normal lexical processing:

-  The *pp-expression* s of the `#if` and `#elif` directives are evaluated in order until one yields `true`. If an expression yields `true`, the *conditional-section* of the corresponding directive is selected.
-  If all *pp-expression* s yield `false`, and if an `#else` directive is present, the *conditional-section* of the `#else` directive is selected.
-  Otherwise, no *conditional-section* is selected.

The selected *conditional-section*, if any, is processed as a normal *input-section*: the source code contained in the section must adhere to the lexical grammar; tokens are generated from the source code in the section; and pre-processing directives in the section have the prescribed effects.

The remaining *conditional-section* s, if any, are processed as *skipped-section* s: except for pre-processing directives, the source code in the section need not adhere to the lexical grammar; no tokens are generated from the source code in the section; and pre-processing directives in the section must be lexically correct but are not otherwise processed. Within a *conditional-section* that is being processed as a *skipped-section*, any nested *conditional-section* s (contained in nested `#if`...`#endif` and `#region`...`#endregion` constructs) are also processed as *skipped-section* s.

The following example illustrates how conditional compilation directives can nest:

```csharp
#define Debug        // Debugging on
#undef Trace        // Tracing off

class PurchaseTransaction
{
    void Commit() {
        #if Debug
            CheckConsistency();
            #if Trace
                WriteToLog(this.ToString());
            #endif
        #endif
        CommitHelper();
    }
}
```

Except for pre-processing directives, skipped source code is not subject to lexical analysis. For example, the following is valid despite the unterminated comment in the `#else` section:

```csharp
#define Debug        // Debugging on

class PurchaseTransaction
{
    void Commit() {
        #if Debug
            CheckConsistency();
        #else
            /* Do something else
        #endif
    }
}
```

Note, however, that pre-processing directives are required to be lexically correct even in skipped sections of source code.

Pre-processing directives are not processed when they appear inside multi-line input elements. For example, the program:

```csharp
class Hello
{
    static void Main() {
        System.Console.WriteLine(@"hello, 
#if Debug
        world
#else
        Nebraska
#endif
        ");
    }
}
```

results in the output:

```
hello,
#if Debug
        world
#else
        Nebraska
#endif
```

In peculiar cases, the set of pre-processing directives that is processed might depend on the evaluation of the *pp-expression*. The example:

```csharp
#if X
    /*
#else
    /* */ class Q { }
#endif
```

always produces the same token stream (`class``Q``{``}`), regardless of whether or not `X` is defined. If `X` is defined, the only processed directives are `#if` and `#endif`, due to the multi-line comment. If `X` is undefined, then three directives (`#if`, `#else`, `#endif`) are part of the directive set.

### Diagnostic directives

The diagnostic directives are used to explicitly generate error and warning messages that are reported in the same way as other compile-time errors and warnings.

<i>pp-diagnostic:
whitespace<sub>opt</sub>   __#__   whitespace<sub>opt</sub>   __error__   pp-message
whitespace<sub>opt</sub>   __#__   whitespace<sub>opt</sub>   __warning__   pp-message</i>

<i>pp-message:
new-line
whitespace   input-characters<sub>opt</sub>   new-line</i>

The example:

```csharp
#warning Code review needed before check-in

#if Debug &amp;&amp; Retail
    #error A build can't be both debug and retail
#endif

class Test {...}
```

always produces a warning ("Code review needed before check-in"), and produces a compile-time error ("A build can't be both debug and retail") if the conditional symbols `Debug` and `Retail` are both defined. Note that a *pp-message* can contain arbitrary text; specifically, it need not contain well-formed tokens, as shown by the single quote in the word `can't`.

### Region directives

The region directives are used to explicitly mark regions of source code.

<i>pp-region:
pp-start-region   conditional-section<sub>opt</sub>   pp-end-region</i>

<i>pp-start-region:
whitespace<sub>opt</sub>   __#__   whitespace<sub>opt</sub>   __region__   pp-message</i>

<i>pp-end-region:
whitespace<sub>opt</sub>   __#__   whitespace<sub>opt</sub>   __endregion__   pp-message</i>

No semantic meaning is attached to a region; regions are intended for use by the programmer or by automated tools to mark a section of source code. The message specified in a `#region` or `#endregion` directive likewise has no semantic meaning; it merely serves to identify the region. Matching `#region` and `#endregion` directives may have different *pp-message* s.

The lexical processing of a region:

```csharp
#region
...
#endregion
```

corresponds exactly to the lexical processing of a conditional compilation directive of the form:

```csharp
#if true
...
#endif
```

### Line directives

Line directives may be used to alter the line numbers and source file names that are reported by the compiler in output such as warnings and errors, and that are used by caller info attributes (§17.4.4).

Line directives are most commonly used in meta-programming tools that generate C# source code from some other text input.

<i>pp-line:
whitespace<sub>opt</sub>   __#__   whitespace<sub>opt</sub>   __line__   whitespace   line-indicator   pp-new-line</i>

<i>line-indicator:
decimal-digits   whitespace   file-name 
decimal-digits
   __default__   __
hidden__</i>

<i>file-name:
   __"__   file-name-characters      __"__</i>

<i>file-name-characters:
file-name-character
file-name-characters   file-name-character</i>

<i>file-name-character:
Any input-character except    __"__</i>

When no `#line` directives are present, the compiler reports true line numbers and source file names in its output. When processing a `#line` directive that includes a *line-indicator* that is not `default`, the compiler treats the line after the directive as having the given line number (and file name, if specified).

A `#line default` directive reverses the effect of all preceding #line directives. The compiler reports true line information for subsequent lines, precisely as if no `#line` directives had been processed.

A `#line hidden` directive has no effect on the file and line numbers reported in error messages, but does affect source level debugging. When debugging, all lines between a `#line hidden` directive and the subsequent `#line` directive (that is not `#line hidden`) have no line number information. When stepping through code in the debugger, these lines will be skipped entirely.

Note that a *file-name* differs from a regular string literal in that escape characters are not processed; the ‘`\`' character simply designates an ordinary backslash character within a *file-name*.

### Pragma directives

The `#pragma` preprocessing directive is used to specify optional contextual information to the compiler. The information supplied in a `#pragma` directive will never change program semantics.

<i>pp-pragma:
whitespace<sub>opt</sub>   __#__   whitespace<sub>opt</sub>   __pragma__   whitespace   pragma-body   pp-new-line</i>

<i>pragma-body:
pragma-warning-body</i>

C# provides `#pragma` directives to control compiler warnings. Future versions of the language may include additional `#pragma` directives. To ensure interoperability with other C# compilers, the Microsoft C# compiler does not issue compilation errors for unknown `#pragma` directives; such directives do however generate warnings.

#### Pragma warning

The `#pragma``warning` directive is used to disable or restore all or a particular set of warning messages during compilation of the subsequent program text.

<i>pragma-warning-body:
   __warning__   whitespace   warning-action
   __warning__   whitespace   warning-action   whitespace   warning-list</i>

<i>warning-action:
   __disable__
   __restore__</i>

<i>warning-list:
decimal-digits
warning-list   whitespace<sub>opt</sub>`,`   whitespace<sub>opt</sub>   decimal-digits</i>

A `#pragma``warning` directive that omits the warning list affects all warnings. A `#pragma``warning` directive the includes a warning list affects only those warnings that are specified in the list.

A `#pragma``warning``disable` directive disables all or the given set of warnings.

A `#pragma``warning``restore` directive restores all or the given set of warnings to the state that was in effect at the beginning of the compilation unit. Note that if a particular warning was disabled externally, a `#pragma``warning``restore` (whether for all or the specific warning) will not re-enable that warning.

The following example shows use of `#pragma``warning` to temporarily disable the warning reported when obsoleted members are referenced, using the warning number from the Microsoft C# compiler.

```csharp
using System;

class Program
{
    [Obsolete]
    static void Foo() {}

    static void Main() {
#pragma warning disable 612
    Foo();
#pragma warning restore 612
    }
}
```
