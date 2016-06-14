using CSharp2Colorized;
using Grammar2Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class Antlr
{


    public static string ToString(Grammar grammar)
    {
        var r = $"grammar {grammar.Name};\r\n";
        foreach (var p in grammar.Productions) r += ToString(p);
        return r;
    }

    public static string ToString(Production p)
    {
        if (p.EBNF == null && string.IsNullOrEmpty(p.Comment))
        {
            return "\r\n";
        }
        else if (p.EBNF == null)
        {
            return $"//{p.Comment}\r\n";
        }
        else
        {
            var r = $"{p.ProductionName}:";
            if (p.RuleStartsOnNewLine) r += "\r\n";
            r += "\t";
            if (p.RuleStartsOnNewLine) r += "| ";
            r += $"{ToString(p.EBNF)};";
            if (!string.IsNullOrEmpty(p.Comment)) r += $"  //{p.Comment}";
            r += "\r\n";
            return r;
        }
    }

    public static string ToString(EBNF ebnf)
    {
        var r = "";
        EBNF prevElement = null;
        switch (ebnf.Kind)
        {
            case EBNFKind.Terminal:
                r = $"'{ebnf.s.Replace("\\", "\\\\").Replace("'", "\\'")}'";
                break;
            case EBNFKind.ExtendedTerminal:
                r = $"'<{ebnf.s.Replace("\\", "\\\\").Replace("'", "\\'")}>'";
                break;
            case EBNFKind.Reference:
                r = ebnf.s;
                break;
            case EBNFKind.OneOrMoreOf:
            case EBNFKind.ZeroOrMoreOf:
            case EBNFKind.ZeroOrOneOf:
                var op = (ebnf.Kind == EBNFKind.OneOrMoreOf ? "+" : (ebnf.Kind == EBNFKind.ZeroOrMoreOf ? "*" : "?"));
                if (ebnf.Children[0].Kind == EBNFKind.Choice || ebnf.Children[0].Kind == EBNFKind.Sequence)
                    r = $"( {ToString(ebnf.Children[0])} ){op}";
                else
                    r = $"{ToString(ebnf.Children[0])}{op}";
                break;
            case EBNFKind.Choice:
                foreach (var c in ebnf.Children)
                {
                    if (prevElement != null) r += (r.Last() == '\t' ? "| " : " | ");
                    r += ToString(c);
                    prevElement = c;
                }
                break;
            case EBNFKind.Sequence:
                foreach (var c in ebnf.Children)
                {
                    if (prevElement != null) r += (r == "" || r.Last() == '\t' ? "" : " ");
                    if (c.Kind == EBNFKind.Choice) r += "( " + ToString(c) + " )"; else r += ToString(c);
                    prevElement = c;
                }
                break;
            default:
                r = "???";
                break;
        }
        if (!string.IsNullOrEmpty(ebnf.FollowingComment)) r += " //" + ebnf.FollowingComment;
        if (ebnf.FollowingNewline) r += "\r\n\t";
        return r;
    }


    public static IEnumerable<ColorizedLine> ColorizeAntlr(string antlr)
    {
        var grammar = ReadString(antlr, "dummyGrammarName");
        return Colorize.Words2Lines(ColorizeAntlr(grammar));
    }

    private static IEnumerable<ColorizedWord> ColorizeAntlr(Grammar grammar)
    {
        foreach (var p in grammar.Productions) foreach (var word in ColorizeAntlr(p)) yield return word;
    }

    private static IEnumerable<ColorizedWord> ColorizeAntlr(Production p)
    {
        if (p.EBNF == null && string.IsNullOrEmpty(p.Comment))
        {
            yield return null;
        }
        else if (p.EBNF == null)
        {
            yield return Col("// " + p.Comment, "Comment");
            yield return null;
        }
        else
        {
            yield return Col(p.ProductionName, "Production");
            yield return Col(":", "PlainText");
            if (p.RuleStartsOnNewLine) { yield return null; yield return Col("\t| ", "PlainText"); }
            else yield return Col(" ", "PlainText");
            foreach (var word in ColorizeAntlr(p.EBNF)) yield return word;
            yield return Col(";", "PlainText");
            if (!string.IsNullOrEmpty(p.Comment)) yield return Col("  //" + p.Comment, "Comment");
            yield return null;
        }
    }

    public static IEnumerable<ColorizedWord> ColorizeAntlr(EBNF ebnf)
    {
        var lastWasTab = false;
        EBNF prevElement = null;
        switch (ebnf.Kind)
        {
            case EBNFKind.Terminal:
                yield return Col("'" + ebnf.s.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\\\"", "\"") + "'", "Terminal");
                break;
            case EBNFKind.ExtendedTerminal:
                yield return Col(ebnf.s, "ExtendedTerminal");
                break;
            case EBNFKind.Reference:
                yield return Col(ebnf.s, "Production");
                break;
            case EBNFKind.OneOrMoreOf:
            case EBNFKind.ZeroOrMoreOf:
            case EBNFKind.ZeroOrOneOf:
                var op = (ebnf.Kind == EBNFKind.OneOrMoreOf ? "+" : (ebnf.Kind == EBNFKind.ZeroOrMoreOf ? "*" : "?"));
                if (ebnf.Children[0].Kind == EBNFKind.Choice || ebnf.Children[0].Kind == EBNFKind.Sequence)
                {
                    yield return Col("( ", "PlainText");
                    foreach (var word in ColorizeAntlr(ebnf.Children[0])) yield return word;
                    yield return Col(" )", "PlainText");
                    yield return Col(op, "PlainText");
                }
                else
                {
                    foreach (var word in ColorizeAntlr(ebnf.Children[0])) yield return word;
                    yield return Col(op, "PlainText");
                }
                break;
            case EBNFKind.Choice:
                foreach (var c in ebnf.Children)
                {
                    if (prevElement != null) yield return Col(lastWasTab ? "| " : "| ", "PlainText");
                    foreach (var word in ColorizeAntlr(c)) { yield return word; lastWasTab = (word?.Text == "\t"); }
                    prevElement = c;
                }
                break;
            case EBNFKind.Sequence:
                foreach (var c in ebnf.Children)
                {
                    if (lastWasTab) yield return Col("  ", "PlainText");
                    if (c.Kind == EBNFKind.Choice)
                    {
                        yield return Col("( ", "PlainText");
                        foreach (var word in ColorizeAntlr(c)) yield return word;
                        yield return Col(" )", "PlainText");
                        lastWasTab = false;
                    }
                    else
                    {
                        foreach (var word in ColorizeAntlr(c)) { yield return word; lastWasTab = (word?.Text == "\t"); }
                    }
                    prevElement = c;
                }
                break;
            default:
                throw new NotSupportedException("Unrecognized EBNF");
        }
        if (!string.IsNullOrEmpty(ebnf.FollowingWhitespace)) yield return Col(ebnf.FollowingWhitespace, "Comment");
        if (!string.IsNullOrEmpty(ebnf.FollowingComment)) yield return Col(" //" + ebnf.FollowingComment, "Comment");
        if (ebnf.FollowingNewline) { yield return null; yield return Col("\t", "PlainText"); }
    }

    private static ColorizedWord Col(string token, string color)
    {
        switch (color)
        {
            case "PlainText" : return new ColorizedWord { Text = token };
            case "Production" : return new ColorizedWord { Text = token, Red = 106, Green = 90, Blue = 205 };
            case "Comment" : return new ColorizedWord { Text = token, Green = 128 };
            case "Terminal" : return new ColorizedWord { Text = token, Red = 163, Green = 21, Blue = 21 };
            case "ExtendedTerminal" : return new ColorizedWord { Text = token, IsItalic = true };
            default: throw new Exception("bad color name");
        }
    }




    public static Grammar ReadFile(string fn)
    {
        return ReadString(File.ReadAllText(fn), Path.GetFileNameWithoutExtension(fn));
    }

    public static Grammar ReadString(string src, string grammarName)
    {
        return new Grammar { Productions = ReadInternal(src).ToList(), Name = grammarName };
    }

    private static IEnumerable<Production> ReadInternal(string src)
    {
        var tokens = Tokenize(src);
        while (tokens.Any())
        {
            var t = tokens.First.Value;  tokens.RemoveFirst();
            if (t == "grammar")
            {
                while (tokens.Any() && tokens.First.Value != ";") tokens.RemoveFirst();
                if (tokens.Any() && tokens.First.Value == ";") tokens.RemoveFirst();
                if (tokens.Any() && tokens.First.Value == "\r\n") tokens.RemoveFirst();
            }
            else if (t.StartsWith("//"))
            {
                yield return new Production { Comment = t.Substring(2) };
                if (tokens.Any() && tokens.First.Value == "\r\n") tokens.RemoveFirst();
            }
            else if (t == "\r\n")
            {
                yield return new Production();
            }
            else if (string.IsNullOrWhiteSpace(t))
            {
                // skip
            }
            else
            {
                var whitespace = "";
                var comment = "";
                var newline = false;
                while (tokens.Any() && string.IsNullOrWhiteSpace(tokens.First.Value))
                {
                    if (tokens.First.Value == "\r\n") newline = true;
                    tokens.RemoveFirst();

                }
                if (tokens.First.Value != ":") throw new Exception($"After '{t}' expected ':' not {tokens.First.Value}");
                tokens.RemoveFirst();
                GobbleUpComments(tokens, ref whitespace, ref comment, ref newline);
                var p = ParseProduction(tokens, ref whitespace, ref comment);
                GobbleUpComments(tokens, ref p.FollowingWhitespace, ref p.FollowingComment, ref p.FollowingNewline);
                if (tokens.Any() && tokens.First.Value == ";") tokens.RemoveFirst();
                if (tokens.Any() && tokens.First.Value == "\r\n") tokens.RemoveFirst();
                var production = new Production { Comment = comment, EBNF = p, ProductionName = t, RuleStartsOnNewLine = newline };
                while (tokens.Any() && tokens.First.Value.StartsWith("//"))
                {
                    production.Comment += tokens.First.Value.Substring(2); tokens.RemoveFirst();
                    if (tokens.First.Value == "\r\n") tokens.RemoveFirst();
                }
                yield return production;
            }
        }
    }

    private static LinkedList<string> Tokenize(string s)
    {
        s = s.Trim();
        var tokens = new LinkedList<String>();
        var pos = 0;

        while (pos < s.Length)
        {
            if (pos + 1 < s.Length && s.Substring(pos, 2) == "\r\n")
            {
                tokens.AddLast("\r\n"); pos += 2;
            }
            else if (":*?|+;()\r\n".Contains(s[pos]))
            {
                tokens.AddLast(s[pos].ToString()); pos++;
            }
            else if (pos + 1 < s.Length && s.Substring(pos, 2) == "//")
            {
                pos += 2;
                var t = "";
                while (pos < s.Length && !"\r\n".Contains(s[pos])) { t += s[pos]; pos += 1; }
                if (t.Contains("*)")) throw new Exception("Comments may not include *)");
                tokens.AddLast("//" + t);
            }
            else if (s[pos] == '\'')
            {
                var t = ""; pos++;
                while (pos < s.Length && s.Substring(pos, 1) != "'")
                {
                    if (s.Substring(pos, 2) == "\\\\") { t += "\\"; pos += 2; }
                    else if (s.Substring(pos, 2) == "\\'") { t += "'"; pos += 2; }
                    else if (s.Substring(pos, 2) == "\\\"") { t += "\""; pos += 2; }
                    else if (s.Substring(pos, 1) == "\\") throw new Exception("Terminals may not include \\ except in \\\\ or \\' or \\\"");
                    else { t += s[pos]; pos++; }
                }
                if (t.Contains("\r") || t.Contains("\n")) throw new Exception("Terminals must be single-line");
                tokens.AddLast("'" + t + "'"); pos++;
            }
            else
            {
                var t = "";
                while (pos < s.Length && !string.IsNullOrWhiteSpace(s[pos].ToString())
                    && !":*?;\r\n'()+".Contains(s[pos]) && (pos + 1 >= s.Length || s.Substring(pos, 2) != "//"))
                {
                    t += s[pos]; pos++;
                }
                tokens.AddLast(t);
            }
            // Bump up to the next non-whitespace character:
            var whitespace = "";
            while (pos<s.Length && !"\r\n".Contains(s[pos]) && string.IsNullOrWhiteSpace(s[pos].ToString()))
            {
                whitespace += s[pos]; pos++;
            }
            if (whitespace != "") tokens.AddLast(whitespace);
        }

        return tokens;
    }


    private static void GobbleUpComments(LinkedList<string> tokens, ref string ExtraWhitespace, ref string ExtraComments, ref bool HasNewline)
    {
        if (tokens.Count == 0) return;
        while (true)
        {
            if (tokens.First.Value.StartsWith("//")) { ExtraComments += tokens.First.Value.Substring(2); tokens.RemoveFirst(); }
            else if (tokens.First.Value == "\r\n") { HasNewline = true; tokens.RemoveFirst(); if (ExtraComments.Length > 0) ExtraComments += " "; }
            else if (string.IsNullOrWhiteSpace(tokens.First.Value)) { ExtraWhitespace += tokens.First.Value; tokens.RemoveFirst(); }
            else break;
        }
        ExtraComments = ExtraComments.TrimEnd();
    }

    static bool dummy;

    private static EBNF ParseProduction(LinkedList<string> tokens, ref string ExtraWhitespace, ref string ExtraComments)
    {
        if (tokens.Count == 0) throw new Exception("empty input stream");
        GobbleUpComments(tokens, ref ExtraWhitespace, ref ExtraComments, ref dummy);
        return ParsePar(tokens, ref ExtraWhitespace, ref ExtraComments);
    }

    private static EBNF ParsePar(LinkedList<string> tokens, ref string ExtraWhitespace, ref string ExtraComments)
    {
        var pp = new LinkedList<EBNF>();
        if (tokens.First.Value == "|") { tokens.RemoveFirst(); GobbleUpComments(tokens, ref ExtraWhitespace, ref ExtraComments, ref dummy); }
        pp.AddLast(ParseSeq(tokens, ref ExtraWhitespace, ref ExtraComments));
        while (tokens.Any() && tokens.First.Value == "|")
        {
            tokens.RemoveFirst();
            GobbleUpComments(tokens, ref ExtraWhitespace, ref ExtraComments, ref pp.Last.Value.FollowingNewline);
            pp.AddLast(ParseSeq(tokens, ref ExtraWhitespace, ref ExtraComments));
        }
        if (pp.Count == 1) return pp.First.Value;
        return new EBNF { Kind = EBNFKind.Choice, Children = pp.ToList() };
    }

    private static EBNF ParseSeq(LinkedList<string> tokens, ref string ExtraWhitespace, ref string ExtraComments)
    {
        var pp = new LinkedList<EBNF>();
        pp.AddLast(ParseUnary(tokens, ref ExtraWhitespace, ref ExtraComments));
        while (tokens.Any() && tokens.First.Value != "|" && tokens.First.Value != ";" && tokens.First.Value != ")")
        {
            GobbleUpComments(tokens, ref ExtraWhitespace, ref ExtraComments, ref pp.Last.Value.FollowingNewline);
            pp.AddLast(ParseUnary(tokens, ref ExtraWhitespace, ref ExtraComments));
        }
        if (pp.Count == 1) return pp.First.Value;
        return new EBNF { Kind = EBNFKind.Sequence, Children = pp.ToList() };
    }

    private static EBNF ParseUnary(LinkedList<string> tokens, ref string ExtraWhitespace, ref string ExtraComments)
    {
        var p = ParseAtom(tokens, ref ExtraWhitespace, ref ExtraComments);
        while (tokens.Any())
        {
            if (tokens.First.Value == "+")
            {
                tokens.RemoveFirst();
                p = new EBNF {Kind = EBNFKind.OneOrMoreOf, Children = new[] { p}.ToList()};
                GobbleUpComments(tokens, ref p.FollowingWhitespace, ref p.FollowingComment, ref p.FollowingNewline);
            }
            else if (tokens.First.Value == "*")
            {
                tokens.RemoveFirst();
                p = new EBNF {Kind = EBNFKind.ZeroOrMoreOf, Children = new[] { p}.ToList()};
                GobbleUpComments(tokens, ref p.FollowingWhitespace, ref p.FollowingComment, ref p.FollowingNewline);
            }
            else if (tokens.First.Value == "?")
            {
                tokens.RemoveFirst();
                p = new EBNF { Kind = EBNFKind.ZeroOrOneOf, Children = new[] { p }.ToList() };
                GobbleUpComments(tokens, ref p.FollowingWhitespace, ref p.FollowingComment, ref p.FollowingNewline);
            }
            else
            {
                break;
            }
        }
        return p;
    }

    private static EBNF ParseAtom(LinkedList<string> tokens, ref string ExtraWhitespace, ref string ExtraComments)
    {
        if (tokens.First.Value == "(")
        {
            tokens.RemoveFirst();
            var p = ParseProduction(tokens, ref ExtraWhitespace, ref ExtraComments);
            if (tokens.Count == 0 || tokens.First.Value != ")") throw new Exception("mismatched parentheses");
            tokens.RemoveFirst();
            GobbleUpComments(tokens, ref p.FollowingWhitespace, ref p.FollowingComment, ref p.FollowingNewline);
            return p;
        }
        else if (tokens.First.Value.StartsWith("'"))
        {
            var t = tokens.First.Value; tokens.RemoveFirst();
            t = t.Substring(1, t.Length - 2);
            var p = new EBNF { Kind = EBNFKind.Terminal, s = t };
            if (t.StartsWith("<") && t.EndsWith(">"))
            {
                p.Kind = EBNFKind.ExtendedTerminal; p.s = t.Substring(1, t.Length - 2);
                if (p.s.Contains("?")) throw new Exception("A special-terminal may not contain a question-mark '?'");
                if (p.s == "") throw new Exception("A terminal may not be '<>'");
            }
            else
            {
                if (t.Contains("'") && t.Contains("\"")) throw new Exception("A terminal must either contain no ' or no \"");
            }
            GobbleUpComments(tokens, ref p.FollowingWhitespace, ref p.FollowingComment, ref p.FollowingNewline);
            return p;
        }
        else
        {
            var t = tokens.First.Value; tokens.RemoveFirst();
            var p = new EBNF { Kind = EBNFKind.Reference, s = t };
            GobbleUpComments(tokens, ref p.FollowingWhitespace, ref p.FollowingComment, ref p.FollowingNewline);
            return p;
        }
    }

}

