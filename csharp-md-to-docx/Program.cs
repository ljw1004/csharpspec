using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        var index_fn = args.Length >= 2 ? args[1] : "readme.md";
        if (!File.Exists(index_fn) || !File.Exists("template.docx") || Directory.GetFiles(".", "*.g4").Length > 1)
        {

            Console.Error.WriteLine("md2docx <filename>.md -- converts it to '<filename>.docx', based on 'template.docx'");
            Console.Error.WriteLine();
            Console.Error.WriteLine("If no file is specified:");
            Console.Error.WriteLine("    it looks for readme.md instead");
            Console.Error.WriteLine("If input file has a list with links of the form `* [Link](subfile.md)`:");
            Console.Error.WriteLine("   it converts the listed subfiles instead of <filename>.md");
            Console.Error.WriteLine("If the current directory contains one <grammar>.g4 file:");
            Console.Error.WriteLine("   it verifies all ```antlr blocks correspond, and also generates <grammar>.html");
            Console.Error.WriteLine("If 'template.docx' contains a Table of Contents:");
            Console.Error.WriteLine("   it replaces it with one based on the markdown (but page numbers aren't supported)");
            Environment.Exit(1);
        }

        // Read input file. If it contains a load of linked filenames, then read them instead.
        var readme = FSharp.Markdown.Markdown.Parse(File.ReadAllText(index_fn));
        var files = (from list in readme.Paragraphs.OfType<FSharp.Markdown.MarkdownParagraph.ListBlock>()
                     let items = list.Item2
                     from par in items
                     from spanpar in par.OfType<FSharp.Markdown.MarkdownParagraph.Span>()
                     let spans = spanpar.Item
                     from link in spans.OfType<FSharp.Markdown.MarkdownSpan.DirectLink>()
                     let url = link.Item2.Item1
                     where url.EndsWith(".md", StringComparison.InvariantCultureIgnoreCase)
                     where File.Exists(url) // TODO: remove this check
                     select url).ToList().Distinct();
        if (files.Count() == 0) files = new[] { index_fn };

        //var md = MarkdownSpec.ReadFiles(files)

        //' Now md.Gramar contains the grammar as extracted out of the *.md files, and moreover has
        //' correct references to within the spec. We'll check that it has the same productions as
        //' in the corresponding ANTLR file
        //Dim antlrfn = IO.Directory.GetFiles(".", "*.g4").FirstOrDefault
        //If antlrfn IsNot Nothing Then
        //    Dim htmlfn = IO.Path.ChangeExtension(antlrfn, ".html")
        //    Dim grammar = Antlr.ReadFile(antlrfn)
        //    If Not grammar.AreProductionsSameAs(md.Grammar) Then Throw New Exception("Grammar mismatch")
        //    md.Grammar.Name = grammar.Name ' because grammar name is derived from antlrfn, and can't be known from markdown
        //    Html.WriteFile(md.Grammar, htmlfn)
        //    Process.Start(htmlfn)
        //End If

        //' Generate the Specification.docx file
        //Dim fn = PickUniqueFilename(IO.Path.ChangeExtension(ifn, ".docx"))
        //md.WriteFile("template.docx", fn)
        //Process.Start(fn)

    }


    static string PickUniqueFilename(string suggestion)
    { 
        var orig = Path.GetFileNameWithoutExtension(suggestion);
        var ext = Path.GetExtension(suggestion);

        for (int i=0; true; i++)
        {
            var fn = orig + (i == 0 ? "" : i.ToString()) + ext;
            if (!File.Exists(fn)) return fn;
            try { File.Delete(fn); return fn; }
            catch (Exception) { }
        }
    }


    public static T Option<T>(this Microsoft.FSharp.Core.FSharpOption<T> o) where T:class
    {
        if (Microsoft.FSharp.Core.FSharpOption<T>.GetTag(o) == Microsoft.FSharp.Core.FSharpOption<T>.Tags.None) return null;
        return o.Value;
    }

}