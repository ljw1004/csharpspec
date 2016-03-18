# Grammar

This appendix contains summaries of the lexical and syntactic grammars found in the main document, and of the grammar extensions for unsafe code. Grammar productions appear here in the same order that they appear in the main document.

Lexical grammar

<pre>input:
input-section<sub>opt</sub></pre>

<pre>input-section:
input-section-part
input-section   input-section-part</pre>

<pre>input-section-part:
input-elements<sub>opt</sub>   new-line
pp-directive</pre>

<pre>input-elements:
input-element
input-elements   input-element</pre>

<pre>input-element:
whitespace
comment
token</pre>

Line terminators

<pre>new-line:
Carriage return character (<b>U+000D</b>)
Line feed character (<b>U+000A</b>)
Carriage return character (<b>U+000D</b>) followed by line feed character (<b>U+000A</b>)
*Next line character (*<b>U+0085</b>*)*
Line separator character (<b>U+2028</b>)
Paragraph separator character (<b>U+2029</b>)</pre>

Comments

<pre>comment:
single-line-comment
delimited-comment</pre>

<pre>single-line-comment:
<b>//</b>   input-characters<sub>opt</sub></pre>

<pre>input-characters:
input-character
input-characters   input-character</pre>

<pre>input-character:
Any Unicode character except a new-line-character</pre>

<pre>new-line-character:
Carriage return character (<b>U+000D</b>)
Line feed character (<b>U+000A</b>)
*Next line character (*<b>U+0085</b>*)*
Line separator character (<b>U+2028</b>)
Paragraph separator character (<b>U+2029</b>)</pre>

<pre>delimited-comment:
/*   delimited-comment-text<sub>opt</sub>   asterisks   /</pre>

<pre>delimited-comment-text:
delimited-comment-section
delimited-comment-text   delimited-comment-section</pre>

<pre>delimited-comment-section:
<b>/</b>
asterisks<sub>opt</sub>   not-slash-or-asterisk</pre>

<pre>asterisks:
<b>*</b>
asterisks   <b>*</b></pre>

<pre>not-slash-or-asterisk:
Any Unicode character except <b>/</b> or <b>*</b></pre>

White space

<pre>whitespace:
Any character with Unicode class Zs
Horizontal tab character (<b>U+0009</b>)
Vertical tab character (<b>U+000B</b>)
Form feed character (<b>U+000C</b>)</pre>

Tokens

<pre>token:
identifier
keyword
integer-literal
real-literal
character-literal
string-literal
operator-or-punctuator</pre>

Unicode character escape sequences

<pre>unicode-escape-sequence:
<b>\u</b>   hex-digit   hex-digit   hex-digit   hex-digit
<b>\U</b>   hex-digit   hex-digit   hex-digit  hex-digit   hex-digit   hex-digit   hex-digit   hex-digit</pre>

Identifiers

<pre>identifier:
available-identifier
<b>@</b>   identifier-or-keyword</pre>

<pre>available-identifier:
An identifier-or-keyword that is not a keyword</pre>

<pre>identifier-or-keyword:
identifier-start-character   identifier-part-characters<sub>opt</sub></pre>

<pre>identifier-start-character:
letter-character
<b>_</b> (the underscore character <b>U+005F</b>)</pre>

<pre>identifier-part-characters:
identifier-part-character
identifier-part-characters   identifier-part-character</pre>

<pre>identifier-part-character:
letter-character
decimal-digit-character
connecting-character
combining-character
formatting-character</pre>

<pre>letter-character:
A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nl 
A unicode-escape-sequence representing a character of classes Lu, Ll, Lt, Lm, Lo, or Nl</pre>

<pre>combining-character:
A Unicode character of classes Mn or Mc 
A unicode-escape-sequence representing a character of classes Mn or Mc</pre>

<pre>decimal-digit-character:
A Unicode character of the class Nd 
A unicode-escape-sequence representing a character of the class Nd</pre>

<pre>connecting-character:  
A Unicode character of the class Pc
A unicode-escape-sequence representing a character of the class Pc</pre>

<pre>formatting-character:  
A Unicode character of the class Cf
A unicode-escape-sequence representing a character of the class Cf</pre>

Keywords

<pre>keyword:  one of
<b>abstract</b><b>as</b><b>base</b><b>bool</b><b>break</b>
<b>byte</b><b>case</b><b>catch</b><b>char</b><b>checked</b>
<b>class</b><b>const</b><b>continue</b><b>decimal</b><b>default</b>
<b>delegate</b><b>do</b><b>double</b><b>else</b><b>enum</b>
<b>event</b><b>explicit</b><b>extern</b><b>false</b><b>finally</b>
<b>fixed</b><b>float</b><b>for</b><b>foreach</b><b>goto</b>
<b>if</b><b>implicit</b><b>in</b><b>int</b><b>interface</b>
<b>internal</b><b>is</b><b>lock</b><b>long</b><b>namespace</b>
<b>new</b><b>null</b><b>object</b><b>operator</b><b>out</b>
<b>override</b><b>params</b><b>private</b><b>protected</b><b>public</b>
<b>readonly</b><b>ref</b><b>return</b><b>sbyte</b><b>sealed</b>
<b>short</b><b>sizeof</b><b>stackalloc</b><b>static</b><b>string</b>
<b>struct</b><b>switch</b><b>this</b><b>throw</b><b>true</b>
<b>try</b><b>typeof</b><b>uint</b><b>ulong</b><b>unchecked</b>
<b>unsafe</b><b>ushort</b><b>using</b><b>virtual</b><b>void</b>
<b>volatile</b><b>while</b></pre>

Literals

<pre>literal:
boolean-literal
integer-literal
real-literal
character-literal
string-literal
null-literal</pre>

<pre>boolean-literal:
<b>true</b>
<b>false</b></pre>

<pre>integer-literal:
decimal-integer-literal
hexadecimal-integer-literal</pre>

<pre>decimal-integer-literal:
decimal-digits   integer-type-suffix<sub>opt</sub></pre>

<pre>decimal-digits:
decimal-digit
decimal-digits   decimal-digit</pre>

<pre>decimal-digit:  one of
<b>0  1  2  3  4  5  6  7  8  9</b></pre>

<pre>integer-type-suffix:  one of
<b>U  u  L  l  UL  Ul  uL  ul  LU  Lu  lU  lu</b></pre>

<pre>hexadecimal-integer-literal:
<b>0x</b>   hex-digits   integer-type-suffix<sub>opt</sub>
<b>0X</b>   hex-digits   integer-type-suffix<sub>opt</sub></pre>

<pre>hex-digits:
hex-digit
hex-digits   hex-digit</pre>

<pre>hex-digit:  one of
<b>0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F  a  b  c  d  e  f</b></pre>

<pre>real-literal:
decimal-digits   <b>.</b>   decimal-digits   exponent-part<sub>opt</sub>   real-type-suffix<sub>opt</sub>
<b>.</b>   decimal-digits   exponent-part<sub>opt</sub>   real-type-suffix<sub>opt</sub>
decimal-digits   exponent-part   real-type-suffix<sub>opt</sub>
decimal-digits   real-type-suffix</pre>

<pre>exponent-part:
<b>e</b>   sign<sub>opt</sub>   decimal-digits
<b>E</b>   sign<sub>opt</sub>   decimal-digits</pre>

<pre>sign:  one of
<b>+  -</b></pre>

<pre>real-type-suffix:  one of
<b>F  f  D  d  M  m</b></pre>

<pre>character-literal:
<b>'</b>   character   <b>'</b></pre>

<pre>character:
single-character
simple-escape-sequence
hexadecimal-escape-sequence
unicode-escape-sequence</pre>

<pre>single-character:
 Any character except <b>'</b> (<b>U+0027</b>), <b>\</b> (<b>U+005C</b>), and new-line-character</pre>

<pre>simple-escape-sequence:  one of
<b>\'  \"  \\  \0  \a  \b  \f  \n  \r  \t  \v</b></pre>

<pre>hexadecimal-escape-sequence:
<b>\x</b>   hex-digit   hex-digit<sub>opt</sub>   hex-digit<sub>opt</sub>   hex-digit<sub>opt</sub></pre>

<pre>string-literal:
regular-string-literal
verbatim-string-literal</pre>

<pre>regular-string-literal:
<b>"</b>   regular-string-literal-characters<sub>opt</sub><b>"</b></pre>

<pre>regular-string-literal-characters:
regular-string-literal-character
regular-string-literal-characters   regular-string-literal-character</pre>

<pre>regular-string-literal-character:
single-regular-string-literal-character
simple-escape-sequence
hexadecimal-escape-sequence
unicode-escape-sequence</pre>

<pre>single-regular-string-literal-character:
Any character except <b>"</b> (<b>U+0022</b>), <b>\</b> (<b>U+005C</b>), and new-line-character</pre>

<pre>verbatim-string-literal:
<b>@"</b>   verbatim-string-literal-characters<sub>opt</sub><b>"</b></pre>

<pre>verbatim-string-literal-characters:
verbatim-string-literal-character
verbatim-string-literal-characters   verbatim-string-literal-character</pre>

<pre>verbatim-string-literal-character:
single-verbatim-string-literal-character
quote-escape-sequence</pre>

<pre>single-verbatim-string-literal-character:
any character except <b>"</b></pre>

<pre>quote-escape-sequence:
<b>""</b></pre>

<pre>null-literal:
<b>null</b></pre>

Operators and punctuators

<pre>operator-or-punctuator:  one of
<b>{</b><b>}</b><b>[</b><b>]</b><b>(</b><b>)</b><b>.</b><b>,</b><b>:</b><b>;</b>
<b>+</b><b>-</b><b>*</b><b>/</b><b>%</b><b>&amp;</b><b>|</b><b>^</b><b>!</b><b>~</b>
<b>=</b><b>&lt;</b><b>&gt;</b><b>?</b><b>??</b><b>::</b><b>++</b><b>--</b><b>&amp;&amp;</b><b>||</b>
<b>-&gt;</b><b>==</b><b>!=</b><b>&lt;=</b><b>&gt;=</b><b>+=</b><b>-=</b><b>*=</b><b>/=</b><b>%=</b>
<b>&amp;=</b><b>|=</b><b>^=</b><b>&lt;&lt;</b><b>&lt;&lt;=</b><b>=&gt;</b></pre>

<pre>right-shift:
<b>&gt;</b>|<b>&gt;</b></pre>

<pre>right-shift-assignment:
<b>&gt;</b>|<b>&gt;=</b></pre>

Pre-processing directives

<pre>pp-directive:
pp-declaration
pp-conditional
pp-line
pp-diagnostic
pp-region 
pp-pragma</pre>

<pre>conditional-symbol:
Any identifier-or-keyword except <b>true</b> or <b>false</b></pre>

<pre>pp-expression:
whitespace<sub>opt</sub>   pp-or-expression   whitespace<sub>opt</sub></pre>

<pre>pp-or-expression:
pp-and-expression
pp-or-expression   whitespace<sub>opt</sub><b>||</b>   whitespace<sub>opt</sub>   pp-and-expression</pre>

<pre>pp-and-expression:
pp-equality-expression
pp-and-expression   whitespace<sub>opt</sub><b>&amp;&amp;</b>   whitespace<sub>opt</sub>   pp-equality-expression</pre>

<pre>pp-equality-expression:
pp-unary-expression
pp-equality-expression   whitespace<sub>opt</sub><b>==</b>   whitespace<sub>opt</sub>   pp-unary-expression
pp-equality-expression   whitespace<sub>opt</sub><b>!=</b>   whitespace<sub>opt</sub>   pp-unary-expression</pre>

<pre>pp-unary-expression:
pp-primary-expression
<b>!</b>   whitespace<sub>opt</sub>   pp-unary-expression</pre>

<pre>pp-primary-expression:
<b>true</b>
<b>false</b>
conditional-symbol
<b>(</b>   whitespace<sub>opt</sub>   pp-expression   whitespace<sub>opt</sub><b>)</b></pre>

<pre>pp-declaration:
whitespace<sub>opt</sub><b>#</b>   whitespace<sub>opt</sub><b>define</b>   whitespace   conditional-symbol   pp-new-line
whitespace<sub>opt</sub><b>#</b>   whitespace<sub>opt</sub><b>undef</b>   whitespace   conditional-symbol   pp-new-line</pre>

<pre>pp-new-line:
whitespace<sub>opt</sub>   single-line-comment<sub>opt</sub>   new-line</pre>

<pre>pp-conditional:
pp-if-section   pp-elif-sections<sub>opt</sub>   pp-else-section<sub>opt</sub>   pp-endif</pre>

<pre>pp-if-section:
whitespace<sub>opt</sub><b>#</b>   whitespace<sub>opt</sub><b>if</b>   whitespace   pp-expression   pp-new-line   conditional-section<sub>opt</sub></pre>

<pre>pp-elif-sections:
pp-elif-section
pp-elif-sections   pp-elif-section</pre>

<pre>pp-elif-section:
whitespace<sub>opt</sub><b>#</b>   whitespace<sub>opt</sub><b>elif</b>   whitespace   pp-expression   pp-new-line   conditional-section<sub>opt</sub></pre>

<pre>pp-else-section:
whitespace<sub>opt</sub><b>#</b>   whitespace<sub>opt</sub><b>else</b>   pp-new-line   conditional-section<sub>opt</sub></pre>

<pre>pp-endif:
whitespace<sub>opt</sub><b>#</b>   whitespace<sub>opt</sub><b>endif</b>   pp-new-line</pre>

<pre>conditional-section:
input-section
skipped-section</pre>

<pre>skipped-section:
skipped-section-part
skipped-section   skipped-section-part</pre>

<pre>skipped-section-part:
skipped-characters<sub>opt</sub>   new-line
pp-directive</pre>

<pre>skipped-characters:
whitespace<sub>opt</sub>   not-number-sign   input-characters<sub>opt</sub></pre>

<pre>not-number-sign:
Any input-character except <b>#</b></pre>

<pre>pp-diagnostic:
whitespace<sub>opt</sub><b>#</b>   whitespace<sub>opt</sub><b>error</b>   pp-message
whitespace<sub>opt</sub><b>#</b>   whitespace<sub>opt</sub><b>warning</b>   pp-message</pre>

<pre>pp-message:
new-line
whitespace   input-characters<sub>opt</sub>   new-line</pre>

<pre>pp-region:
pp-start-region   conditional-section<sub>opt</sub>   pp-end-region</pre>

<pre>pp-start-region:
whitespace<sub>opt</sub><b>#</b>   whitespace<sub>opt</sub><b>region</b>   pp-message</pre>

<pre>pp-end-region:
whitespace<sub>opt</sub><b>#</b>   whitespace<sub>opt</sub><b>endregion</b>   pp-message</pre>

<pre>pp-line:
whitespace<sub>opt</sub><b>#</b>   whitespace<sub>opt</sub><b>line</b>   whitespace   line-indicator   pp-new-line</pre>

<pre>line-indicator:
decimal-digits   whitespace   file-name 
decimal-digits
<b>default</b><b />
<b>hidden</b></pre>

<pre>file-name:
<b>"</b>   file-name-characters   <b>"</b></pre>

<pre>file-name-characters:
file-name-character
file-name-characters   file-name-character</pre>

<pre>file-name-character:
Any input-character except <b>"</b></pre>

<pre>pp-pragma:
whitespace<sub>opt</sub><b>#</b>   whitespace<sub>opt</sub><b>pragma</b>   whitespace   pragma-body   pp-new-line</pre>

<pre>pragma-body:
pragma-warning-body</pre>

<pre>pragma-warning-body:
<b>warning</b>   whitespace   warning-action
<b>warning</b>   whitespace   warning-action   whitespace   warning-list</pre>

<pre>warning-action:
<b>disable</b>
<b>restore</b></pre>

<pre>warning-list:
decimal-digits
warning-list   whitespace<sub>opt</sub>`,`   whitespace<sub>opt</sub>   decimal-digits</pre>

Syntactic grammar

Basic concepts

<pre>namespace-name:
namespace-or-type-name</pre>

<pre>type-name:
namespace-or-type-name</pre>

<pre>namespace-or-type-name:
identifier   type-argument-list<sub>opt</sub>
namespace-or-type-name   <b>.</b>   identifier   type-argument-list<sub>op</sub><sub>t</sub><sub>
</sub>qualified-alias-member</pre>

Types

<pre>type:
value-type
reference-type 
type-parameter</pre>

<pre>value-type:
struct-type
enum-type</pre>

<pre>struct-type:
type-name
simple-type 
nullable-type</pre>

<pre>simple-type:
numeric-type
<b>bool</b></pre>

<pre>numeric-type:
integral-type
floating-point-type
<b>decimal</b></pre>

<pre>integral-type:
<b>sbyte</b>
<b>byte</b>
<b>short</b>
<b>ushort</b>
<b>int</b>
<b>uint</b>
<b>long</b>
<b>ulong</b>
<b>char</b></pre>

<pre>floating-point-type:
<b>float</b>
<b>double</b></pre>

<pre>nullable-type:
non-nullable-value-type   <b>?</b></pre>

<pre>non-nullable-value-type:
type</pre>

<pre>enum-type:
type-name</pre>

<pre>reference-type:
class-type
interface-type
array-type
delegate-type</pre>

<pre>class-type:
type-name
<b>object</b>
<b>dynamic</b>
<b>string</b></pre>

<pre>interface-type:
type-name</pre>

<pre>rank-specifiers:
rank-specifier
rank-specifiers   rank-specifier</pre>

<pre>rank-specifier:
<b>[</b>   dim-separators<sub>opt</sub><b>]</b></pre>

<pre> dim-separators:
<b>,</b>
dim-separators   <b>,</b></pre>

<pre>delegate-type:
type-name</pre>

<pre>type-argument-list:
<b>&lt;</b>   type-arguments   <b>&gt;</b></pre>

<pre>type-arguments:
type-argument
type-arguments   <b>,</b>   type-argument</pre>

<pre>type-argument:
type</pre>

<pre>type-parameter:
identifier</pre>

Variables

<pre>variable-reference:
expression</pre>

Expressions

<pre>argument-list:
argument
argument-list   <b>,</b>   argument</pre>

<pre>argument:
argument-name<sub>opt</sub>   argument-value</pre>

<pre>argument-name:
identifier   <b>:</b></pre>

<pre>argument-value:
expression
<b>ref</b>   variable-reference
<b>out</b>   variable-reference</pre>

<pre>primary-expression: 
primary-no-array-creation-expression
array-creation-expression</pre>

<pre>primary-no-array-creation-expression:
literal
simple-name
parenthesized-expression
member-access
invocation-expression
element-access
this-access
base-access
post-increment-expression
post-decrement-expression
object-creation-expression
delegate-creation-expression
anonymous-object-creation-expression
typeof-expression
 checked-expression
unchecked-expression 
default-value-expression
anonymous-method-expression</pre>

<pre>simple-name:
identifier   type-argument-list<sub>opt</sub></pre>

<pre>parenthesized-expression:
<b>(</b>   expression   <b>)</b></pre>

<pre>member-access:
primary-expression   <b>.</b>   identifier  type-argument-list<sub>opt</sub>
predefined-type   <b>.</b>   identifier  type-argument-list<sub>opt</sub>
qualified-alias-member   <b>.</b>   identifier</pre>

<pre>predefined-type:  one of
<b>bool</b><b>byte</b><b>char</b><b>decimal</b><b>double</b><b>float</b><b>int</b><b>long</b>
<b>object</b><b>sbyte</b><b>short</b><b>string</b><b>uint</b><b>ulong</b><b>ushort</b></pre>

<pre>invocation-expression:
primary-expression   <b>(</b>   argument-list<sub>opt</sub><b>)</b></pre>

<pre>element-access:
primary-no-array-creation-expression   <b>[</b>   argument-list   <b>]</b></pre>

<pre>this-access:
<b>this</b></pre>

<pre>base-access:
<b>base</b><b>.</b>   identifier
<b>base</b><b>[</b>   argument-list   <b>]</b></pre>

<pre>post-increment-expression:
primary-expression   <b>++</b></pre>

<pre>post-decrement-expression:
primary-expression   <b>--</b></pre>

<pre>object-creation-expression:
<b>new</b>   type   <b>(</b>   argument-list<sub>opt</sub><b>)</b>   object-or-collection-initializer<sub>opt</sub>
<b>new</b>   type   object-or-collection-initializer</pre>

<pre>object-or-collection-initializer:
object-initializer
collection-initializer</pre>

<pre>object-initializer:
<b>{</b>   member-initializer-list<sub>opt</sub><b>}</b>
<b>{</b>   member-initializer-list   <b>,</b><b>}</b></pre>

<pre>member-initializer-list:
member-initializer
member-initializer-list   <b>,</b>   member-initializer</pre>

<pre>member-initializer:
identifier   =   initializer-value</pre>

<pre>initializer-value:
expression
object-or-collection-initializer</pre>

<pre>collection-initializer:
<b>{</b>   element-initializer-list   <b>}</b>
<b>{</b>   element-initializer-list   <b>,</b><b>}</b></pre>

<pre>element-initializer-list:
element-initializer
element-initializer-list   <b>,</b>   element-initializer</pre>

<pre>element-initializer:
non-assignment-expression
<b>{</b>   expression-list   <b>}</b></pre>

<pre>expression-list:
expression
expression-list   <b>,</b>   expression</pre>

<pre>array-creation-expression:
<b>new</b>   non-array-type   <b>[</b>   expression-list   <b>]</b>   rank-specifiers<sub>opt</sub>   array-initializer<sub>opt</sub>
<b>new</b>   array-type   array-initializer 
<b>new</b>   rank-specifier   array-initializer</pre>

<pre>delegate-creation-expression:
<b>new</b>   delegate-type   <b>(</b>   expression   <b>)</b></pre>

<pre>anonymous-object-creation-expression:
<b>new</b>   anonymous-object-initializer</pre>

<pre>anonymous-object-initializer:
<b>{</b>   member-declarator-list<sub>opt</sub><b>}</b>
<b>{</b>   member-declarator-list   <b>,</b><b>}</b></pre>

<pre>member-declarator-list:
member-declarator
member-declarator-list   <b>,</b>   member-declarator</pre>

<pre>member-declarator:
simple-name
member-access
identifier   =   expression</pre>

<pre>typeof-expression:
<b>typeof</b><b>(</b>   type   <b>)</b>
<b>typeof</b><b>(</b>   unbound-type-name   <b>)</b>
<b>typeof ( void )</b></pre>

<pre>unbound-type-name:
identifier   generic-dimension-specifier<sub>opt</sub>
identifier   <b>::</b>   identifier   generic-dimension-specifier<sub>opt</sub>
unbound-type-name   *__.__*   identifier   generic-dimension-specifier<sub>opt</sub></pre>

<pre>generic-dimension-specifier:
<b>&lt;</b>   commas<sub>opt</sub><b>&gt;</b></pre>

<pre>commas:
<b>,</b>
commas   <b>,</b></pre>

<pre>checked-expression:
<b>checked</b><b>(</b>   expression   <b>)</b></pre>

<pre>unchecked-expression:
<b>unchecked</b><b>(</b>   expression   <b>)</b></pre>

<pre>default-value-expression:
<b>default</b><b>(</b>   type   <b>)</b></pre>

<pre>unary-expression:
primary-expression
<b>+</b>   unary-expression
<b>-</b>   unary-expression
<b>!</b>   unary-expression
<b>~</b>   unary-expression
pre-increment-expression
pre-decrement-expression
cast-expression</pre>

<pre>pre-increment-expression:
<b>++</b>   unary-expression</pre>

<pre>pre-decrement-expression:
<b>--</b>   unary-expression</pre>

<pre>cast-expression:
<b>(</b>   type   <b>)</b>   unary-expression</pre>

<pre>multiplicative-expression:
unary-expression
multiplicative-expression   <b>*</b>   unary-expression
multiplicative-expression   <b>/</b>   unary-expression
multiplicative-expression   <b>%</b>   unary-expression</pre>

<pre>additive-expression:
multiplicative-expression
additive-expression   <b>+</b>   multiplicative-expression
additive-expression   <b>–</b>   multiplicative-expression</pre>

<pre>shift-expression:
additive-expression 
shift-expression   <b>&lt;&lt;</b>   additive-expression
shift-expression   right-shift   additive-expression</pre>

<pre>relational-expression:
shift-expression
relational-expression   <b>&lt;</b>   shift-expression
relational-expression   <b>&gt;</b>   shift-expression
relational-expression   <b>&lt;=</b>   shift-expression
relational-expression   <b>&gt;=</b>   shift-expression
relational-expression   <b>is</b>   type
relational-expression   <b>as</b>   type</pre>

<pre>equality-expression:
relational-expression
equality-expression   <b>==</b>   relational-expression
equality-expression   <b>!=</b>   relational-expression</pre>

<pre>and-expression:
equality-expression
and-expression   <b>&amp;</b>   equality-expression</pre>

<pre>exclusive-or-expression:
and-expression
exclusive-or-expression   <b>^</b>   and-expression</pre>

<pre>inclusive-or-expression:
exclusive-or-expression
inclusive-or-expression   <b>|</b>   exclusive-or-expression</pre>

<pre>conditional-and-expression:
inclusive-or-expression
conditional-and-expression   <b>&amp;&amp;</b>   inclusive-or-expression</pre>

<pre>conditional-or-expression:
conditional-and-expression
conditional-or-expression   <b>||</b>   conditional-and-expression</pre>

<pre>null-coalescing-expression:
conditional-or-expression
conditional-or-expression   <b>??</b>   null-coalescing-expression</pre>

<pre>conditional-expression:
null-coalescing-expression
null-coalescing-expression   <b>?</b>   expression   <b>:</b>   expression</pre>

<pre>lambda-expression:
anonymous-function-signature   <b>=&gt;</b>   anonymous-function-body</pre>

<pre>anonymous-method-expression:
<b>delegate</b>   explicit-anonymous-function-signature<sub>opt</sub>   block</pre>

<pre>anonymous-function-signature:
explicit-anonymous-function-signature 
implicit-anonymous-function-signature</pre>

<pre>explicit-anonymous-function-signature:
<b>(</b>   explicit-anonymous-function-parameter-list<sub>*opt*</sub>**`*)*`</pre>

<pre>explicit-anonymous-function-parameter-list:
explicit-anonymous-function-parameter
explicit-anonymous-function-parameter-list   <b>,</b>   explicit-anonymous-function-parameter</pre>

<pre>explicit-anonymous-function-parameter:
anonymous-function-parameter-modifier<sub>opt</sub>   type   identifier</pre>

<pre>anonymous-function-parameter-modifier: 
<b>ref</b>
<b>out</b></pre>

<pre>implicit-anonymous-function-signature:
`*(*`** implicit-anonymous-function-parameter-list<sub>*opt*</sub>**`*)*``
`implicit-anonymous-function-parameter</pre>

<pre>implicit-anonymous-function-parameter-list:
implicit-anonymous-function-parameter
implicit-anonymous-function-parameter-list   <b>,</b>   implicit-anonymous-function-parameter</pre>

<pre>implicit-anonymous-function-parameter:
identifier</pre>

<pre>anonymous-function-body:
expression
block</pre>

<pre>query-expression:
from-clause   query-body</pre>

<pre>from-clause:
<b>from</b>   type<sub>opt</sub>   identifier   <b>in</b>   expression</pre>

<pre>query-body:
query-body-clauses<sub>opt</sub>   select-or-group-clause   query-continuation<sub>opt</sub></pre>

<pre>query-body-clauses:
query-body-clause
query-body-clauses   query-body-clause</pre>

<pre>query-body-clause:
from-clause
let-clause
where-clause
join-clause
join-into-clause
orderby-clause</pre>

<pre>let-clause:
<b>let</b>   identifier   <b>=</b>   expression</pre>

<pre>where-clause:
<b>where</b>   boolean-expression</pre>

<pre>join-clause:
<b>join</b>   type<sub>opt</sub>   identifier   <b>in</b>   expression   <b>on</b>   expression   <b>equals</b>   expression </pre>

<pre>join-into-clause:
<b>j</b><b>oin</b>   type<sub>opt</sub>   identifier   <b>in</b>   expression   <b>on</b>   expression   <b>equals</b>   expression   <b>into</b>   identifier</pre>

<pre>orderby-clause:
<b>orderby</b>   orderings</pre>

<pre>orderings:
ordering
orderings   <b>,</b>   ordering</pre>

<pre>ordering:
expression    ordering-direction<sub>opt</sub></pre>

<pre>ordering-direction:
<b>ascending</b>
<b>descending</b></pre>

<pre>select-or-group-clause:
select-clause
group-clause</pre>

<pre>select-clause:
<b>select</b>   expression</pre>

<pre>group-clause:
<b>group</b>   expression   <b>by</b>   expression</pre>

<pre>query-continuation:
<b>into</b>   identifier   query-body</pre>

<pre>assignment:
unary-expression   assignment-operator   expression</pre>

<pre>assignment-operator:
<b>=</b>
<b>+=</b>
<b>-=</b>
<b>*=</b>
<b>/=</b>
<b>%=</b>
<b>&amp;=</b>
<b>|=</b>
<b>^=</b>
<b>&lt;&lt;=</b>
right-shift-assignment</pre>

<pre>expression: 
non-assignment-expression
assignment</pre>

<pre>non-assignment-expression:
conditional-expression
lambda-expression
query-expression</pre>

<pre>constant-expression:
expression</pre>

<pre>boolean-expression:
expression</pre>

Statements

<pre>statement:
labeled-statement
declaration-statement
embedded-statement</pre>

<pre>embedded-statement:
block
empty-statement
expression-statement
selection-statement
iteration-statement
jump-statement
try-statement
checked-statement
unchecked-statement
lock-statement
using-statement 
yield-statement</pre>

<pre>block:
<b>{</b>   statement-list<sub>opt</sub><b>}</b></pre>

<pre>statement-list:
statement
statement-list   statement</pre>

<pre>empty-statement:
<b>;</b></pre>

<pre>labeled-statement:
identifier   <b>:</b>   statement</pre>

<pre>declaration-statement:
local-variable-declaration   <b>;</b>
local-constant-declaration   <b>;</b></pre>

<pre>local-variable-declaration:
local-variable-type   local-variable-declarators</pre>

<pre>local-variable-type:
type
`*var*`</pre>

<pre>local-variable-declarators:
local-variable-declarator
local-variable-declarators   <b>,</b>   local-variable-declarator</pre>

<pre>local-variable-declarator:
identifier
identifier   =   local-variable-initializer</pre>

<pre>local-variable-initializer:
expression
array-initializer</pre>

<pre>local-constant-declaration:
<b>const</b>   type   constant-declarators</pre>

<pre>constant-declarators:
constant-declarator
constant-declarators   <b>,</b>   constant-declarator</pre>

<pre>constant-declarator:
identifier   =   constant-expression</pre>

<pre>expression-statement:
statement-expression   <b>;</b></pre>

<pre>statement-expression:
invocation-expression
object-creation-expression
assignment
post-increment-expression
post-decrement-expression
pre-increment-expression
pre-decrement-expression</pre>

<pre>selection-statement:
if-statement
switch-statement</pre>

<pre>if-statement:
<b>if</b><b>(</b>   boolean-expression   <b>)</b>   embedded-statement
<b>if</b><b>(</b>   boolean-expression   <b>)</b>   embedded-statement   <b>else</b>   embedded-statement</pre>

<pre>switch-statement:
<b>switch</b><b>(</b>   expression   <b>)</b>   switch-block</pre>

<pre>switch-block:
<b>{</b>   switch-sections<sub>opt</sub><b>}</b></pre>

<pre>switch-sections:
switch-section
switch-sections   switch-section</pre>

<pre>switch-section:
switch-labels   statement-list</pre>

<pre>switch-labels:
switch-label
switch-labels   switch-label</pre>

<pre>switch-label:
<b>case</b>   constant-expression   <b>:</b>
<b>default</b><b>:</b></pre>

<pre>iteration-statement:
while-statement
do-statement
for-statement
foreach-statement</pre>

<pre>while-statement:
<b>while</b><b>(</b>   boolean-expression   <b>)</b>   embedded-statement</pre>

<pre>do-statement:
<b>do</b>   embedded-statement   <b>while</b><b>(</b>   boolean-expression   <b>)</b><b>;</b></pre>

<pre>for-statement:
<b>for</b><b>(</b>   for-initializer<sub>opt</sub><b>;</b>   for-condition<sub>opt</sub><b>;</b>   for-iterator<sub>opt</sub><b>)</b>   embedded-statement</pre>

<pre>for-initializer:
local-variable-declaration
statement-expression-list</pre>

<pre>for-condition:
boolean-expression</pre>

<pre>for-iterator:
statement-expression-list</pre>

<pre>statement-expression-list:
statement-expression
statement-expression-list   <b>,</b>   statement-expression</pre>

<pre>foreach-statement:
<b>foreach</b><b>(</b>   local-variable-type   identifier   <b>in</b>   expression   <b>)</b>   embedded-statement</pre>

<pre>jump-statement:
break-statement
continue-statement
goto-statement
return-statement
throw-statement</pre>

<pre>break-statement:
<b>break</b><b>;</b></pre>

<pre>continue-statement:
<b>continue</b><b>;</b></pre>

<pre>goto-statement:
<b>goto</b>   identifier   <b>;</b>
<b>goto</b><b>case</b>   constant-expression   ;
<b>goto</b><b>default</b><b>;</b></pre>

<pre>return-statement:
<b>return</b>   expression<sub>opt</sub><b>;</b></pre>

<pre>throw-statement:
<b>throw</b>   expression<sub>opt</sub><b>;</b></pre>

<pre>try-statement:
<b>try</b>   block   catch-clauses
<b>try</b>   block   finally-clause
<b>try</b>   block   catch-clauses   finally-clause</pre>

<pre>catch-clauses:
specific-catch-clauses   general-catch-clause<sub>opt</sub>
specific-catch-clauses<sub>opt</sub>   general-catch-clause</pre>

<pre>specific-catch-clauses:
specific-catch-clause
specific-catch-clauses   specific-catch-clause</pre>

<pre>specific-catch-clause:
<b>catch</b><b>(</b>   class-type   identifier<sub>opt</sub><b>)</b>   block</pre>

<pre>general-catch-clause:
<b>catch</b>   block</pre>

<pre>finally-clause:
<b>finally</b>   block</pre>

<pre>checked-statement:
<b>checked</b>   block</pre>

<pre>unchecked-statement:
<b>unchecked</b>   block</pre>

<pre>lock-statement:
<b>lock</b><b>(</b>   expression   <b>)</b>   embedded-statement</pre>

<pre>using-statement:
<b>using</b><b>(</b>    resource-acquisition   <b>)</b>    embedded-statement</pre>

<pre>resource-acquisition:
local-variable-declaration
expression</pre>

<pre>yield-statement:
<b>yield</b><b>return</b>   expression   <b>;</b>
<b>yield</b><b>break</b><b>;</b></pre>

Namespaces

<pre>compilation-unit:
extern-alias-directives<sub>opt</sub>   using-directives<sub>opt</sub>  global-attributes<sub>opt</sub>
        namespace-member-declarations<sub>opt</sub></pre>

<pre>namespace-declaration:
<b>namespace</b>   qualified-identifier   namespace-body   <b>;</b><sub>opt</sub></pre>

<pre>qualified-identifier:
identifier
qualified-identifier   <b>.</b>   identifier</pre>

<pre>namespace-body:
<b>{</b>   extern-alias-directives<sub>opt</sub>   using-directives<sub>opt</sub>   namespace-member-declarations<sub>opt</sub><b>}</b></pre>

<pre>extern-alias-directives:
extern-alias-directive
extern-alias-directives   extern-alias-directive</pre>

<pre>extern-alias-directive:
<b>extern</b><b>alias</b>   identifier   <b>;</b></pre>

<pre>using-directives:
using-directive
using-directives   using-directive</pre>

<pre>using-directive:
using-alias-directive
using-namespace-directive</pre>

<pre>using-alias-directive:
<b>using</b>   identifier   <b>=</b>   namespace-or-type-name   <b>;</b></pre>

<pre>using-namespace-directive:
<b>using</b>   namespace-name   <b>;</b></pre>

<pre>namespace-member-declarations:
namespace-member-declaration
namespace-member-declarations   namespace-member-declaration</pre>

<pre>namespace-member-declaration:
namespace-declaration
type-declaration</pre>

<pre>type-declaration:
class-declaration
struct-declaration
interface-declaration
enum-declaration
delegate-declaration</pre>

<pre>qualified-alias-member:
identifier   <b>::</b>   identifier   type-argument-list<sub>opt</sub></pre>

Classes

<pre>class-declaration:
attributes<sub>opt</sub>   class-modifiers<sub>opt</sub><b>partial</b><sub>opt</sub><b>class</b>   identifier   type-parameter-list<sub>opt</sub>
        class-base<sub>opt</sub>   type-parameter-constraints-clauses<sub>opt</sub>   class-body   <b>;</b><sub>opt</sub></pre>

<pre>class-modifiers:
class-modifier
class-modifiers   class-modifier</pre>

<pre>class-modifier:
<b>new</b>
<b>public</b>
<b>protected</b>
<b>internal</b>
<b>private</b>
<b>abstract</b>
<b>sealed</b>
<b>static</b></pre>

<pre>type-parameter-list:
<b>&lt;</b>   type-parameters   <b>&gt;</b></pre>

<pre>type-parameters:
attributes<sub>opt</sub>   type-parameter
type-parameters   <b>,</b>   attributes<sub>opt</sub>   type-parameter</pre>

<pre>type-parameter:
identifier</pre>

<pre>class-base:
<b>:</b>   class-type
<b>:</b>   interface-type-list
<b>:</b>   class-type   <b>,</b>   interface-type-list</pre>

<pre>interface-type-list:
interface-type
interface-type-list   <b>,</b>   interface-type</pre>

<pre>type-parameter-constraints-clauses:
type-parameter-constraints-clause
type-parameter-constraints-clauses   type-parameter-constraints-clause</pre>

<pre>type-parameter-constraints-clause:
<b>where</b>   type-parameter   <b>:</b>   type-parameter-constraints</pre>

<pre>type-parameter-constraints:
primary-constraint
secondary-constraints
constructor-constraint
primary-constraint   <b>,</b>   secondary-constraints
primary-constraint   <b>,</b>   constructor-constraint
secondary-constraints   <b>,</b>   constructor-constraint
primary-constraint   <b>,</b>   secondary-constraints   <b>,</b>   constructor-constraint</pre>

<pre>primary-constraint:
class-type
<b>class</b>
<b>struct</b></pre>

<pre>secondary-constraints:
interface-type
type-parameter
secondary-constraints   <b>,</b>   interface-type
secondary-constraints   <b>,</b>   type-parameter</pre>

<pre>constructor-constraint:
<b>new</b><b>(</b><b>)</b></pre>

<pre>class-body:
<b>{</b>   class-member-declarations<sub>opt</sub><b>}</b></pre>

<pre>class-member-declarations:
class-member-declaration
class-member-declarations   class-member-declaration</pre>

<pre>class-member-declaration:
constant-declaration
field-declaration
method-declaration
property-declaration
event-declaration
indexer-declaration
operator-declaration
constructor-declaration
destructor-declaration
static-constructor-declaration
type-declaration</pre>

<pre>constant-declaration:
attributes<sub>opt</sub>   constant-modifiers<sub>opt</sub><b>const</b>   type   constant-declarators   <b>;</b></pre>

<pre>constant-modifiers:
constant-modifier
constant-modifiers   constant-modifier</pre>

<pre>constant-modifier:
<b>new</b>
<b>public</b>
<b>protected</b>
<b>internal</b>
<b>private</b></pre>

<pre>constant-declarators:
constant-declarator
constant-declarators   <b>,</b>   constant-declarator</pre>

<pre>constant-declarator:
identifier   =   constant-expression</pre>

<pre>field-declaration:
attributes<sub>opt</sub>   field-modifiers<sub>opt</sub>   type   variable-declarators   <b>;</b></pre>

<pre>field-modifiers:
field-modifier
field-modifiers   field-modifier</pre>

<pre>field-modifier:
<b>new</b>
<b>public</b>
<b>protected</b>
<b>internal</b>
<b>private</b>
<b>static</b>
<b>readonly</b>
<b>volatile</b></pre>

<pre>variable-declarators:
variable-declarator
variable-declarators   <b>,</b>   variable-declarator</pre>

<pre>variable-declarator:
identifier
identifier   =   variable-initializer</pre>

<pre>variable-initializer:
expression
array-initializer</pre>

<pre>method-declaration:
method-header   method-body</pre>

<pre>method-header:
attributes<sub>opt</sub>   method-modifiers<sub>opt</sub>`*partial*`<sub>opt</sub>   return-type   member-name   type-parameter-list<sub>opt</sub>
<b>(</b>   formal-parameter-list<sub>opt</sub><b>)</b>   type-parameter-constraints-clauses<sub>opt</sub></pre>

<pre>method-modifiers:
method-modifier
method-modifiers   method-modifier</pre>

<pre>method-modifier:
<b>new</b>
<b>public</b>
<b>protected</b>
<b>internal</b>
<b>private</b>
<b>static</b>
<b>virtual</b>
<b>sealed</b>
<b>override</b>
<b>abstract</b>
<b>extern</b></pre>

<pre>return-type:
type
<b>void</b></pre>

<pre>member-name:
identifier
interface-type   <b>.</b>   identifier</pre>

<pre>method-body:
block
<b>;</b></pre>

<pre>formal-parameter-list:
fixed-parameters
fixed-parameters   <b>,</b>   parameter-array
parameter-array</pre>

<pre>fixed-parameters:
fixed-parameter
fixed-parameters   <b>,</b>   fixed-parameter</pre>

<pre>fixed-parameter:
attributes<sub>opt</sub>   parameter-modifier<sub>opt</sub>   type   identifier   default-argument<sub>opt</sub></pre>

<pre>default-argument:
`=`  expression</pre>

<pre>parameter-modifier:
<b>ref</b>
<b>out</b>
<b>this</b></pre>

<pre>parameter-array:
attributes<sub>opt</sub><b>params</b>   array-type   identifier</pre>

<pre>property-declaration:
attributes<sub>opt</sub>   property-modifiers<sub>opt</sub>   type   member-name   <b>{</b>   accessor-declarations   <b>}</b></pre>

<pre>property-modifiers:
property-modifier
property-modifiers   property-modifier</pre>

<pre>property-modifier:
<b>new</b>
<b>public</b>
<b>protected</b>
<b>internal</b>
<b>private</b>
<b>static</b>
<b>virtual</b>
<b>sealed</b>
<b>override</b>
<b>abstract</b>
<b>extern</b></pre>

<pre>member-name:
identifier
interface-type   <b>.</b>   identifier</pre>

<pre>accessor-declarations:
get-accessor-declaration   set-accessor-declaration<sub>opt</sub>
set-accessor-declaration   get-accessor-declaration<sub>opt</sub></pre>

<pre>get-accessor-declaration:
attributes<sub>opt</sub>   accessor-modifier<sub>opt </sub><b>get</b>   accessor-body</pre>

<pre>set-accessor-declaration:
attributes<sub>opt</sub>   accessor-modifier<sub>opt</sub><b>set</b>   accessor-body</pre>

<pre>accessor-modifier:
<b>protected</b>
<b>internal</b>
<b>private</b>
<b>protected</b><b>internal</b>
<b>internal</b><b>protected</b></pre>

<pre>accessor-body:
block
<b>;</b></pre>

<pre>event-declaration:
attributes<sub>opt</sub>   event-modifiers<sub>opt</sub><b>event</b>   type   variable-declarators   <b>;</b>
attributes<sub>opt</sub>   event-modifiers<sub>opt</sub><b>event</b>   type   member-name   <b>{</b>   event-accessor-declarations   <b>}</b></pre>

<pre>event-modifiers:
event-modifier
event-modifiers   event-modifier</pre>

<pre>event-modifier:
<b>new</b>
<b>public</b>
<b>protected</b>
<b>internal</b>
<b>private</b>
<b>static</b>
<b>virtual</b>
<b>sealed</b>
<b>override</b>
<b>abstract</b>
<b>extern</b></pre>

<pre>event-accessor-declarations:
add-accessor-declaration   remove-accessor-declaration
remove-accessor-declaration   add-accessor-declaration</pre>

<pre>add-accessor-declaration:
attributes<sub>opt</sub><b>add</b>   block</pre>

<pre>remove-accessor-declaration:
attributes<sub>opt</sub><b>remove</b>   block</pre>

<pre>indexer-declaration:
attributes<sub>opt</sub>   indexer-modifiers<sub>opt</sub>   indexer-declarator   <b>{</b>   accessor-declarations   <b>}</b></pre>

<pre>indexer-modifiers:
indexer-modifier
indexer-modifiers   indexer-modifier</pre>

<pre>indexer-modifier:
<b>new</b>
<b>public</b>
<b>protected</b>
<b>internal</b>
<b>private </b>
<b>virtual</b>
<b>sealed</b>
<b>override</b>
<b>abstract</b>
<b>extern</b></pre>

<pre>indexer-declarator:
type   <b>this</b><b>[</b>   formal-parameter-list   <b>]</b>
type   interface-type   <b>.</b><b>this</b><b>[</b>   formal-parameter-list   <b>]</b></pre>

<pre>operator-declaration:
attributes<sub>opt</sub>   operator-modifiers   operator-declarator   operator-body</pre>

<pre>operator-modifiers:
operator-modifier
operator-modifiers   operator-modifier</pre>

<pre>operator-modifier:
<b>public</b>
<b>static</b>
<b>extern</b></pre>

<pre>operator-declarator:
unary-operator-declarator
binary-operator-declarator
conversion-operator-declarator</pre>

<pre>unary-operator-declarator:
type   <b>operator</b>   overloadable-unary-operator   <b>(</b>   type   identifier   <b>)</b></pre>

<pre>overloadable-unary-operator:  one of
<b>+   -   !   ~   ++   --   true   false</b></pre>

<pre>binary-operator-declarator:
type   <b>operator</b>   overloadable-binary-operator   <b>(</b>   type   identifier   <b>,</b>   type   identifier   <b>)</b></pre>

<pre>overloadable-binary-operator:
<b>+</b>
<b>-</b>
<b>*</b>
<b>/</b>
<b>%</b>
<b>&amp;</b>
<b>|</b>
<b>^</b>
<b>&lt;&lt;</b>
right-shift
<b>==</b>
<b>!=</b>
<b>&gt;</b>
<b>&lt;</b>
<b>&gt;=</b>
<b>&lt;=</b></pre>

<pre>conversion-operator-declarator:
<b>implicit</b><b>operator</b>   type   <b>(</b>   type   identifier   <b>)</b>
<b>explicit</b><b>operator</b>   type   <b>(</b>   type   identifier   <b>)</b></pre>

<pre>operator-body:
block
<b>;</b></pre>

<pre>constructor-declaration:
attributes<sub>opt</sub>   constructor-modifiers<sub>opt</sub>   constructor-declarator   constructor-body</pre>

<pre>constructor-modifiers:
constructor-modifier
constructor-modifiers   constructor-modifier</pre>

<pre>constructor-modifier:
<b>public</b>
<b>protected</b>
<b>internal</b>
<b>private</b>
<b>extern</b></pre>

<pre>constructor-declarator:
identifier   <b>(</b>   formal-parameter-list<sub>opt</sub><b>)</b>   constructor-initializer<sub>opt</sub></pre>

<pre>constructor-initializer:
<b>:</b><b>base</b><b>(</b>   argument-list<sub>opt</sub><b>)</b>
<b>:</b><b>this</b><b>(</b>   argument-list<sub>opt</sub><b>)</b></pre>

<pre>constructor-body:
block
<b>;</b></pre>

<pre>static-constructor-declaration:
attributes<sub>opt</sub>   static-constructor-modifiers  identifier   <b>(</b><b>)</b>   static-constructor-body</pre>

<pre>static-constructor-modifiers:
<b>extern</b><sub>opt</sub><b> static</b>
<b>static extern</b><sub>opt</sub></pre>

<pre>static-constructor-body:
block
<b>;</b></pre>

<pre>destructor-declaration:
attributes<sub>opt</sub><b>extern</b><sub>opt</sub><b>~</b>   identifier   <b>(</b><b>)</b>    destructor-body</pre>

<pre>destructor-body:
block
<b>;</b></pre>

Structs

<pre>struct-declaration:
attributes<sub>opt</sub>   struct-modifiers<sub>opt</sub><b>partial</b><sub>opt</sub><b>struct</b>   identifier   type-parameter-list<sub>opt</sub>
        struct-interfaces<sub>opt</sub>   type-parameter-constraints-clauses<sub>opt</sub>   struct-body   <b>;</b><sub>opt</sub></pre>

<pre>struct-modifiers:
struct-modifier
struct-modifiers   struct-modifier</pre>

<pre>struct-modifier:
<b>new</b>
<b>public</b>
<b>protected</b>
<b>internal</b>
<b>private</b></pre>

<pre>struct-interfaces:
<b>:</b>   interface-type-list</pre>

<pre>struct-body:
<b>{</b>   struct-member-declarations<sub>opt</sub><b>}</b></pre>

<pre>struct-member-declarations:
struct-member-declaration
struct-member-declarations   struct-member-declaration</pre>

<pre>struct-member-declaration:
constant-declaration
field-declaration
method-declaration
property-declaration
event-declaration
indexer-declaration
operator-declaration
constructor-declaration
static-constructor-declaration
type-declaration</pre>

Arrays

<pre>array-type:
non-array-type   rank-specifiers</pre>

<pre>non-array-type:
type</pre>

<pre>rank-specifiers:
rank-specifier
rank-specifiers   rank-specifier</pre>

<pre>rank-specifier:
<b>[</b>   dim-separators<sub>opt</sub><b>]</b></pre>

<pre>dim-separators:
<b>,</b>
dim-separators   <b>,</b></pre>

<pre>array-initializer:
<b>{</b>   variable-initializer-list<sub>opt</sub><b>}</b>
<b>{</b>   variable-initializer-list   <b>,</b><b>}</b></pre>

<pre>variable-initializer-list:
variable-initializer
variable-initializer-list   <b>,</b>   variable-initializer</pre>

<pre>variable-initializer:
expression
array-initializer</pre>

Interfaces

<pre>interface-declaration:
attributes<sub>opt</sub>   interface-modifiers<sub>opt</sub><b>partial</b><sub>opt</sub><b>interface</b>
        identifier   variant-type-parameter-list<sub>opt</sub>   interface-base<sub>opt</sub>
        type-parameter-constraints-clauses<sub>opt</sub>   interface-body   <b>;</b><sub>opt</sub></pre>

<pre>interface-modifiers:
interface-modifier
interface-modifiers   interface-modifier</pre>

<pre>interface-modifier:
<b>new</b>
<b>public</b>
<b>protected</b>
<b>internal</b>
<b>private</b></pre>

<pre>variant-type-parameter-list:
<b>&lt;</b>   variant-type-parameters   <b>&gt;</b></pre>

<pre>variant-type-parameters:
attributes<sub>opt</sub>  variance-annotation<sub>opt </sub> type-parameter
variant-type-parameters   <b>,</b>   attributes<sub>opt</sub>   variance-annotation<sub>opt</sub>  type-parameter</pre>

<pre>variance-annotation:
`*in*``
``*out*`</pre>

<pre>interface-base:
<b>:</b>   interface-type-list</pre>

<pre>interface-body:
<b>{</b>   interface-member-declarations<sub>opt</sub><b>}</b></pre>

<pre>interface-member-declarations:
interface-member-declaration
interface-member-declarations   interface-member-declaration</pre>

<pre>interface-member-declaration:
interface-method-declaration
interface-property-declaration
interface-event-declaration
interface-indexer-declaration</pre>

<pre>interface-method-declaration:
attributes<sub>opt</sub><b>new</b><sub>opt</sub>   return-type   identifier   type-parameter-list
<b>(</b>   formal-parameter-list<sub>opt</sub><b>)</b>   type-parameter-constraints-clauses<sub>opt</sub><b>;</b></pre>

<pre>interface-property-declaration:
attributes<sub>opt</sub><b>new</b><sub>opt</sub>   type   identifier   <b>{</b>   interface-accessors   <b>}</b></pre>

<pre>interface-accessors:
attributes<sub>opt</sub><b>get</b><b>;</b>
attributes<sub>opt</sub><b>set</b><b>;</b>
attributes<sub>opt</sub><b>get</b><b>;</b>   attributes<sub>opt</sub><b>set</b><b>;</b>
attributes<sub>opt</sub><b>set</b><b>;</b>   attributes<sub>opt</sub><b>get</b><b>;</b></pre>

<pre>interface-event-declaration:
attributes<sub>opt</sub><b>new</b><sub>opt</sub><b>event</b>   type   identifier   <b>;</b></pre>

<pre>interface-indexer-declaration:
attributes<sub>opt</sub><b>new</b><sub>opt</sub>   type   <b>this</b><b>[</b>   formal-parameter-list   <b>]</b><b>{</b>   interface-accessors   <b>}</b></pre>

Enums

<pre>enum-declaration:
attributes<sub>opt</sub>   enum-modifiers<sub>opt</sub><b>enum</b>   identifier   enum-base<sub>opt</sub>   enum-body   <b>;</b><sub>opt</sub></pre>

<pre>enum-base:
<b>:</b>   integral-type</pre>

<pre>enum-body:
<b>{</b>   enum-member-declarations<sub>opt</sub><b>}</b>
<b>{</b>   enum-member-declarations   <b>,</b><b>}</b></pre>

<pre>enum-modifiers:
enum-modifier
enum-modifiers   enum-modifier</pre>

<pre>enum-modifier:
<b>new</b>
<b>public</b>
<b>protected</b>
<b>internal</b>
<b>private</b></pre>

<pre>enum-member-declarations:
enum-member-declaration
enum-member-declarations   <b>,</b>   enum-member-declaration</pre>

<pre>enum-member-declaration:
attributes<sub>opt</sub>   identifier
attributes<sub>opt</sub>   identifier   <b>=</b>   constant-expression</pre>

Delegates

<pre>delegate-declaration:
attributes<sub>opt</sub>   delegate-modifiers<sub>opt</sub><b>delegate</b>   return-type   
        identifier  variant-type-parameter-list<sub>opt</sub>
<b>(</b>   formal-parameter-list<sub>opt</sub><b>)</b>   type-parameter-constraints-clauses<sub>opt</sub><b>;</b></pre>

<pre>delegate-modifiers:
delegate-modifier
delegate-modifiers   delegate-modifier</pre>

<pre>delegate-modifier:
<b>new</b>
<b>public</b>
<b>protected</b>
<b>internal</b>
<b>private</b></pre>

Attributes

<pre>global-attributes:
global-attribute-sections</pre>

<pre>global-attribute-sections:
global-attribute-section
global-attribute-sections   global-attribute-section</pre>

<pre>global-attribute-section:
<b>[</b>   global-attribute-target-specifier   attribute-list   <b>]</b>
<b>[</b>   global-attribute-target-specifier   attribute-list   <b>,</b><b>]</b></pre>

<pre>global-attribute-target-specifier:
global-attribute-target   <b>:</b></pre>

<pre>global-attribute-target:
<b>assembly</b>
<b>module</b></pre>

<pre>attributes:
attribute-sections</pre>

<pre>attribute-sections:
attribute-section
attribute-sections   attribute-section</pre>

<pre>attribute-section:
<b>[</b>   attribute-target-specifier<sub>opt</sub>   attribute-list   <b>]</b>
<b>[</b>   attribute-target-specifier<sub>opt</sub>   attribute-list   <b>,</b><b>]</b></pre>

<pre>attribute-target-specifier:
attribute-target   <b>:</b></pre>

<pre>attribute-target:
<b>field</b>
<b>event</b>
<b>method</b>
<b>param</b>
<b>property</b>
<b>return</b>
<b>type</b></pre>

<pre>attribute-list:
attribute
attribute-list   <b>,</b>   attribute</pre>

<pre>attribute:
attribute-name   attribute-arguments<sub>opt</sub></pre>

<pre>attribute-name:
 type-name</pre>

<pre>attribute-arguments:
<b>(</b>   positional-argument-list<sub>opt</sub><b>)</b>
<b>(</b>   positional-argument-list   <b>,</b>   named-argument-list   <b>)</b>
<b>(</b>   named-argument-list   <b>)</b></pre>

<pre>positional-argument-list:
positional-argument
positional-argument-list   <b>,</b>   positional-argument</pre>

<pre>positional-argument:
argument-name<sub>opt</sub>   attribute-argument-expression</pre>

<pre>named-argument-list:
named-argument
named-argument-list   <b>,</b>   named-argument</pre>

<pre>named-argument:
identifier   <b>=</b>   attribute-argument-expression</pre>

<pre>attribute-argument-expression:
expression</pre>

Grammar extensions for unsafe code

<pre>class-modifier:
...
<b>unsafe</b></pre>

<pre>struct-modifier:
...
<b>unsafe</b></pre>

<pre>interface-modifier:
...
<b>unsafe</b></pre>

<pre>delegate-modifier:
...
<b>unsafe</b></pre>

<pre>field-modifier:
...
<b>unsafe</b></pre>

<pre>method-modifier:
...
<b>unsafe</b></pre>

<pre>property-modifier:
...
<b>unsafe</b></pre>

<pre>event-modifier:
...
<b>unsafe</b></pre>

<pre>indexer-modifier:
...
<b>unsafe</b></pre>

<pre>operator-modifier:
...
<b>unsafe</b></pre>

<pre>constructor-modifier:
...
<b>unsafe</b></pre>

<pre>destructor-declaration:
attributes<sub>opt</sub><b>extern</b><sub>opt</sub><b>unsafe</b><sub>opt</sub><b>~</b>   identifier   <b>(</b><b>)</b>    destructor-body
attributes<sub>opt</sub><b>unsafe</b><sub>opt</sub><b>extern</b><sub>opt</sub><b>~</b>   identifier   <b>(</b><b>)</b>    destructor-body</pre>

<pre>static-constructor-modifiers:
<b>extern</b><sub>opt</sub><b>unsafe</b><sub>opt</sub><b>static</b>
<b>unsafe</b><sub>opt</sub><b>extern</b><sub>opt</sub><b>static</b>
<b>extern</b><sub>opt</sub><b>static</b><b>unsafe</b><sub>opt</sub>
<b>unsafe</b><sub>opt</sub><b>static</b><b>extern</b><sub>opt</sub>
<b>static</b><b>extern</b><sub>opt</sub><b>unsafe</b><sub>opt</sub>
<b>static</b><b>unsafe</b><sub>opt</sub><b>extern</b><sub>opt</sub></pre>

<pre>embedded-statement:
...
unsafe-statement 
fixed-statement</pre>

<pre>unsafe-statement:
<b>unsafe</b>   block</pre>

<pre>type:
...
pointer-type</pre>

<pre>pointer-type:
unmanaged-type   <b>*</b>
<b>void</b><b>*</b></pre>

<pre>unmanaged-type:
type</pre>

<pre>primary-no-array-creation-expression:
...
pointer-member-access
pointer-element-access
sizeof-expression</pre>

<pre>unary-expression:
...
pointer-indirection-expression
addressof-expression</pre>

<pre>pointer-indirection-expression:
<b>*</b>   unary-expression</pre>

<pre>pointer-member-access:
primary-expression   <b>-&gt;</b>   identifier  type-argument-list<sub>opt</sub></pre>

<pre>pointer-element-access:
primary-no-array-creation-expression   <b>[</b>   expression   <b>]</b></pre>

<pre>addressof-expression:
<b>&amp;</b>   unary-expression</pre>

<pre>sizeof-expression:
<b>sizeof</b><b>(</b>   unmanaged-type   <b>)</b></pre>

<pre>fixed-statement:
<b>fixed</b><b>(</b>   pointer-type   fixed-pointer-declarators   <b>)</b>   embedded-statement</pre>

<pre>fixed-pointer-declarators:
fixed-pointer-declarator
fixed-pointer-declarators   <b>,</b>   fixed-pointer-declarator</pre>

<pre>fixed-pointer-declarator:
identifier   <b>=</b>   fixed-pointer-initializer</pre>

<pre>fixed-pointer-initializer:
<b>&amp;</b>   variable-reference
expression</pre>

<pre>struct-member-declaration:
…
fixed-size-buffer-declaration</pre>

<pre>fixed-size-buffer-declaration:
attributes<sub>opt</sub>   fixed-size-buffer-modifiers<sub>opt</sub><b>fixed</b>   buffer-element-type
        fixed-size-buffer-declarators   <b>;</b></pre>

<pre>fixed-size-buffer-modifiers:
fixed-size-buffer-modifier
fixed-size-buffer-modifier   fixed-size-buffer-modifiers</pre>

<pre>fixed-size-buffer-modifier:
<b>new</b>
<b>public</b>
<b>protected</b>
<b>internal</b>
<b>private</b>
<b>unsafe</b></pre>

<pre>buffer-element-type:
type</pre>

<pre>fixed-size-buffer-declarators:
fixed-size-buffer-declarator
fixed-size-buffer-declarator   fixed-size-buffer-declarators</pre>

<pre>fixed-size-buffer-declarator:
identifier   <b>[</b>   constant-expression   <b>]</b></pre>

<pre>local-variable-initializer:
…
stackalloc-initializer</pre>

<pre>stackalloc-initializer:
<b>stackalloc</b>   unmanaged-type   <b>[</b>   expression   <b>]</b></pre>
