' TODO:
'
' * Turn the github repro into a published one, check in html+docx, and add hyperlinks to them in readme.md
' * Make a better "report error" thing, and distinguish fatal from non-fatal (e.g. duplicate section title)
' * Factor into a VB "grammar2html" library, a C# "colorize-csharp" NuGet + command-line util, and md2docx

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
                     Select url).ToList.Distinct
        If files.Count = 0 Then files = {ifn}
        Dim md = MarkdownSpec.ReadFiles(files)


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

