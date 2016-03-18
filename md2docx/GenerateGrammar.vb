' GRAMMAR ANALYZER
' This program translates a grammar FROM antlr.g format or a subset of EBNF (ISO 14977) format
' INTO antlr.g format or that same subset of EBNF or an HTML page
'
' Dim grammar = Antlr.Parse(File.ReadAllText("vb11.g4"))
' File.WriteAllText("vb11.ebnf", ISO14977.ToString(grammar, "vb11"))
' File.WriteAllText("vb11.html", Html.ToString(grammar, "vb11"), Encoding.UTF8)

Imports System.Text

Class Production
    Public EBNF As EBNF, ProductionName As String ' optional. ProductionName contains no whitespace and is not delimited by '
    Public Comment As String ' optional. Does not contain *) or newline
    Public RuleStartsOnNewLine As Boolean ' e.g. "Rule: \n | Choice1"
    Public Link, LinkName As String ' optional. Link to the spec
End Class

Class EBNF
    Public Kind As EBNFKind
    Public s As String
    Public Children As List(Of EBNF)
    Public FollowingComment As String = "" ' Does not contain *) or newline
    Public FollowingNewline As Boolean
End Class

Enum EBNFKind
    ZeroOrMoreOf ' has exactly one child   e*  {e}
    OneOrMoreOf  ' has exactly one child   e+  [e]
    ZeroOrOneOf  ' has exactly one child   e?  {e}-
    Sequence     ' has 2+ children
    Choice       ' has 2+ children
    Terminal     ' has 0 children and an unescaped string without linebreaks which is not "<>", which either does not contain ' or does not contain "
    ExtendedTerminal ' has 0 children and a string without linebreaks, which does not itself contain '?'
    Reference    ' has 0 children and a string
End Enum


Class Grammar
    Public Productions As New List(Of Production)
    Public Name As String

    Public Function AreProductionsSameAs(copy As Grammar) As Boolean

        Dim ToDictionary = Function(g As Grammar)
                               Dim d As New Dictionary(Of String, Production)
                               For Each p In g.Productions
                                   If p.ProductionName IsNot Nothing Then d.Add(p.ProductionName, p)
                               Next
                               Return d
                           End Function
        Dim dme = ToDictionary(Me), dcopy = ToDictionary(copy)
        Dim ok = True

        For Each p In dme.Keys
            If Not dcopy.ContainsKey(p) Then Continue For
            Dim pme = Antlr.ToString(dme(p)), pcopy = Antlr.ToString(dcopy(p))
            If pme = pcopy Then Continue For
            ok = False
            Console.WriteLine($"MISMATCH for '{p}'")
            Console.WriteLine($"AUTHORITY:{vbCrLf}{pme}")
            Console.WriteLine($"COPY:{vbCrLf}{pcopy}")
            Console.WriteLine()
        Next

        For Each p In dme.Keys
            If p = "start" Then Continue For
            If Not dcopy.ContainsKey(p) Then Console.WriteLine($"Copy doesn't contain '{p}'") : ok = False
        Next
        For Each p In dcopy.Keys
            If p = "start" Then Continue For
            If Not dme.ContainsKey(p) Then Console.WriteLine($"Authority doesn't contain '{p}'") : ok = False
        Next

        Return ok
    End Function

End Class


Class ISO14977
    Public Shared Shadows Function ToString(grammar As Grammar) As String
        Dim productions = grammar.Productions, grammarName = grammar.Name
        Dim r = ""
        r &= "(* Grammar " & grammarName & " *)" & vbCrLf
        For Each p In productions
            If p.EBNF Is Nothing AndAlso String.IsNullOrEmpty(p.Comment) Then
                r &= vbCrLf
            ElseIf p.EBNF Is Nothing Then
                r &= "(*" & p.Comment & "*) " & vbCrLf
            Else
                r &= p.ProductionName & " ="
                If p.RuleStartsOnNewLine Then r &= vbCrLf
                r &= vbTab & ToString(p.EBNF) & ";" & If(String.IsNullOrEmpty(p.Comment), "", "  (*" & p.Comment & "*)") & vbCrLf
            End If
        Next
        Return r
    End Function

    Public Shared Shadows Function ToString(ebnf As EBNF) As String
        Dim r = ""
        Select Case ebnf.Kind
            Case EBNFKind.Terminal : If ebnf.s.Contains("'") Then r = """" & ebnf.s & """" Else r = "'" & ebnf.s & "'"
            Case EBNFKind.ExtendedTerminal : r = "?" & ebnf.s & "?"
            Case EBNFKind.Reference
                r = ebnf.s
            Case EBNFKind.ZeroOrMoreOf : r = "{" & ToString(ebnf.Children(0)) & "}"
            Case EBNFKind.ZeroOrOneOf : r = "[" & ToString(ebnf.Children(0)) & "]"
            Case EBNFKind.OneOrMoreOf : r = "{" & ToString(ebnf.Children(0)) & "}-"
            Case EBNFKind.Choice : r = String.Join(" | ", From c In ebnf.Children Select ToString(c))
            Case EBNFKind.Sequence
                Dim firstElement = True
                For Each c In ebnf.Children
                    If firstElement Then firstElement = False Else r &= ", "
                    Dim c2 = New EBNF With {.Kind = c.Kind, .s = c.s, .Children = c.Children}
                    If c2.Kind = EBNFKind.Choice Then r &= "(" & ToString(c2) & ")" Else r &= ToString(c2)
                    If Not String.IsNullOrEmpty(c.FollowingComment) Then r &= " (*" & c.FollowingComment & "*)"
                    If c.FollowingNewline Then r &= vbCrLf & vbTab
                Next
            Case Else : r = "???"
        End Select
        If Not String.IsNullOrEmpty(ebnf.FollowingComment) Then r &= " (*" & ebnf.FollowingComment & "*)"
        If ebnf.FollowingNewline Then r &= vbCrLf & vbTab
        Return r
    End Function

    Public Shared Function Parse(src As String) As LinkedList(Of Production)
        Dim tokens = Tokenize(src)
        Dim productions As New LinkedList(Of Production)
        While tokens.Count > 0
            Dim t = tokens.First.Value : tokens.RemoveFirst()
            If t.StartsWith("(* grammar") Then
                If tokens.First.Value = vbCrLf Then tokens.RemoveFirst()
            ElseIf t.StartsWith("(*") Then
                productions.AddLast(New Production With {.Comment = t.Substring(2, t.Length - 4)})
                If tokens.Count > 0 AndAlso tokens.First.Value = vbCrLf Then tokens.RemoveFirst()
            ElseIf t = vbCrLf Then
                productions.AddLast(New Production)
            ElseIf tokens.Count > 0 AndAlso tokens.First.Value = "=" Then
                tokens.RemoveFirst()
                Dim comment = "", newline = False
                GobbleUpComments(tokens, comment, newline)
                Dim p = ParseProduction(tokens, comment)
                GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
                If tokens.Count > 0 AndAlso tokens.First.Value = ";" Then tokens.RemoveFirst()
                If tokens.Count > 0 AndAlso tokens.First.Value = vbCrLf Then tokens.RemoveFirst()
                productions.AddLast(New Production With {.Comment = comment, .EBNF = p, .ProductionName = t, .RuleStartsOnNewLine = newline})
                While tokens.Count > 0 AndAlso tokens.First.Value.StartsWith("(*")
                    productions.Last.Value.Comment &= tokens.First.Value.Substring(2, tokens.First.Value.Length - 4) : tokens.RemoveFirst()
                    If tokens.First.Value = vbCrLf Then tokens.RemoveFirst()
                End While
            Else
                Throw New Exception("Unrecognized " & t)
            End If
        End While

        Return productions
    End Function


    Private Shared Function Tokenize(s As String) As LinkedList(Of String)
        s = s.Trim()
        Dim tokens As New LinkedList(Of String), pos = 0

        While (pos < s.Length)
            If s(pos) = "="c Then
                tokens.AddLast("=") : pos += 1
            ElseIf s(pos) = "{"c Then
                tokens.AddLast("{") : pos += 1
            ElseIf pos + 1 < s.Length AndAlso s.Substring(pos, 2) = "}-" Then
                tokens.AddLast("}-") : pos += 2
            ElseIf s(pos) = "}"c Then
                tokens.AddLast("}") : pos += 1
            ElseIf s(pos) = "["c Then
                tokens.AddLast("[") : pos += 1
            ElseIf s(pos) = "]"c Then
                tokens.AddLast("]") : pos += 1
            ElseIf pos + 1 < s.Length AndAlso s.Substring(pos, 2) = "(*" Then
                pos += 2
                Dim t = ""
                While pos < s.Length AndAlso (pos >= s.Length - 1 OrElse s.Substring(pos, 2) <> "*)")
                    t += s(pos) : pos += 1
                End While
                If t.Contains(vbCrLf) OrElse t.Contains(vbCr) OrElse t.Contains(vbLf) Then Throw New Exception("Comments must be single-line")
                If pos < s.Length - 1 AndAlso s.Substring(pos, 2) = "*)" Then pos += 2
                tokens.AddLast("(*" & t & "*)")
            ElseIf s(pos) = "("c Then
                tokens.AddLast("(") : pos += 1
            ElseIf s(pos) = ")"c Then
                tokens.AddLast(")") : pos += 1
            ElseIf s(pos) = "|"c Then
                tokens.AddLast("|") : pos += 1
            ElseIf s(pos) = ","c Then
                tokens.AddLast(",") : pos += 1
            ElseIf s(pos) = ";"c Then
                tokens.AddLast(";") : pos += 1
            ElseIf pos + 1 < s.Length AndAlso s.Substring(pos, 2) = vbCrLf Then
                tokens.AddLast(vbCrLf) : pos += 2
            ElseIf s.Substring(pos, 1) = vbCr Then
                tokens.AddLast(vbCrLf) : pos += 1
            ElseIf s.Substring(pos, 1) = vbLf Then
                tokens.AddLast(vbCrLf) : pos += 1
            ElseIf s.Substring(pos, 1) = "?" Then
                pos += 1
                Dim t = ""
                While pos < s.Length AndAlso s(pos) <> "?"c
                    t += s(pos) : pos += 1
                End While
                If t.Contains(vbCrLf) OrElse t.Contains(vbCr) OrElse t.Contains(vbLf) Then Throw New Exception("Special-terminals must be single-line")
                tokens.AddLast("?" & t & "?")
                If pos < s.Length AndAlso s(pos) = "?" Then pos += 1
            ElseIf s(pos) = "'" Then
                Dim t = "" : pos += 1
                While pos < s.Length AndAlso s.Substring(pos, 1) <> "'"c
                    t &= s(pos) : pos += 1
                End While
                If t.Contains(vbCrLf) OrElse t.Contains(vbCr) OrElse t.Contains(vbLf) Then Throw New Exception("Terminals must be single-line")
                If t.Contains("'") Then Throw New Exception("Single-quoted terminals may not include single-quote")
                tokens.AddLast("'" & t & "'")
                If pos < s.Length AndAlso s(pos) = "'" Then pos += 1
            ElseIf s(pos) = """" Then
                Dim t = "" : pos += 1
                While pos < s.Length AndAlso s.Substring(pos, 1) <> """"c
                    t &= s(pos) : pos += 1
                End While
                If t.Contains(vbCrLf) OrElse t.Contains(vbCr) OrElse t.Contains(vbLf) Then Throw New Exception("Terminals must be single-line")
                If t.Contains("""") Then Throw New Exception("Double-quoted terminals may not include double-quote")
                tokens.AddLast("'" & t & "'")
                If pos < s.Length AndAlso s(pos) = """" Then pos += 1
            Else
                Dim t = ""
                While pos < s.Length AndAlso Not String.IsNullOrWhiteSpace(s(pos)) AndAlso
                    s(pos) <> "="c AndAlso s(pos) <> "{"c AndAlso s(pos) <> "}"c AndAlso s(pos) <> "-" AndAlso
                    s(pos) <> "["c AndAlso s(pos) <> "]"c AndAlso s(pos) <> "("c AndAlso s(pos) <> ")"c AndAlso
                    s(pos) <> "|"c AndAlso s(pos) <> ","c AndAlso s(pos) <> ";"c AndAlso
                    s(pos) <> "'"c AndAlso s(pos) <> """" AndAlso s(pos) <> "?"c AndAlso
                    s(pos) <> vbCr(0) AndAlso s(pos) <> vbLf(0) AndAlso
                    (pos + 1 >= s.Length OrElse s.Substring(pos, 2) <> "(*")
                    t &= s(pos) : pos += 1
                End While
                t = t.Trim()
                If t.Length > 0 Then tokens.AddLast(t)
            End If
            ' Bump up to the next non-whitespace character:
            While pos < s.Length AndAlso s(pos) <> vbCr(0) AndAlso s(pos) <> vbLf(0) AndAlso String.IsNullOrWhiteSpace(s(pos))
                pos += 1
            End While
        End While

        Return tokens
    End Function


    Private Shared Sub GobbleUpComments(tokens As LinkedList(Of String), ByRef ExtraComments As String, ByRef HasNewline As Boolean)
        If tokens.Count = 0 Then Return
        While True
            If tokens.First.Value.StartsWith("(*") Then
                ExtraComments &= tokens.First.Value.Substring(2, tokens.First.Value.Length - 4) : tokens.RemoveFirst()
            ElseIf tokens.First.Value = vbCrLf Then
                HasNewline = True : tokens.RemoveFirst() : If ExtraComments.Length > 0 Then ExtraComments &= " "
            Else
                Exit While
            End If
        End While
        ExtraComments = ExtraComments.TrimEnd()
    End Sub

    Private Shared Function ParseProduction(tokens As LinkedList(Of String), ByRef ExtraComments As String) As EBNF
        If tokens.Count = 0 Then Throw New Exception("empty input stream")
        GobbleUpComments(tokens, ExtraComments, False)
        Return ParsePar(tokens, ExtraComments)
    End Function

    Private Shared Function ParsePar(tokens As LinkedList(Of String), ByRef ExtraComments As String) As EBNF
        Dim pp As New LinkedList(Of EBNF)
        pp.AddLast(ParseSeq(tokens, ExtraComments))
        While tokens.Count > 0 AndAlso tokens.First.Value = "|"
            tokens.RemoveFirst()
            GobbleUpComments(tokens, ExtraComments, pp.Last.Value.FollowingNewline)
            pp.AddLast(ParseSeq(tokens, ExtraComments))
        End While
        If pp.Count = 1 Then Return pp(0)
        Return New EBNF With {.Kind = EBNFKind.Choice, .Children = pp.ToList}
    End Function

    Private Shared Function ParseSeq(tokens As LinkedList(Of String), ByRef ExtraComments As String) As EBNF
        Dim pp As New LinkedList(Of EBNF)
        pp.AddLast(ParseAtom(tokens, ExtraComments))
        While tokens.Count > 0 AndAlso tokens.First.Value = ","
            tokens.RemoveFirst()
            GobbleUpComments(tokens, ExtraComments, pp.Last.Value.FollowingNewline)
            pp.AddLast(ParseAtom(tokens, ExtraComments))
        End While
        If pp.Count = 1 Then Return pp(0)
        Return New EBNF With {.Kind = EBNFKind.Sequence, .Children = pp.ToList}
    End Function

    Private Shared Function ParseAtom(tokens As LinkedList(Of String), ByRef ExtraComments As String) As EBNF
        If tokens.First.Value = "(" Then
            tokens.RemoveFirst()
            Dim p = ParseProduction(tokens, ExtraComments)
            If tokens.Count = 0 OrElse tokens.First.Value <> ")" Then Throw New Exception("mismatched parentheses")
            tokens.RemoveFirst()
            GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
            Return p
        ElseIf tokens.First.Value = "{" Then
            tokens.RemoveFirst()
            Dim p = ParseProduction(tokens, ExtraComments)
            If tokens.Count = 0 OrElse (tokens.First.Value <> "}" AndAlso tokens.First.Value <> "}-") Then Throw New Exception("mismatched braces")
            Dim kind = If(tokens.First.Value = "}", EBNFKind.ZeroOrMoreOf, EBNFKind.OneOrMoreOf)
            tokens.RemoveFirst()
            p = New EBNF With {.Kind = kind, .Children = {p}.ToList}
            GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
            Return p
        ElseIf tokens.First.Value = "[" Then
            tokens.RemoveFirst()
            Dim p = ParseProduction(tokens, ExtraComments)
            If tokens.Count = 0 OrElse tokens.First.Value <> "]" Then Throw New Exception("mismatched brackets")
            tokens.RemoveFirst()
            p = New EBNF With {.Kind = EBNFKind.ZeroOrOneOf, .Children = {p}.ToList}
            GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
            Return p
        ElseIf tokens.First.Value.StartsWith("'") Then
            Dim t = tokens.First.Value : tokens.RemoveFirst()
            t = t.Substring(1, t.Length - 2)
            Dim p = New EBNF With {.Kind = EBNFKind.Terminal, .s = t}
            GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
            Return p
        ElseIf tokens.First.Value.StartsWith("?") Then
            Dim t = tokens.First.Value : tokens.RemoveFirst()
            t = t.Substring(1, t.Length - 2)
            Dim p = New EBNF With {.Kind = EBNFKind.ExtendedTerminal, .s = t}
            GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
            Return p
        Else
            Dim t = tokens.First.Value : tokens.RemoveFirst()
            Dim p = New EBNF With {.Kind = EBNFKind.Reference, .s = t}
            GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
            Return p
        End If
    End Function


End Class


Class Html
    ' Problem: we want a huge number of hyperlinks on the page, but this causes browsers to load
    ' the page sluggishly.
    ' Solution: each href just looks like "<a>fred</a>", and dynamic javascript synthesizes the
    ' href attribute as you mouse over. Note that HTML anchor targets used to be written <h2><a name="xyz">
    ' but in HTML5 we now prefer to use <h2 id="xyz">. Note that the attribute in HTML5, unlike HTML4,
    ' is allowed to include ANY character other than quote, which is escaped as &quot;. This escaping
    ' is done automatically by VB when you write <h2 id=<%= s %>>. As for the href argument, I don't
    ' know what escaping if any is needed, but I found that doing encodeUriComponent on it works fine.
    ' So you can even have <a href="%22">click</a> <h2 id="&quot;">title</a> and the link works fine.

    ' Problem: we need some data-structures e.g. MayBeFollowedBySet which are defined on *both* productions and terminals and extended-terminals
    ' And we also need to print stuff out based on just the pure text name of a production/terminal.
    ' But we'd rather keep things simple and avoid lots of classes whose sole job is to distinguish between the various kinds.
    ' Solution:
    ' "Productions" stores plain-strings of production names.
    ' "Terminals" stores plain-strings of terminals, and stores extended terminals normalized as <extendedterminal>
    ' All other data-structures apart from CaseEscapedNames are keyed off strings like "production" or "'terminal'" or "'<extendedterminal>'"
    Dim GrammarName As String
    Dim Productions As New Dictionary(Of String, IEnumerable(Of EBNF)) ' String ::= EBNF0 | EBNF1 | ...
    Dim ProductionReferences As New Dictionary(Of String, Tuple(Of String, String))
    Dim Terminals As New HashSet(Of String) ' The list of [extended]terminals.
    Dim CaseEscapedNames As New Dictionary(Of String, String) ' For a given Production/Terminal string, gives a version that's case-escaped
    Dim MayBeEmptySet As New Dictionary(Of String, Boolean)  ' Says whether a given production/[extended]terminal may be empty
    Dim MayStartWithSet As New Dictionary(Of String, HashSet(Of String))  ' The list of terminals which may start a given production/[extended]terminal
    Dim MayBeFollowedBySet As New Dictionary(Of String, HashSet(Of String)) ' The list of terminals which may follow a given production/[extended]terminal
    Dim UsedBySet As New Dictionary(Of String, HashSet(Of String)) ' Given an production/[extended]terminal, says which productions use it directly

    ' two functions to normalize how productions/terminals/extended_terminals appear in the above data-structures
    ' extended terminals have already been normalized to terminals surrounded by <>
    Shared Function normt(e As EBNF) As String
        If e.Kind = EBNFKind.Terminal Then Return e.s
        If e.Kind = EBNFKind.ExtendedTerminal Then Return "<" & e.s & ">"
        Throw New Exception("unexpected terminal")
    End Function
    Shared Function normt(t As String) As String
        Return $"'{t}'"
    End Function

    Public Shared Shadows Function ToString(grammar As Grammar) As String
        Dim html As New Html(grammar)
        html.Analyze()
        Return html.ToString()
    End Function

    Public Shared Sub WriteFile(grammar As Grammar, fn As String)
        IO.File.WriteAllText(fn, ToString(grammar), Encoding.UTF8)
    End Sub

    Sub New(grammar As Grammar)
        GrammarName = grammar.Name
        For Each p In grammar.Productions
            If p.EBNF Is Nothing Then Continue For
            Dim choices = If(p.EBNF.Kind = EBNFKind.Choice, CType(p.EBNF.Children, IEnumerable(Of EBNF)), {p.EBNF})
            If flatten(choices).Where(Function(e) e.Kind = EBNFKind.Choice).Count > 0 Then Throw New Exception("nested choice not implemented")
            Me.Productions.Add(p.ProductionName, choices)
            If p.Link IsNot Nothing Then Me.ProductionReferences.Add(p.ProductionName, Tuple.Create(p.Link, p.LinkName))
        Next
        Dim InvalidReferences As New HashSet(Of String)
        For Each p In Me.Productions
            Dim rr = From e In flatten(p.Value)
                     Where e.Kind = EBNFKind.Reference AndAlso Not Me.Productions.ContainsKey(e.s)
                     Select e.s
            InvalidReferences.UnionWith(rr)
        Next
        For Each r In InvalidReferences
            Me.Productions.Add(r, {New EBNF With {.Kind = EBNFKind.ExtendedTerminal, .s = "UNDEFINED"}})
        Next

    End Sub

    Public Shadows Function ToString() As String
        ' We'll pick colors based on the grammarName...
        Dim perms = {({2, 0, 1}), ({0, 2, 1}), ({1, 0, 2}), ({1, 2, 0}), ({0, 1, 2}), ({2, 1, 0})}
        Dim perm = perms(Asc(GrammarName(0)) Mod 6)
        Dim permf = Function(c As Integer()) String.Join(",", New Integer() {c(perm(0)), c(perm(1)), c(perm(2))})
        Dim rgb_background = permf({210, 220, 240})
        Dim rgb_popup = permf({225, 215, 255})
        Dim rgb_divider = permf({0, 0, 220})

        Dim r = <html>
                    <!-- saved from url=(0014)about:internet -->
                    <head>
                        <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
                        <title>Grammar <%= GrammarName %></title>
                        <style type="text/css">
body {font-family:calibri; color:black; background-color:rgb(<%= rgb_background %>);}
#popup {background-color: rgb(<%= rgb_popup %>); padding: 2ex; border: solid thick black; font-size: 80%;}
a {background-color: yellow; font-family:"courier new"; font-size: 80%;}
a.n {background-color:transparent; font-family:calibri; font-size: 100%;}
a.s {background-color:transparent; font-family:calibri; font-size: 100%; text-decoration: underline;}
ul {margin-top:0; margin-bottom:0;}
h1 {margin-bottom:2ex;}
h2 {margin-bottom:0; padding:0.5ex;
border-top: dotted 1px rgb(<%= rgb_divider %>);}
#popup h2 {background-color:transparent; padding:0; border-top: 0px;}
.u {font-style: italic; margin-top:1ex; width:80%;}
#popup .u {margin-top: 3ex;}
.u a {padding-left:1ex;}
.t {margin-top:1ex; line-height:130%;}
.t {display:none;}
li {list-style-type: circle;}
li.r {padding-left:2em; list-style-type:square;}
li a.n {font-weight:bold;}
li a.s {font-weight:bold;}
li.u a.n {font-weight:normal;}
li.u a.s {font-weight:normal;}
a {color: black; text-decoration:none;}
a:hover {text-decoration: underline;}
                        </style>
                        <script type="text/javascript"><!--
var timer = undefined;
var timerfor = undefined;

document.onkeydown = function(e)
{
  if (!e) e=window.event;
  if (e.keyCode==27) p();
}

document.onmouseover = function(e)
{
// Typically we'll be viewing this document at something like
// http://ljw1004.github.io/vbspec/dir1/dir2/vb.html
// but hrefs to "statements.md#section" must point to
// https://github.com/ljw1004/vbspec/blob/gh-pages/dir1/dir2/statements.md#section
// The raw HTML won't know where it's going to be sitting.
// So instead we'll do the appropriate patching-up when you mouse over a link.
if (!e) e=window.event;
var a = e.toElement || e.relatedTarget;
if (!a || a.tagName.toLowerCase()!="a") return;
if (a.className!="s") return;
if (!a.href || typeof(a.href)=='undefined') return;
var href = a.getAttribute("href");
if (href.indexOf("http")==0) return; // has already been processed
if (window.location.protocol.indexOf("http")!=0) return;
if (window.location.host.indexOf(".github.io") == -1) return;

var user = window.location.host.replace(".github.io",""); // "ljw1004"
var dirs = window.location.pathname.split("/"); dirs.splice(0,1);
var repository = dirs[0];                                 // "vbspec"
dirs.splice(0,1); dirs.splice(dirs.length-1,1);
var dir = dirs.join("/");                                 // "dir1/dir2"
var target = href;                                        // "statements.md#section"
var url = "https://github.com/"+user+"/"+repository+"/blob/gh-pages/"+dir+"/"+target;
a.setAttribute("href",url); a.href = url; // I'm not sure which of these to change, so I'm doing both
}

document.onmouseout = function(e)
{
  // I honestly can't remember why this code is in "onMouseOut" rather than "onMouseOver".
  // But it seems to work okay! ... 

  if (!e) e=window.event;
  if (timerfor && (e.fromElement || e.target)==timerfor) {clearTimeout(timer); timer=undefined; timerfor=undefined;}
  var a = e.toElement || e.relatedTarget;
  if (!a || a.tagName.toLowerCase()!="a") return;

  // synthesize the href target if it wasn't there in the (minimized) html already:
  if (!a.href || typeof(a.href)=='undefined')
  {
    var acontent = a.firstChild.data;
    if (!a.className || a.className!="n") acontent="'"+acontent+"'";
    var t = acontent.replace(/([A-Z])/g,"-$1");
    t = t.replace(/\&lt;/g,'<').replace(/\&gt;/g,'>').replace(/\&amp;/g,'&');
    t = encodeURIComponent(t);
    if (document.getElementsByName(t).length == 0)
    {
      t = acontent;
      t = t.replace(/\&lt;/g,'<').replace(/\&gt;/g,'>').replace(/\&amp;/g,'&');
      t = encodeURIComponent(t);
    }
    a.href = "#" + t;
  }

  // Only show popup tooltips for grammar links within this page
  if (a.href.charAt(0) != '#') return;

  // Only show popup tooltips for "top-level" links (i.e. not for links within the popup tooltip itself)
  var r = a;
  while (r.parentNode!=null && r.id!="popup") r=r.parentNode;
  if (r.id=="popup") return;

  if (!isvisible(document.getElementById("popup"))) {p(a); return;}

  // If you want to move the cursor from its current location to hover over something
  // inside the popup tooltip, well, moving it shouldn't cause the popup tooltip to go away!
  // The following logic gives it some little persistence.
  if (timer) clearTimeout(timer);
  timerfor=a; timer = setTimeout(function() {p(a);},100);  
}


function isvisible(pup)
{
  if (pup.style.visibility=="hidden") return false;
  if (pup.style.display=="none") return false;
  var pl=pup.offsetLeft, pt=pup.offsetTop, pr=pl+pup.offsetWidth, pb=pt+pup.offsetHeight;
  var sl=window.pageXOffset || document.body.scrollLeft || document.documentElement.scrollLeft;
  var st=window.pageYOffset || document.body.scrollTop || document.documentElement.scrollTop;
  var sr=sl+document.body.clientWidth, sb=st+document.body.clientHeight;
  if ((pl<sl && pr<sl) || (pl>sr && pr>sr)) return false;
  if ((pt<st && pb<st) || (pt>sb && pb>sb)) return false;
  return true;
}


function p(a)
{
  if (timer) clearTimeout(timer); timer=undefined; timerfor=undefined;
  var pup = document.getElementById("popup");
  while (pup.hasChildNodes()) pup.removeChild(pup.firstChild);
  if (typeof(a)=='undefined' || !a) {pup.style.visibility="hidden"; return;}
  var div = a; while (div.parentNode!=null && (typeof(div.tagName)=='undefined' || div.tagName.toLowerCase()!='div')) div=div.parentNode;
  var ref = a.href.split("#")[1];
  var src=null;
  var bb = document.getElementsByTagName("h2")
  for (var i=0; i<bb.length && src==null; i++)
  {
    var cc = bb[i].getElementsByTagName("a");
    for (var j=0; j<cc.length && src==null; j++)
    {
      if (cc[j].id==ref || encodeURIComponent(cc[j].id)==ref) src=bb[i].parentNode.childNodes;
    }
  }
  for (var i=0; i<src.length; i++) pup.appendChild(src[i].cloneNode(true));
  var aa = pup.getElementsByTagName("a");
  for (var i=0; i<aa.length; i++) {if (aa[i].id) aa[i].id=""; if (aa[i].onmouseover) aa[i].onmouseover="";}
  var aa = pup.getElementsByTagName("li");
  for (var i=0; i<aa.length; i++) if (aa[i].style.display='none') aa[i].style.display='block';
  pup.style.visibility="visible";
  var top=div.offsetHeight; while (div) {top+=div.offsetTop; div=div.offsetParent;}
  pup.style.top = top + "px";
}
                        --></script>
                    </head>
                    <body onclick="p()">
                        <div id="popup" style="visibility:hidden; position:absolute; right:0; width: auto; top:0; height:auto;"></div>
                        <h1>Grammar <%= GrammarName %></h1>
                        <%= From production In Productions Select
                            <div class="p">
                                <h2><a id=<%= CaseEscapedNames(production.Key) %>><%= MakeNonterminal(production.Key) %></a> ::=</h2>
                                <ul>
                                    <%= From choice In production.Value Select <li class="r"><%= MakeEbnf(choice) %></li>
                                    %>
                                </ul>
                                <ul>
                                    <%= Iterator Function()
                                            If Not ProductionReferences.ContainsKey(production.Key) Then Return
                                            Dim tt = ProductionReferences(production.Key)
                                            Yield <li class="u">(Spec: <a class="s" href=<%= tt.Item1 %>><%= tt.Item2 %></a>)</li>
                                        End Function() %>
                                    <li class="u">(used in <%= From p In UsedBySet(production.Key) Select <xml>&#x20;<%= MakeNonterminal(p) %></xml>.Nodes %>)</li>
                                    <li class="t"><%= If(MayBeEmptySet(production.Key), "May be empty", "Never empty") %></li>
                                    <li class="t">MayStartWith: <%= From t In MayStartWithSet(production.Key) Select <xml>&#x20;<%= MakeTerminal(t) %></xml>.Nodes %></li>
                                    <li class="t">MayBeFollowedBy: <%= From t In MayBeFollowedBySet(production.Key) Select <xml>&#x20;<%= MakeTerminal(t) %></xml>.Nodes %></li>
                                </ul>
                            </div>
                        %>
                        <%= From terminal In Terminals Order By terminal Select
                            <div class="p">
                                <h2><a id=<%= CaseEscapedNames(normt(terminal)) %>><%= MakeTerminal(terminal) %></a></h2>
                                <ul>
                                    <li class="u">(used in <%= From p In UsedBySet(normt(terminal)) Select <xml>&#x20;<%= MakeNonterminal(p) %></xml>.Nodes %>)</li>
                                    <li class="t">MayBeFollowedBy: <%= From t In MayBeFollowedBySet(normt(terminal)) Select <xml>&#x20;<%= MakeTerminal(t) %></xml>.Nodes %></li>
                                </ul>
                            </div>
                        %>
                    </body>
                </html>

        Dim s = "<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">" &
                vbCrLf & r.ToString
        While s.Contains("  ") : s = s.Replace("  ", " ") : End While
        s = s.Replace(vbCrLf & " ", vbCrLf)
        Return s
    End Function

    Shared Function MakeTerminal(t As String) As IEnumerable(Of XNode)
        ' t has been part-normalized to either the form "terminal" or "<extendedterminal>", as in the Terminals set
        ' it has not been further-normalized with single-quotes.
        Return <xml><a><%= t %></a></xml>.Nodes
    End Function

    Shared Function MakeNonterminal(p As String) As IEnumerable(Of XNode)
        Return <xml><a class="n"><%= p %></a></xml>.Nodes
    End Function

    Shared Function MakeEbnf(e As EBNF) As IEnumerable(Of XNode)
        Select Case e.Kind
            Case EBNFKind.OneOrMoreOf, EBNFKind.ZeroOrMoreOf, EBNFKind.ZeroOrOneOf
                Dim op = If(e.Kind = EBNFKind.OneOrMoreOf, "+", If(e.Kind = EBNFKind.ZeroOrMoreOf, "*", "?"))
                If e.Children(0).Kind = EBNFKind.Sequence OrElse e.Children(0).Kind = EBNFKind.Choice Then
                    Return <xml>( <%= MakeEbnf(e.Children(0)) %> )<%= op %></xml>.Nodes
                Else
                    Return <xml><%= MakeEbnf(e.Children(0)) %><%= op %></xml>.Nodes
                End If
            Case EBNFKind.Sequence
                Return <xml><%= From c In e.Children Select <t><%= MakeEbnf(c) %>&#x20;</t>.Nodes %></xml>.Nodes
            Case EBNFKind.Reference
                Return MakeNonterminal(e.s)
            Case EBNFKind.Terminal, EBNFKind.ExtendedTerminal
                Return MakeTerminal(normt(e))
            Case Else
                Throw New Exception("unexpected EBNF kind")
        End Select
    End Function


    Shared Function flatten(ebnfs As IEnumerable(Of EBNF)) As IEnumerable(Of EBNF)
        Dim r As New LinkedList(Of EBNF)
        Dim queue As New Stack(Of IEnumerable(Of EBNF))
        queue.Push(ebnfs)
        While queue.Count > 0
            For Each e In queue.Pop()
                r.AddLast(e)
                If e.Children IsNot Nothing Then queue.Push(e.Children)
            Next
        End While
        Return r
    End Function

    Sub Analyze()
        Dim anyChanges = True

        ' Compute the "Terminals" set of all [extended]terminals mentioned in the grammar
        For Each p In Productions
            Dim terminals = From e In flatten(p.Value) Where e.Kind = EBNFKind.Terminal OrElse e.Kind = EBNFKind.ExtendedTerminal Select normt(e)
            Me.Terminals.UnionWith(terminals)
        Next

        ' Compute the "CaseEscapedNames": where capitals may be escaped by prefixing with a "-" to avoid case-sensitivity clashes
        Dim CaseCounts = (From s In Productions.Select(Function(p) p.Key).Concat(Terminals.Select(Function(t) normt(t)))
                          Group By ci = s.ToLowerInvariant Into Count()
                          Select ci, Count).ToDictionary(Function(fi) fi.ci, Function(fi) fi.Count)
        For Each p In Productions
            CaseEscapedNames(p.Key) = p.Key
            If CaseCounts(p.Key.ToLowerInvariant) > 1 Then
                CaseEscapedNames(p.Key) = Text.RegularExpressions.Regex.Replace(p.Key, "([A-Z])", "-$1")
            End If
        Next
        For Each t In Terminals
            Dim nt = normt(t)
            CaseEscapedNames(nt) = nt
            If CaseCounts(nt.ToLowerInvariant) > 1 Then
                CaseEscapedNames(nt) = Text.RegularExpressions.Regex.Replace(nt, "([A-Z])", "-$1")
            End If
        Next

        ' Compute the "MayBeEmpty" flag: for each production, says whether it may be empty or not
        ' This is an iterative algorithm. It starts with everything "false" i.e. it can't be empty.
        ' On each iteration we monotonically increase one or more things to "true" i.e. they can be empty.
        For Each p In Productions
            MayBeEmptySet.Add(p.Key, False)
        Next
        For Each t In Terminals
            MayBeEmptySet.Add(normt(t), False)
        Next
        anyChanges = True
        While anyChanges
            anyChanges = False
            For Each p In Productions
                If MayBeEmptySet(p.Key) Then Continue For ' if true, then there's nowhere else to monotonically go
                For Each b In p.Value
                    Dim mbe = MayBeEmpty(b)
                    If mbe Then MayBeEmptySet(p.Key) = True : anyChanges = True
                Next
            Next
        End While

        ' Compute the "MayStartWith" set: for each production, gather the tokens that may start this production
        ' This starts with each production's MayStartWith set being empty. On each iteration, the set monotonically increases.
        For Each p In Productions
            MayStartWithSet.Add(p.Key, New HashSet(Of String)())
        Next
        For Each t In Terminals
            MayStartWithSet.Add(normt(t), New HashSet(Of String)({t}))
        Next
        anyChanges = True
        While anyChanges
            anyChanges = False
            For Each p In Productions
                Dim p0 = p
                For Each e In p.Value
                    Dim updatedStarts = MayStartWith(e)
                    Dim newStarts = From s In updatedStarts Where Not MayStartWithSet(p0.Key).Contains(s) Select s
                    If newStarts.Count = 0 Then Continue For
                    MayStartWithSet(p.Key).UnionWith(updatedStarts)
                    anyChanges = True
                Next
            Next
        End While

        ' Compute the "MayBeFollowedBy" set: for each production, gather the tokens that may come after this production
        ' This starts with each production's MayBeFollowedBy set being empty. On each iteration, the set monotonically increases.
        For Each p In Productions
            MayBeFollowedBySet.Add(p.Key, New HashSet(Of String)())
        Next
        For Each t In Terminals
            MayBeFollowedBySet.Add(normt(t), New HashSet(Of String)())
        Next
        anyChanges = True
        While anyChanges
            anyChanges = False
            For Each p In Productions
                For Each e In p.Value
                    UpdateMayBeFollowedBySet(e, MayBeFollowedBySet(p.Key), anyChanges)
                Next
            Next
        End While

        ' Compute the "UsedBy" set: for each production, gather up the productions that use it
        For Each p In Productions
            UsedBySet.Add(p.Key, New HashSet(Of String)())
        Next
        For Each t In Terminals
            UsedBySet.Add(normt(t), New HashSet(Of String)())
        Next
        For Each p In Productions
            Dim productionReferences = flatten(p.Value).Where(Function(e) e.Kind = EBNFKind.Reference)
            Dim terminalReferences = flatten(p.Value).Where(Function(e) e.Kind = EBNFKind.Terminal OrElse e.Kind = EBNFKind.ExtendedTerminal)
            For Each r In productionReferences : UsedBySet(r.s).Add(p.Key) : Next
            For Each r In terminalReferences : UsedBySet(normt(normt(r))).Add(p.Key) : Next
        Next

    End Sub

    Function MayBeEmpty(e As EBNF) As Boolean
        Select Case e.Kind
            Case EBNFKind.OneOrMoreOf : Return MayBeEmpty(e.Children(0)) ' E+ may be empty only if E may be empty
            Case EBNFKind.ZeroOrMoreOf : Return True ' E* may always be empty
            Case EBNFKind.ZeroOrOneOf : Return True ' E? may always be empty
            Case EBNFKind.Terminal, EBNFKind.ExtendedTerminal : Return False
            Case EBNFKind.Reference : Return MayBeEmptySet(e.s) ' S may be empty only if it's a production which may be empty
            Case EBNFKind.Sequence
                For Each c In e.Children
                    If Not MayBeEmpty(c) Then Return False
                Next
                Return True ' E0 E1 E2 ... may be empty only if all the elemnets in the sequence may be empty
            Case Else
                Throw New Exception("unexpected EBNFKind")
                Return Nothing
        End Select
    End Function

    Function MayStartWith(e As EBNF) As HashSet(Of String)
        Select Case e.Kind
            Case EBNFKind.OneOrMoreOf : Return MayStartWith(e.Children(0))
            Case EBNFKind.ZeroOrMoreOf : Return MayStartWith(e.Children(0))
            Case EBNFKind.ZeroOrOneOf : Return MayStartWith(e.Children(0))
            Case EBNFKind.Terminal, EBNFKind.ExtendedTerminal : Return New HashSet(Of String)({normt(e)})
            Case EBNFKind.Reference : Return MayStartWithSet(e.s)
            Case EBNFKind.Sequence
                Dim hs As New HashSet(Of String)
                For Each c In e.Children
                    hs.UnionWith(MayStartWith(c))
                    If Not MayBeEmpty(c) Then Return hs
                Next
                Return hs
            Case Else
                Throw New Exception("unexpected EBNFKind")
                Return Nothing
        End Select
    End Function

    Sub UpdateMayBeFollowedBySet(e As EBNF, endings0 As HashSet(Of String), ByRef anyChanges As Boolean)
        ' The meaning of this function is subtle.
        ' The outside world is telling us that "endings0 may come after "e", and we have to update the data-structures about elements within e.
        ' For instance, if we were told "{X,Y,Z} may come after prod1" then we have to update MayBeFollowedBySet(prod1) to include {X,Y,Z}.
        ' For instance, if we were told "{X,Y,Z} may come after prod2*" then we have to update MayBeFollowedBySet(prod2) to include {X,Y,Z}.UnionWith(MayStartWith(prod2))"
        ' For instance, if we were told "{X,Y,Z} may come after (prod1,prod2)" then we have to update MayBeFollowedBySet(prod2) to include {X,Y,Z}
        '   and also either MayBeFollowedBySet(prod1) has to include MayStartWith(prod2), or it has to include {X,Y,Z}.UnionWith(MayStartWith(prod2))
        Dim endings As New HashSet(Of String)(endings0) ' make a local copy of it that we can alter
        Select Case e.Kind
            Case EBNFKind.OneOrMoreOf, EBNFKind.ZeroOrMoreOf
                Dim additionalEndings = MayStartWith(e.Children(0))
                endings.UnionWith(additionalEndings)
                UpdateMayBeFollowedBySet(e.Children(0), endings, anyChanges)
            Case EBNFKind.ZeroOrOneOf
                UpdateMayBeFollowedBySet(e.Children(0), endings, anyChanges)
            Case EBNFKind.Terminal, EBNFKind.ExtendedTerminal
                If Not MayBeFollowedBySet(normt(normt(e))).IsSupersetOf(endings) Then MayBeFollowedBySet(normt(normt(e))).UnionWith(endings) : anyChanges = True
            ' do nothing
            Case EBNFKind.Reference
                If Not MayBeFollowedBySet(e.s).IsSupersetOf(endings) Then MayBeFollowedBySet(e.s).UnionWith(endings) : anyChanges = True
            Case EBNFKind.Sequence
                For i = e.Children.Count - 1 To 0 Step -1
                    UpdateMayBeFollowedBySet(e.Children(i), endings, anyChanges)
                    Dim additionalEndings = MayStartWith(e.Children(i))
                    If MayBeEmpty(e.Children(i)) Then endings.UnionWith(additionalEndings) Else endings = New HashSet(Of String)(additionalEndings)
                Next
            Case Else
                Debug.Fail("unexpected EBNFKind")
        End Select
    End Sub
End Class





Class Antlr

    Public Shared Shadows Function ToString(grammar As Grammar, grammarName As String) As String
        Dim productions = grammar.Productions
        Dim r = ""
        r &= "grammar " & grammarName & ";" & vbCrLf
        For Each p In productions
            r &= ToString(p)
        Next
        Return r
    End Function

    Public Shared Shadows Function ToString(p As Production) As String
        If p.EBNF Is Nothing AndAlso String.IsNullOrEmpty(p.Comment) Then
            Return vbCrLf
        ElseIf p.EBNF Is Nothing Then
            Return "//" & p.Comment & vbCrLf
        Else
            Dim r = ""
            r &= p.ProductionName & ":"
            If p.RuleStartsOnNewLine Then r &= vbCrLf
            r &= vbTab
            If p.RuleStartsOnNewLine Then r &= "| "
            r &= ToString(p.EBNF) & ";" & If(String.IsNullOrEmpty(p.Comment), "", "  //" & p.Comment) & vbCrLf
            Return r
        End If
    End Function

    Public Shared Shadows Function ToString(ebnf As EBNF) As String
        Dim r = ""
        Select Case ebnf.Kind
            Case EBNFKind.Terminal
                r = "'" & ebnf.s.Replace("\", "\\").Replace("'", "\'") & "'"
            Case EBNFKind.ExtendedTerminal : r = "'<" & ebnf.s.Replace("\", "\\").Replace("'", "\'") & ">'"
            Case EBNFKind.Reference : r = ebnf.s
            Case EBNFKind.OneOrMoreOf, EBNFKind.ZeroOrMoreOf, EBNFKind.ZeroOrOneOf
                Dim op = If(ebnf.Kind = EBNFKind.OneOrMoreOf, "+", If(ebnf.Kind = EBNFKind.ZeroOrMoreOf, "*", "?"))
                If ebnf.Children(0).Kind = EBNFKind.Choice OrElse ebnf.Children(0).Kind = EBNFKind.Sequence Then
                    r = "( " & ToString(ebnf.Children(0)) & " )" & op
                Else
                    r = ToString(ebnf.Children(0)) & op
                End If
            Case EBNFKind.Choice
                Dim prevElement As EBNF = Nothing
                For Each c In ebnf.Children
                    If prevElement IsNot Nothing Then r &= If(r.Last = vbTab, "| ", " | ")
                    r &= ToString(c)
                    prevElement = c
                Next
            Case EBNFKind.Sequence
                Dim prevElement As EBNF = Nothing
                For Each c In ebnf.Children
                    If prevElement IsNot Nothing Then r &= If(r = "" OrElse r.Last = vbTab, "", " ")
                    If c.Kind = EBNFKind.Choice Then r &= "( " & ToString(c) & " )" Else r &= ToString(c)
                    prevElement = c
                Next
            Case Else : r = "???"
        End Select
        If Not String.IsNullOrEmpty(ebnf.FollowingComment) Then r &= " //" & ebnf.FollowingComment
        If ebnf.FollowingNewline Then r &= vbCrLf & vbTab
        Return r
    End Function

    Public Shared Function ReadFile(fn As String) As Grammar
        Return ReadString(IO.File.ReadAllText(fn), IO.Path.GetFileNameWithoutExtension(fn))
    End Function

    Public Shared Function ReadString(src As String, grammarName As String) As Grammar
        Return New Grammar With {.Productions = ReadInternal(src).ToList(), .Name = grammarName}
    End Function

    Private Shared Iterator Function ReadInternal(src As String) As IEnumerable(Of Production)
        Dim tokens = Tokenize(src)
        While tokens.Count > 0
            Dim t = tokens.First.Value : tokens.RemoveFirst()
            If t = "grammar" Then
                While tokens.Count > 0 AndAlso tokens.First.Value <> ";" : tokens.RemoveFirst() : End While
                If tokens.Count > 0 AndAlso tokens.First.Value = ";" Then tokens.RemoveFirst()
                If tokens.Count > 0 AndAlso tokens.First.Value = vbCrLf Then tokens.RemoveFirst()
            ElseIf t.StartsWith("//") Then
                Yield New Production With {.Comment = t.Substring(2)}
                If tokens.Count > 0 AndAlso tokens.First.Value = vbCrLf Then tokens.RemoveFirst()
            ElseIf t = vbCrLf Then
                Yield New Production
            Else
                Dim comment = "", newline = False
                While tokens.Count > 0 AndAlso tokens.First.Value = vbCrLf : tokens.RemoveFirst() : newline = True : End While
                If Not tokens.First.Value = ":" Then Throw New Exception($"After '{t}' expected ':' not {tokens.First.Value}")
                tokens.RemoveFirst()
                GobbleUpComments(tokens, comment, newline)
                Dim p = ParseProduction(tokens, comment)
                GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
                If tokens.Count > 0 AndAlso tokens.First.Value = ";" Then tokens.RemoveFirst()
                If tokens.Count > 0 AndAlso tokens.First.Value = vbCrLf Then tokens.RemoveFirst()
                Dim production As New Production With {.Comment = comment, .EBNF = p, .ProductionName = t, .RuleStartsOnNewLine = newline}
                While tokens.Count > 0 AndAlso tokens.First.Value.StartsWith("//")
                    production.Comment &= tokens.First.Value.Substring(2) : tokens.RemoveFirst()
                    If tokens.First.Value = vbCrLf Then tokens.RemoveFirst()
                End While
                Yield production
            End If
        End While
    End Function

    Private Shared Function SkipWhitespace(n As LinkedList(Of String)) As LinkedList(Of String)
        Return n
    End Function

    Private Shared Function Tokenize(s As String) As LinkedList(Of String)
        s = s.Trim()
        Dim tokens As New LinkedList(Of String), pos = 0

        While (pos < s.Length)
            If s(pos) = ":"c Then
                tokens.AddLast(":") : pos += 1
            ElseIf s(pos) = "*"c Then
                tokens.AddLast("*") : pos += 1
            ElseIf s(pos) = "?"c Then
                tokens.AddLast("?") : pos += 1
            ElseIf s(pos) = "|"c Then
                tokens.AddLast("|") : pos += 1
            ElseIf s(pos) = "+"c Then
                tokens.AddLast("+") : pos += 1
            ElseIf s(pos) = ";"c Then
                tokens.AddLast(";") : pos += 1
            ElseIf s(pos) = "("c Then
                tokens.AddLast("(") : pos += 1
            ElseIf s(pos) = ")"c Then
                tokens.AddLast(")") : pos += 1
            ElseIf pos + 1 < s.Length AndAlso s.Substring(pos, 2) = vbCrLf Then
                tokens.AddLast(vbCrLf) : pos += 2
            ElseIf s.Substring(pos, 1) = vbCr Then
                tokens.AddLast(vbCrLf) : pos += 1
            ElseIf s.Substring(pos, 1) = vbLf Then
                tokens.AddLast(vbCrLf) : pos += 1
            ElseIf pos + 1 < s.Length AndAlso s.Substring(pos, 2) = "//" Then
                pos += 2
                Dim t = ""
                While pos < s.Length AndAlso s(pos) <> vbCr(0) AndAlso s(pos) <> vbLf(0)
                    t += s(pos) : pos += 1
                End While
                If t.Contains("*)") Then Throw New Exception("Comments may not include *)")
                tokens.AddLast("//" & t)
            ElseIf s(pos) = "'" Then
                Dim t = "" : pos += 1
                While pos < s.Length AndAlso s.Substring(pos, 1) <> "'"c
                    If s.Substring(pos, 2) = "\\" Then
                        t &= "\" : pos += 2
                    ElseIf s.Substring(pos, 2) = "\'" Then
                        t &= "'" : pos += 2
                    ElseIf s.Substring(pos, 1) = "\" Then
                        Throw New Exception("Terminals may not include \ except in \\ or \'")
                    Else
                        t &= s(pos) : pos += 1
                    End If
                End While
                If t.Contains(vbCrLf) OrElse t.Contains(vbCr) OrElse t.Contains(vbCrLf) Then Throw New Exception("Terminals must be single-line")
                tokens.AddLast("'" & t & "'") : pos += 1
            Else
                Dim t = ""
                While pos < s.Length AndAlso Not String.IsNullOrWhiteSpace(s(pos)) AndAlso
                    s(pos) <> ":"c AndAlso s(pos) <> "*"c AndAlso s(pos) <> "?"c AndAlso s(pos) <> ";" AndAlso
                    s(pos) <> vbCr(0) AndAlso s(pos) <> vbLf(0) AndAlso s(pos) <> "'"c AndAlso s(pos) <> "("c AndAlso s(pos) <> ")"c AndAlso
                    s(pos) <> "+"c AndAlso
                    (pos + 1 >= s.Length OrElse s.Substring(pos, 2) <> "//")
                    t &= s(pos) : pos += 1
                End While
                tokens.AddLast(t)
            End If
            ' Bump up to the next non-whitespace character:
            While pos < s.Length AndAlso s(pos) <> vbCr(0) AndAlso s(pos) <> vbLf(0) AndAlso String.IsNullOrWhiteSpace(s(pos))
                pos += 1
            End While
        End While

        Return tokens
    End Function


    Private Shared Sub GobbleUpComments(tokens As LinkedList(Of String), ByRef ExtraComments As String, ByRef HasNewline As Boolean)
        If tokens.Count = 0 Then Return
        While True
            If tokens.First.Value.StartsWith("//") Then
                ExtraComments &= tokens.First.Value.Substring(2) : tokens.RemoveFirst()
            ElseIf tokens.First.Value = vbCrLf Then
                HasNewline = True : tokens.RemoveFirst() : If ExtraComments.Length > 0 Then ExtraComments &= " "
            Else
                Exit While
            End If
        End While
        ExtraComments = ExtraComments.TrimEnd()
    End Sub

    Private Shared Function ParseProduction(tokens As LinkedList(Of String), ByRef ExtraComments As String) As EBNF
        If tokens.Count = 0 Then Throw New Exception("empty input stream")
        GobbleUpComments(tokens, ExtraComments, False)
        Return ParsePar(tokens, ExtraComments)
    End Function

    Private Shared Function ParsePar(tokens As LinkedList(Of String), ByRef ExtraComments As String) As EBNF
        Dim pp As New LinkedList(Of EBNF)
        If tokens.First.Value = "|" Then
            tokens.RemoveFirst()
            GobbleUpComments(tokens, ExtraComments, False)
        End If
        pp.AddLast(ParseSeq(tokens, ExtraComments))
        While tokens.Count > 0 AndAlso tokens.First.Value = "|"
            tokens.RemoveFirst()
            GobbleUpComments(tokens, ExtraComments, pp.Last.Value.FollowingNewline)
            pp.AddLast(ParseSeq(tokens, ExtraComments))
        End While
        If pp.Count = 1 Then Return pp(0)
        Return New EBNF With {.Kind = EBNFKind.Choice, .Children = pp.ToList}
    End Function

    Private Shared Function ParseSeq(tokens As LinkedList(Of String), ByRef ExtraComments As String) As EBNF
        Dim pp As New LinkedList(Of EBNF)
        pp.AddLast(ParseUnary(tokens, ExtraComments))
        While tokens.Count > 0 AndAlso tokens.First.Value <> "|" AndAlso tokens.First.Value <> ";" AndAlso tokens.First.Value <> ")"
            GobbleUpComments(tokens, ExtraComments, pp.Last.Value.FollowingNewline)
            pp.AddLast(ParseUnary(tokens, ExtraComments))
        End While
        If pp.Count = 1 Then Return pp(0)
        Return New EBNF With {.Kind = EBNFKind.Sequence, .Children = pp.ToList}
    End Function

    Private Shared Function ParseUnary(tokens As LinkedList(Of String), ByRef ExtraComments As String) As EBNF
        Dim p = ParseAtom(tokens, ExtraComments)
        While tokens.Count > 0
            If tokens.First.Value = "+" Then
                tokens.RemoveFirst()
                p = New EBNF With {.Kind = EBNFKind.OneOrMoreOf, .Children = {p}.ToList}
                GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
            ElseIf tokens.First.Value = "*" Then
                tokens.RemoveFirst()
                p = New EBNF With {.Kind = EBNFKind.ZeroOrMoreOf, .Children = {p}.ToList}
                GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
            ElseIf tokens.First.Value = "?" Then
                tokens.RemoveFirst()
                p = New EBNF With {.Kind = EBNFKind.ZeroOrOneOf, .Children = {p}.ToList}
                GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
            Else
                Exit While
            End If
        End While
        Return p
    End Function

    Private Shared Function ParseAtom(tokens As LinkedList(Of String), ByRef ExtraComments As String) As EBNF
        If tokens.First.Value = "(" Then
            tokens.RemoveFirst()
            Dim p = ParseProduction(tokens, ExtraComments)
            If tokens.Count = 0 OrElse tokens.First.Value <> ")" Then Throw New Exception("mismatched parentheses")
            tokens.RemoveFirst()
            GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
            Return p
        ElseIf tokens.First.Value.StartsWith("'") Then
            Dim t = tokens.First.Value : tokens.RemoveFirst()
            t = t.Substring(1, t.Length - 2)
            Dim p = New EBNF With {.Kind = EBNFKind.Terminal, .s = t}
            If t.StartsWith("<") AndAlso t.EndsWith(">") Then
                p.Kind = EBNFKind.ExtendedTerminal : p.s = t.Substring(1, t.Length - 2)
                If p.s.Contains("?") Then Throw New Exception("A special-terminal may not contain a question-mark '?'")
                If p.s = "" Then Throw New Exception("A terminal may not be '<>'")
            Else
                If t.Contains("'") AndAlso t.Contains("""") Then Throw New Exception("A terminal must either contain no ' or no """)
            End If
            GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
            Return p
        Else
            Dim t = tokens.First.Value : tokens.RemoveFirst()
            Dim p = New EBNF With {.Kind = EBNFKind.Reference, .s = t}
            GobbleUpComments(tokens, p.FollowingComment, p.FollowingNewline)
            Return p
        End If
    End Function

End Class
