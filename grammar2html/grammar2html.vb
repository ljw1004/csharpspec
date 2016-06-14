' This library is for emitting an EBNF grammar into a hyperlinked HTML document.
'
' The HTML document also includes in it the result of some useful grammar analysis --
' e.g. which characters might start a given production, or might follow it; and whether
' the production might be empty.
' Those results can be substantial! So, to save space, the HTML is emitted in a pretty
' compact form, and relies upon javascript to expand them out upon-demand.
'
' This code is written in VB rather than C# for ease of constructing the HTML: with XML-literals.


Public Class Grammar
    Public Productions As New List(Of Production)
    Public Name As String

    Public Function ToHtml() As String
        Return Html.ToString(Me)
    End Function
End Class


Public Class Production
    Public EBNF As EBNF, ProductionName As String ' optional. ProductionName contains no whitespace and is not delimited by '
    Public Comment As String ' optional. Does not contain *) or newline
    Public RuleStartsOnNewLine As Boolean ' e.g. "Rule: \n | Choice1"
    Public Link, LinkName As String ' optional. Link to the spec

    Public Overrides Function ToString() As String
        Return $"{ProductionName} := {EBNF}"
    End Function
End Class

Public Class EBNF
    Public Kind As EBNFKind
    Public s As String
    Public Children As List(Of EBNF)
    Public FollowingWhitespace As String
    Public FollowingComment As String = "" ' Does not contain *) or newline
    Public FollowingNewline As Boolean

    Public Overrides Function ToString() As String
        Dim c = Children.Select(Function(child) child.ToString())
        If Kind = EBNFKind.ExtendedTerminal Then Return s
        If Kind = EBNFKind.Reference Then Return s
        If Kind = EBNFKind.Terminal Then Return s
        If Kind = EBNFKind.OneOrMoreOf Then Return $"({Children(0)})+"
        If Kind = EBNFKind.ZeroOrMoreOf Then Return $"({Children(0)})*"
        If Kind = EBNFKind.ZeroOrOneOf Then Return $"({Children(0)})?"
        If Kind = EBNFKind.Choice Then Return String.Join(" | ", c)
        If Kind = EBNFKind.Sequence Then Return String.Join(" ", c)
        Return "???"
    End Function
End Class


Public Enum EBNFKind
    ZeroOrMoreOf ' has exactly one child   e*  {e}
    OneOrMoreOf  ' has exactly one child   e+  [e]
    ZeroOrOneOf  ' has exactly one child   e?  {e}-
    Sequence     ' has 2+ children
    Choice       ' has 2+ children
    Terminal     ' has 0 children and an unescaped string without linebreaks which is not "<>", which either does not contain ' or does not contain "
    ExtendedTerminal ' has 0 children and a string without linebreaks, which does not itself contain '?'
    Reference    ' has 0 children and a string
End Enum




Friend Class Html
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

    Sub New(grammar As Grammar)
        GrammarName = grammar.Name
        For Each p In grammar.Productions
            If p.EBNF Is Nothing Then Continue For
            Dim choices = If(p.EBNF.Kind = EBNFKind.Choice, CType(p.EBNF.Children, IEnumerable(Of EBNF)), {p.EBNF})
            If flatten(choices).Where(Function(e) e.Kind = EBNFKind.Choice).Count > 0 Then Throw New Exception("nested choice not implemented")
            Me.Productions(p.ProductionName) = choices
            ' In cases where the MD defined a production multiple times (which is allowed),
            ' the above statement means we'll only hyperlink to the final version of it
            If p.Link IsNot Nothing Then Me.ProductionReferences(p.ProductionName) = Tuple.Create(p.Link, p.LinkName)
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

  // Only show popup tooltips for grammar links within this page; not for spec links
  if (a.href.indexOf(".md") > -1) return;

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
                        <div id="popup" style="visibility:hidden; position:absolute; left:16em; width: auto; top:0; height:auto;"></div>
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



