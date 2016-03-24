Imports Microsoft.CodeAnalysis

Module Module1

    Sub Main()
        Dim args = Environment.GetCommandLineArgs
        Dim ifn = If(args.Length >= 2, args(1), "readme.md")

        If Not IO.File.Exists(ifn) OrElse
           Not IO.File.Exists("template.docx") OrElse
           IO.Directory.GetFiles(".", "*.g4").Length > 1 Then
            Console.Error.WriteLine("md2docx <filename>.md -- converts it to '<filename>.docx', based on 'template.docx'")
            Console.Error.WriteLine()
            Console.Error.WriteLine("If no file is specified:")
            Console.Error.WriteLine("    it looks for readme.md instead")
            Console.Error.WriteLine("If input file has a list with links of the form `* [Link](subfile.md)`:")
            Console.Error.WriteLine("   it converts the listed subfiles instead of <filename>.md")
            Console.Error.WriteLine("If the current directory contains one <grammar>.g4 file:")
            Console.Error.WriteLine("   it verifies all ```antlr blocks correspond, and also generates <grammar>.html")
            Console.Error.WriteLine("If 'template.docx' contains a Table of Contents:")
            Console.Error.WriteLine("   it replaces it with one based on the markdown (but page numbers aren't supported)")
            Environment.Exit(1)
        End If

        ' Read input file. If it contains a load of linked filenames, then read them instead.
        Dim readme = FSharp.Markdown.Markdown.Parse(IO.File.ReadAllText(ifn))
        Dim files = (From list In readme.Paragraphs.OfType(Of FSharp.Markdown.MarkdownParagraph.ListBlock)
                     Let items = list.Item2
                     From par In items
                     From spanpar In par.OfType(Of FSharp.Markdown.MarkdownParagraph.Span)
                     Let spans = spanpar.Item
                     From link In spans.OfType(Of FSharp.Markdown.MarkdownSpan.DirectLink)
                     Let url = link.Item2.Item1
                     Where url.EndsWith(".md", StringComparison.InvariantCultureIgnoreCase)
                     Where IO.File.Exists(url) ' TODO: remove this
                     Select url).ToList.Distinct
        If files.Count = 0 Then files = {ifn}
        'Dim md = MarkdownSpec.ReadFiles(files)

        Dim md = MarkdownSpec.ReadString("
### Multiplication operator

For an operation of the form `x``*``y`, binary operator overload resolution (§7.3.4) is applied to select a specific operator implementation. The operands are converted to the parameter types of the selected operator, and the type of the result is the return type of the operator.

The predefined multiplication operators are listed below. The operators all compute the product of `x` and `y`.

*  Integer multiplication:

   ```csharp
   int operator *(int x, int y);
   uint operator *(uint x, uint y);
   long operator *(long x, long y);
   ulong operator *(ulong x, ulong y);
   ```

   In a `checked` context, if the product is outside the range of the result type, a `System.OverflowException` is thrown. In an `unchecked` context, overflows are not reported and any significant high-order bits outside the range of the result type are discarded.


*  Floating-point multiplication:

   ```csharp
   float operator *(float x, float y);
   double operator *(double x, double y);
   ```

   The product is computed according to the rules of IEEE 754 arithmetic. The following table lists the results of all possible combinations of nonzero finite values, zeros, infinities, and NaN's. In the table, `x` and `y` are positive finite values. `z` is the result of `x * y`. If the result is too large for the destination type, `z` is infinity. If the result is too small for the destination type, `z` is zero.

   |:----:|-----:|:----:|:---:|:---:|:----:|:----:|:----|
   |      | +y   | -y   | +0  | -0  | +inf | -inf | NaN | 
   | +x   | +z   | -z   | +0  | -0  | +inf | -inf | NaN | 
   | -x   | -z   | +z   | -0  | +0  | -inf | +inf | NaN | 
   | +0   | +0   | -0   | +0  | -0  | NaN  | NaN  | NaN | 
   | -0   | -0   | +0   | -0  | +0  | NaN  | NaN  | NaN | 
   | +inf | +inf | -inf | NaN | NaN | +inf | -inf | NaN | 
   | -inf | -inf | +inf | NaN | NaN | -inf | +inf | NaN | 
   | NaN  | NaN  | NaN  | NaN | NaN | NaN  | NaN  | NaN | 

*  Decimal multiplication:

   ```csharp
   decimal operator *(decimal x, decimal y);
   ```

   If the resulting value is too large to represent in the `decimal` format, a `System.OverflowException` is thrown. If the result value is too small to represent in the `decimal` format, the result is zero. The scale of the result, before any rounding, is the sum of the scales of the two operands.

   Decimal multiplication is equivalent to using the multiplication operator of type `System.Decimal`.


### Division operator

For an operation of the form `x / y`, binary operator overload resolution (§7.3.4) is applied to select a specific operator implementation. The operands are converted to the parameter types of the selected operator, and the type of the result is the return type of the operator.

The predefined division operators are listed below. The operators all compute the quotient of `x` and `y`.

*  Integer division:

   ```csharp
   int operator /(int x, int y);
   uint operator /(uint x, uint y);
   long operator /(long x, long y);
   ulong operator /(ulong x, ulong y);
   ```

   If the value of the right operand is zero, a `System.DivideByZeroException` is thrown.

   The division rounds the result towards zero. Thus the absolute value of the result is the largest possible integer that is less than or equal to the absolute value of the quotient of the two operands. The result is zero or positive when the two operands have the same sign and zero or negative when the two operands have opposite signs.

   If the left operand is the smallest representable `int` or `long` value and the right operand is `-1`, an overflow occurs. In a `checked` context, this causes a `System.ArithmeticException` (or a subclass thereof) to be thrown. In an `unchecked` context, it is implementation-defined as to whether a `System.ArithmeticException` (or a subclass thereof) is thrown or the overflow goes unreported with the resulting value being that of the left operand.

*  Floating-point division:

   ```csharp
   float operator /(float x, float y);
   double operator /(double x, double y);
   ```

   The quotient is computed according to the rules of IEEE 754 arithmetic. The following table lists the results of all possible combinations of nonzero finite values, zeros, infinities, and NaN's. In the table, `x` and `y` are positive finite values. `z` is the result of `x / y`. If the result is too large for the destination type, `z` is infinity. If the result is too small for the destination type, `z` is zero.

   |:----:|:----:|:----:|:----:|:----:|:----:|:----:|:----:|
   |      | +y   | -y   | +0   | -0   | +inf | -inf | NaN  | 
   | +x   | +z   | -z   | +inf | -inf | +0   | -0   | NaN  | 
   | -x   | -z   | +z   | -inf | +inf | -0   | +0   | NaN  | 
   | +0   | +0   | -0   | NaN  | NaN  | +0   | -0   | NaN  | 
   | -0   | -0   | +0   | NaN  | NaN  | -0   | +0   | NaN  | 
   | +inf | +inf | -inf | +inf | -inf | NaN  | NaN  | NaN  | 
   | -inf | -inf | +inf | -inf | +inf | NaN  | NaN  | NaN  | 
   | NaN  | NaN  | NaN  | NaN  | NaN  | NaN  | NaN  | NaN  | 

*  Decimal division:

   ```csharp
   decimal operator /(decimal x, decimal y);
   ```

   If the value of the right operand is zero, a `System.DivideByZeroException` is thrown. If the resulting value is too large to represent in the `decimal` format, a `System.OverflowException` is thrown. If the result value is too small to represent in the `decimal` format, the result is zero. The scale of the result is the smallest scale that will preserve a result equal to the nearest representantable decimal value to the true mathematical result.

   Decimal division is equivalent to using the division operator of type `System.Decimal`.


### Remainder operator

For an operation of the form `x % y`, binary operator overload resolution (§7.3.4) is applied to select a specific operator implementation. The operands are converted to the parameter types of the selected operator, and the type of the result is the return type of the operator.

The predefined remainder operators are listed below. The operators all compute the remainder of the division between `x` and `y`.

*  Integer remainder:

   ```csharp
   int operator %(int x, int y);
   uint operator %(uint x, uint y);
   long operator %(long x, long y);
   ulong operator %(ulong x, ulong y);
   ```

   The result of `x``%``y` is the value produced by `x``-``(x``/``y)``*``y`. If `y` is zero, a `System.DivideByZeroException` is thrown.

   If the left operand is the smallest `int` or `long` value and the right operand is `-1`, a `System.``Overflow``Exception` is thrown. In no case does `x``%``y` throw an exception where `x``/``y` would not throw an exception.

*  Floating-point remainder:

   ```csharp
   float operator %(float x, float y);
   double operator %(double x, double y);
   ```

   The following table lists the results of all possible combinations of nonzero finite values, zeros, infinities, and NaN's. In the table, `x` and `y` are positive finite values. `z` is the result of `x % y` and is computed as `x - n * y`, where `n` is the largest possible integer that is less than or equal to `x / y`. This method of computing the remainder is analogous to that used for integer operands, but differs from the IEEE 754 definition (in which `n` is the integer closest to `x / y`).

   |:----:|:----:|:----:|:----:|:----:|:----:|:----:|:----:|
   | NaN  | +y   | -y   | +0   | -0   | +inf | -inf | NaN  | 
   | +x   | +z   | +z   | NaN  | NaN  | x    | x    | NaN  | 
   | -x   | -z   | -z   | NaN  | NaN  | -x   | -x   | NaN  | 
   | +0   | +0   | +0   | NaN  | NaN  | +0   | +0   | NaN  | 
   | -0   | -0   | -0   | NaN  | NaN  | -0   | -0   | NaN  | 
   | +inf | NaN  | NaN  | NaN  | NaN  | NaN  | NaN  | NaN  | 
   | -inf | NaN  | NaN  | NaN  | NaN  | NaN  | NaN  | NaN  | 
   | NaN  | NaN  | NaN  | NaN  | NaN  | NaN  | NaN  | NaN  | 

*  Decimal remainder:

   ```csharp
   decimal operator %(decimal x, decimal y);
   ```

   If the value of the right operand is zero, a `System.DivideByZeroException` is thrown. The scale of the result, before any rounding, is the larger of the scales of the two operands, and the sign of the result, if non-zero, is the same as that of `x`.

   Decimal remainder is equivalent to using the remainder operator of type `System.Decimal`.


### Addition operator

For an operation of the form `x + y`, binary operator overload resolution (§7.3.4) is applied to select a specific operator implementation. The operands are converted to the parameter types of the selected operator, and the type of the result is the return type of the operator.

The predefined addition operators are listed below. For numeric and enumeration types, the predefined addition operators compute the sum of the two operands. When one or both operands are of type string, the predefined addition operators concatenate the string representation of the operands.

*  Integer addition:

   ```csharp
   int operator +(int x, int y);
   uint operator +(uint x, uint y);
   long operator +(long x, long y);
   ulong operator +(ulong x, ulong y);
   ```

   In a `checked` context, if the sum is outside the range of the result type, a `System.OverflowException` is thrown. In an `unchecked` context, overflows are not reported and any significant high-order bits outside the range of the result type are discarded.

*  Floating-point addition:

   ```csharp
   float operator +(float x, float y);
   double operator +(double x, double y);
   ```

   The sum is computed according to the rules of IEEE 754 arithmetic. The following table lists the results of all possible combinations of nonzero finite values, zeros, infinities, and NaN's. In the table, `x` and `y` are nonzero finite values, and `z` is the result of `x``+``y`. If `x` and `y` have the same magnitude but opposite signs, `z` is positive zero. If `x``+``y` is too large to represent in the destination type, `z` is an infinity with the same sign as `x``+``y`.

   |:-----|:-----|:-----|:-----|:-----|:-----|:-----|
   | NaN  | y    | +0   | -0   | +inf | -inf | NaN  | 
   | x    | z    | x    | x    | +inf | -inf | NaN  | 
   | +0   | y    | +0   | +0   | +inf | -inf | NaN  | 
   | -0   | y    | +0   | -0   | +inf | -inf | NaN  | 
   | +inf | +inf | +inf | +inf | +inf | NaN  | NaN  | 
   | -inf | -inf | -inf | -inf | NaN  | -inf | NaN  | 
   | NaN  | NaN  | NaN  | NaN  | NaN  | NaN  | NaN  | 

*  Decimal addition:

   ```csharp
   decimal operator +(decimal x, decimal y);
   ```

   If the resulting value is too large to represent in the `decimal` format, a `System.OverflowException` is thrown. The scale of the result, before any rounding, is the larger of the scales of the two operands.

   Decimal addition is equivalent to using the addition operator of type `System.Decimal`.

*  Enumeration addition. Every enumeration type implicitly provides the following predefined operators, where `E` is the enum type, and `U` is the underlying type of `E`:

   ```csharp
   E operator +(E x, U y);
   E operator +(U x, E y);
   ```

   At run-time these operators are evaluated exactly as `(E)((U)x + (U)y)`.

*  String concatenation:

   ```csharp
   string operator +(string x, string y);
   string operator +(string x, object y);
   string operator +(object x, string y);
   ```

   These overloads of the binary `+` operator perform string concatenation. If an operand of string concatenation is `null`, an empty string is substituted. Otherwise, any non-string argument is converted to its string representation by invoking the virtual `ToString` method inherited from type `object`. If `ToString` returns `null`, an empty string is substituted.

   ```csharp
   using System;
   
   class Test
   {
       static void Main() {
           string s = null;
           Console.WriteLine('s = >' + s + '<');        // displays s = ><
           int i = 1;
           Console.WriteLine('i = ' + i);                // displays i = 1
           float f = 1.2300E+15F;
           Console.WriteLine('f = ' + f);                // displays f = 1.23E+15
           decimal d = 2.900m;
           Console.WriteLine('d = ' + d);                // displays d = 2.900
       }
   }
   ```

   The result of the string concatenation operator is a string that consists of the characters of the left operand followed by the characters of the right operand. The string concatenation operator never returns a `null` value. A `System.OutOfMemoryException` may be thrown if there is not enough memory available to allocate the resulting string.

*  Delegate combination. Every delegate type implicitly provides the following predefined operator, where `D` is the delegate type:

   ```csharp
   D operator `+`(D x, D y);
   ```

   The binary `+` operator performs delegate combination when both operands are of some delegate type `D`. (If the operands have different delegate types, a binding-time error occurs.) If the first operand is `null`, the result of the operation is the value of the second operand (even if that is also `null`). Otherwise, if the second operand is `null`, then the result of the operation is the value of the first operand. Otherwise, the result of the operation is a new delegate instance that, when invoked, invokes the first operand and then invokes the second operand. For examples of delegate combination, see §7.8.5 and §15.4. Since `System.Delegate` is not a delegate type, `operator` `+` is not defined for it.

### Subtraction operator
")


        ' Now md.Gramar contains the grammar as extracted out of the *.md files, and moreover has
        ' correct references to within the spec. We'll check that it has the same productions as
        ' in the corresponding ANTLR file
        Dim antlrfn = IO.Directory.GetFiles(".", "*.g4").FirstOrDefault
        If antlrfn IsNot Nothing Then
            Dim htmlfn = IO.Path.ChangeExtension(antlrfn, ".html")
            Dim grammar = Antlr.ReadFile(antlrfn)
            If Not grammar.AreProductionsSameAs(md.Grammar) Then Throw New Exception("Grammar mismatch")
            md.Grammar.Name = grammar.Name ' because grammar name is derived from antlrfn, and can't be known from markdown
            Html.WriteFile(md.Grammar, htmlfn)
            Process.Start(htmlfn)
        End If

        ' Generate the Specification.docx file
        Dim fn = PickUniqueFilename(IO.Path.ChangeExtension(ifn, ".docx"))
        md.WriteFile("template.docx", fn)
        Process.Start(fn)

    End Sub


    Function PickUniqueFilename(suggestion As String) As String
        Dim base = IO.Path.GetFileNameWithoutExtension(suggestion)
        Dim ext = IO.Path.GetExtension(suggestion)

        Dim ifn = 0
        Do
            Dim fn = base & If(ifn = 0, "", CStr(ifn)) & ext
            If Not IO.File.Exists(fn) Then Return fn
            Try
                IO.File.Delete(fn) : Return fn
            Catch ex As Exception
            End Try
            ifn += 1
        Loop
    End Function

End Module

