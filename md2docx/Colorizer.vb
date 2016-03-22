Imports System.Runtime.CompilerServices
Imports Microsoft.CodeAnalysis

Module Colorizer

    Public Class ColorizedWord
        Public Text As String
        Public Red As Integer
        Public Green As Integer
        Public Blue As Integer
        Public IsItalic As Boolean

        Public Overrides Function ToString() As String
            Return Text
        End Function
    End Class

    Public Class ColorizedLine
        Public Words As New List(Of ColorizedWord)

        Public Overrides Function ToString() As String
            Return String.Join("", Words.Select(Function(w) w.Text))
        End Function
    End Class

    Public Iterator Function Colorize(code As String, language As String) As IEnumerable(Of ColorizedLine)
        Dim words As IEnumerable(Of ColorizedWord) = Nothing
        If language = "csharp" OrElse language = "c#" OrElse language = "cs" Then
            words = CSharpColorizer.Colorize(code)
        ElseIf language = "antlr" Then
            words = AntlrColorizer.Colorize(code)
        ElseIf language = "" Then
            words = ColorizePlainText(code)
        Else
            Throw New NotSupportedException("unrecognized language")
        End If

        Dim encounteredFirstLinebreak = False
        Dim currentLine As New ColorizedLine
        Dim currentWord As ColorizedWord = Nothing
        For Each nextWord In words
            If nextWord Is Nothing Then ' linebreak
                If currentWord IsNot Nothing Then currentLine.Words.Add(currentWord)
                If encounteredFirstLinebreak OrElse currentLine.Words.Count > 0 Then Yield currentLine
                encounteredFirstLinebreak = True
                currentLine = New ColorizedLine
                currentWord = Nothing
            ElseIf currentWord Is Nothing Then ' first word on line
                currentWord = nextWord
            ElseIf String.IsNullOrWhiteSpace(currentWord.Text) Then ' merge the currentWord into the new one
                nextWord.Text = currentWord.Text & nextWord.Text
                currentWord = nextWord
            ElseIf String.IsNullOrWhiteSpace(nextWord.Text) Then ' merge the word into the previous one
                currentWord.Text = currentWord.Text & nextWord.Text
            ElseIf currentWord.Red = nextWord.Red AndAlso currentWord.Green = nextWord.Green AndAlso currentWord.Blue = nextWord.Blue AndAlso currentWord.IsItalic = nextWord.IsItalic Then
                currentWord.Text = currentWord.Text & nextWord.Text
            Else
                currentLine.Words.Add(currentWord)
                currentWord = nextWord
            End If
        Next
        If currentWord IsNot Nothing Then currentLine.Words.Add(currentWord)
        If currentLine.Words.Count > 0 Then Yield currentLine
    End Function


    Private Iterator Function ColorizePlainText(code As String) As IEnumerable(Of ColorizedWord)
        Dim lines = code.Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.None).ToList()
        If lines.Last = "" Then lines.RemoveAt(lines.Count - 1)
        For Each line In lines
            If line <> "" Then Yield New ColorizedWord With {.Text = line, .Red = 0, .Green = 0, .Blue = 0}
            Yield Nothing
        Next
    End Function


    Private Class AntlrColorizer

        Private Shared Function Col(token As String, color As String) As ColorizedWord
            Select Case color
                Case "PlainText" : Return New ColorizedWord With {.Text = token, .Red = 0, .Green = 0, .Blue = 0}
                Case "Production" : Return New ColorizedWord With {.Text = token, .Red = 106, .Green = 90, .Blue = 205}
                Case "Comment" : Return New ColorizedWord With {.Text = token, .Red = 0, .Green = 128, .Blue = 0}
                Case "Terminal" : Return New ColorizedWord With {.Text = token, .Red = 163, .Green = 21, .Blue = 21}
                Case "ExtendedTerminal" : Return New ColorizedWord With {.Text = token, .Red = 0, .Green = 0, .Blue = 0, .IsItalic = True}
                Case Else : Throw New Exception("bad color name")
            End Select
        End Function

        Public Shared Shadows Function Colorize(code As String) As IEnumerable(Of ColorizedWord)
            Dim grammar = Antlr.ReadString(code, "dummyGrammarName")
            Return Colorize(grammar)
        End Function

        Private Shared Shadows Iterator Function Colorize(grammar As Grammar) As IEnumerable(Of ColorizedWord)
            For Each p In grammar.Productions
                For Each word In Colorize(p) : Yield word : Next
            Next
        End Function

        Private Shared Shadows Iterator Function Colorize(p As Production) As IEnumerable(Of ColorizedWord)
            If p.EBNF Is Nothing AndAlso String.IsNullOrEmpty(p.Comment) Then
                Yield Nothing
                Return
            ElseIf p.EBNF Is Nothing Then
                Dim r As New List(Of Tuple(Of String, Integer, Integer, Integer))
                Yield Col("// " & p.Comment, "Comment")
                Yield Nothing
                Return
            Else
                Yield Col(p.ProductionName, "Production")
                Yield Col(":", "PlainText")
                If p.RuleStartsOnNewLine Then Yield Nothing
                Yield Col(vbTab, "PlainText")
                If p.RuleStartsOnNewLine Then Yield Col("| ", "PlainText")
                For Each word In Colorize(p.EBNF) : Yield word : Next
                Yield Col(";", "PlainText")
                If Not String.IsNullOrEmpty(p.Comment) Then Yield Col("  //" & p.Comment, "Comment")
                Yield Nothing
            End If
        End Function

        Public Shared Shadows Iterator Function Colorize(ebnf As EBNF) As IEnumerable(Of ColorizedWord)
            Select Case ebnf.Kind
                Case EBNFKind.Terminal
                    Yield Col("'" & ebnf.s.Replace("\", "\\").Replace("'", "\'") & "'", "Terminal")
                Case EBNFKind.ExtendedTerminal
                    Yield Col(ebnf.s.Replace("\", "\\").Replace("'", "\'"), "ExtendedTerminal")
                Case EBNFKind.Reference
                    Yield Col(ebnf.s, "Production")
                Case EBNFKind.OneOrMoreOf, EBNFKind.ZeroOrMoreOf, EBNFKind.ZeroOrOneOf
                    Dim op = If(ebnf.Kind = EBNFKind.OneOrMoreOf, "+", If(ebnf.Kind = EBNFKind.ZeroOrMoreOf, "*", "?"))
                    If ebnf.Children(0).Kind = EBNFKind.Choice OrElse ebnf.Children(0).Kind = EBNFKind.Sequence Then
                        Yield Col("( ", "PlainText")
                        For Each word In Colorize(ebnf.Children(0)) : Yield word : Next
                        Yield Col(" )", "PlainText")
                        Yield Col(op, "PlainText")
                    Else
                        For Each word In Colorize(ebnf.Children(0)) : Yield word : Next
                        Yield Col(op, "PlainText")
                    End If
                Case EBNFKind.Choice
                    Dim lastWasTab = False
                    Dim prevElement As EBNF = Nothing
                    For Each c In ebnf.Children
                        If prevElement IsNot Nothing Then Yield Col(If(lastWasTab, "| ", " | "), "PlainText")
                        For Each word In Colorize(c) : Yield word : lastWasTab = (word IsNot Nothing AndAlso word.Text = vbTab) : Next
                        prevElement = c
                    Next
                Case EBNFKind.Sequence
                    Dim lastWasNonTab = False
                    Dim prevElement As EBNF = Nothing
                    For Each c In ebnf.Children
                        ' put a space if r was a non-empty non-tab thing
                        If prevElement IsNot Nothing AndAlso lastWasNonTab Then Yield Col(" ", "PlainText")
                        If c.Kind = EBNFKind.Choice Then
                            Yield Col("( ", "PlainText")
                            For Each word In Colorize(c) : Yield word : Next
                            Yield Col(" )", "PlainText")
                            lastWasNonTab = True
                        Else
                            For Each word In Colorize(c) : Yield word : lastWasNonTab = (word IsNot Nothing AndAlso word.Text <> vbTab) : Next
                        End If
                        prevElement = c
                    Next
                Case Else
                    Throw New NotSupportedException("Unrecognized EBNF")
            End Select
            If Not String.IsNullOrEmpty(ebnf.FollowingComment) Then Yield Col(" //" & ebnf.FollowingComment, "Comment")
            If ebnf.FollowingNewline Then Yield Nothing : Yield Col(vbTab, "PlainText")
        End Function


    End Class



    Private Class CSharpColorizer
        Inherits SyntaxWalker
        ' This code is based on that of Shiv Kumar at http://www.matlus.com/c-to-html-syntax-highlighter-using-roslyn/


        Public Shared Function Colorize(code As String) As IEnumerable(Of ColorizedWord)
            Dim ref_mscorlib = MetadataReference.CreateFromFile(GetType(Object).Assembly.Location)
            Dim ref_system = MetadataReference.CreateFromFile(GetType(Uri).Assembly.Location)
            Dim ref_systemcore = MetadataReference.CreateFromFile(GetType(Enumerable).Assembly.Location)
            Dim ref_systemcollectionsimmutable = MetadataReference.CreateFromFile(GetType(Immutable.ImmutableArray).Assembly.Location)
            Dim ienumerable = MetadataReference.CreateFromFile(GetType(IEnumerable(Of Integer)).Assembly.Location)

            For i = 0 To 1
                Dim isScript = (i = 0)
                Dim code1 = If(isScript, code, "using System; using System.Collections; using System.Collections.Generic;" & vbCrLf & code)
                Dim parse_options = If(isScript, New CSharp.CSharpParseOptions(kind:=SourceCodeKind.Script), Nothing)
                Dim compile_options = If(isScript, New CSharp.CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings:={"System", "System.Collections", "System.Collections.Generic"}), Nothing)
                Dim compilationUnit = CSharp.SyntaxFactory.ParseCompilationUnit(code1, options:=parse_options)
                Dim syntaxTree = compilationUnit.SyntaxTree
                Dim compilation As CSharp.CSharpCompilation
                Try
                    compilation = CSharp.CSharpCompilation.Create("dummyAssemblyName", {syntaxTree}, {ref_mscorlib, ref_system, ref_systemcore, ref_systemcollectionsimmutable}, compile_options)
                Catch ex As Exception
                    ' I believe that Script will parse all complete programs fine, so the convoluted "if" code here isn't needed.
                    ' But let's see.
                    Throw New Exception("Oops! It looks like non-script compilation is needed after all")
                    Continue For
                End Try
                Dim semanticModel = compilation.GetSemanticModel(syntaxTree, True)
                Dim w As New CSharpColorizer With {.sm = semanticModel}
                w.Visit(syntaxTree.GetRoot)
                Return If(isScript, w.words, w.words.SkipWhile(Function(c) c IsNot Nothing).Skip(1)) ' skip the dummy "usings" line
            Next
            Throw New Exception("unable to parse code")
        End Function

        Private words As New LinkedList(Of ColorizedWord)
        Private sm As SemanticModel

        Private Sub New()
            MyBase.New(SyntaxWalkerDepth.StructuredTrivia)
        End Sub

        Protected Overrides Sub VisitToken(token As SyntaxToken)
            VisitTrivia(token.LeadingTrivia)

            Dim r As ColorizedWord = Nothing

            Dim specialCase = False
            If token.IsKeywordCS Then
                r = Col(token.Text, "Keyword")
            ElseIf token.KindCS = CSharp.SyntaxKind.StringLiteralToken Then
                r = Col(token.Text, "StringLiteral")
            ElseIf token.KindCS = CSharp.SyntaxKind.CharacterLiteralToken Then
                r = Col(token.Text, "StringLiteral")
            ElseIf token.KindCS = CSharp.SyntaxKind.IdentifierToken AndAlso TypeOf token.Parent Is CSharp.Syntax.TypeParameterSyntax Then
                r = Col(token.Text, "UserType")
            ElseIf token.KindCS = CSharp.SyntaxKind.IdentifierToken AndAlso TypeOf token.Parent Is CSharp.Syntax.SimpleNameSyntax Then
                Dim name = CType(token.Parent, CSharp.Syntax.SimpleNameSyntax)
                Dim symbol As ISymbol = Nothing
                Try
                    symbol = sm.GetSymbolInfo(name).Symbol ' How come this throws a NullRefException even when sm and name are non-null?
                    ' I don't know. So I'll brute-force hack around it.
                Catch ex As Exception
                End Try
                If symbol?.Kind = SymbolKind.NamedType OrElse
                    symbol?.Kind = SymbolKind.TypeParameter Then
                    r = Col(token.Text, "UserType")
                ElseIf symbol?.Kind = SymbolKind.Namespace OrElse
                        symbol?.Kind = SymbolKind.Parameter OrElse
                        symbol?.Kind = SymbolKind.Local OrElse
                        symbol?.Kind = SymbolKind.Field OrElse
                        symbol?.Kind = SymbolKind.Property Then
                    r = Col(token.Text, "PlainText")
                End If
            ElseIf token.KindCS = CSharp.SyntaxKind.IdentifierToken AndAlso TypeOf token.Parent Is CSharp.Syntax.TypeDeclarationSyntax Then
                Dim name = CType(token.Parent, CSharp.Syntax.TypeDeclarationSyntax)
                Dim symbol = sm.GetDeclaredSymbol(name)
                If symbol?.Kind = Microsoft.CodeAnalysis.SymbolKind.NamedType Then
                    r = Col(token.Text, "UserType")
                End If
            End If

            If r Is Nothing Then
                If token.Parent.KindCS = CSharp.SyntaxKind.EnumDeclaration Then
                    r = Col(token.Text, "UserType")
                ElseIf token.Parent.KindCS = CSharp.SyntaxKind.IdentifierName Then
                    If token.Parent.Parent.KindCS = CSharp.SyntaxKind.Parameter OrElse
                        token.Parent.Parent.KindCS = CSharp.SyntaxKind.Attribute OrElse
                        token.Parent.Parent.KindCS = CSharp.SyntaxKind.CatchDeclaration OrElse
                        token.Parent.Parent.KindCS = CSharp.SyntaxKind.ObjectCreationExpression OrElse
                        (token.Parent.Parent.KindCS = CSharp.SyntaxKind.ForEachStatement AndAlso token.GetNextToken().RawKind <> CSharp.SyntaxKind.CloseParenToken) OrElse
                        (token.Parent.Parent.Parent.KindCS = CSharp.SyntaxKind.CaseSwitchLabel AndAlso token.GetPreviousToken().RawKind <> CSharp.SyntaxKind.DotToken) OrElse
                        token.Parent.Parent.KindCS = CSharp.SyntaxKind.MethodDeclaration OrElse
                        TryCast(token.Parent.Parent, CSharp.Syntax.CastExpressionSyntax)?.Type Is token.Parent OrElse ' e.g. "(Foo)x" the Foo
                        TryCast(token.Parent.Parent, CSharp.Syntax.TypeConstraintSyntax)?.Type Is token.Parent OrElse ' e.g. "where T:Foo" the Foo
                        token.Parent.Parent.KindCS = CSharp.SyntaxKind.BaseList OrElse ' e.g. "public sealed class BuilderRouteHandler  IRouteHandler" IRouteHandler in this case
                        token.Parent.Parent.Parent.KindCS = CSharp.SyntaxKind.TypeOfExpression OrElse ' e.g. "Type baseBuilderType = TypeOf(BaseBuilder);" BaseBuilder in this case
                        token.Parent.Parent.KindCS = CSharp.SyntaxKind.VariableDeclaration OrElse ' e.g. "private DbProviderFactory dbProviderFactory;" Or "DbConnection connection = dbProviderFactory.CreateConnection();"
                        token.Parent.Parent.KindCS = CSharp.SyntaxKind.TypeArgumentList OrElse ' e.g. "DbTypes = New Dictionary();" DbType in this case
                        (token.Parent.Parent.KindCS = CSharp.SyntaxKind.SimpleMemberAccessExpression AndAlso token.Parent.Parent.Parent.RawKind = CSharp.SyntaxKind.Argument AndAlso token.GetPreviousToken().RawKind <> CSharp.SyntaxKind.DotToken AndAlso Not Char.IsLower(token.Text(0))) OrElse ' // e.g. "DbTypes.Add("int", DbType.Int32);" DbType in this case
                        (token.Parent.Parent.KindCS = CSharp.SyntaxKind.SimpleMemberAccessExpression AndAlso token.GetPreviousToken().RawKind <> CSharp.SyntaxKind.DotToken AndAlso Not Char.IsLower(token.Text(0))) Then
                        r = Col(token.Text, "UserType")
                    End If
                ElseIf token.Parent.KindCS = CSharp.SyntaxKind.GenericName Then
                    If token.Parent.Parent.KindCS = CSharp.SyntaxKind.VariableDeclaration OrElse ' e.g. "private static readonly HashSet patternHashSet = New HashSet();" the first HashSet in this case
                        token.Parent.Parent.KindCS = CSharp.SyntaxKind.ObjectCreationExpression OrElse ' e.g. "private static readonly HashSet patternHashSet = New HashSet();" the second HashSet in this case
                        TryCast(token.Parent, CSharp.Syntax.GenericNameSyntax)?.Identifier = token Then ' e.g. "Box<int>" the word Box
                        r = Col(token.Text, "UserType")
                    End If
                End If
            End If

            If r Is Nothing And Not String.IsNullOrEmpty(token.Text) Then ' Empty comes from EndOfFile, OmmittedToken, ...
                r = Col(token.Text, "PlainText")
            End If

            If r IsNot Nothing Then words.AddLast(r)

            VisitTrivia(token.TrailingTrivia)

        End Sub

        Overloads Sub VisitTrivia(trivias As SyntaxTriviaList)
            For Each trivia In trivias
                Dim text = trivia.ToFullString
                If trivia.KindCS = CSharp.SyntaxKind.EndOfLineTrivia Then
                    words.AddLast(CType(Nothing, ColorizedWord))
                ElseIf trivia.KindCS = CSharp.SyntaxKind.MultiLineCommentTrivia OrElse
                trivia.RawKind = CSharp.SyntaxKind.SingleLineCommentTrivia OrElse
                trivia.RawKind = CSharp.SyntaxKind.MultiLineDocumentationCommentTrivia OrElse
                trivia.RawKind = CSharp.SyntaxKind.SingleLineDocumentationCommentTrivia Then
                    words.AddLast(Col(text, "Comment"))
                ElseIf trivia.KindCS = CSharp.SyntaxKind.DisabledTextTrivia Then
                    words.AddLast(Col(text, "ExcludedCode"))
                ElseIf trivia.KindCS = CSharp.SyntaxKind.RegionDirectiveTrivia OrElse
                    trivia.RawKind = CSharp.SyntaxKind.EndRegionDirectiveTrivia Then
                    words.AddLast(Col(text, "Region"))
                Else
                    words.AddLast(Col(text, "PlainText"))
                End If
            Next
        End Sub

        Private Function Col(token As String, color As String) As ColorizedWord
            Select Case color
                Case "PlainText" : Return New ColorizedWord With {.Text = token, .Red = 0, .Green = 0, .Blue = 0}
                Case "Keyword" : Return New ColorizedWord With {.Text = token, .Red = 0, .Green = 0, .Blue = 255}
                Case "UserType" : Return New ColorizedWord With {.Text = token, .Red = 43, .Green = 145, .Blue = 175}
                Case "StringLiteral" : Return New ColorizedWord With {.Text = token, .Red = 163, .Green = 21, .Blue = 21}
                Case "Comment" : Return New ColorizedWord With {.Text = token, .Red = 0, .Green = 128, .Blue = 0}
                Case "ExcludedCode" : Return New ColorizedWord With {.Text = token, .Red = 128, .Green = 128, .Blue = 128}
                Case "Region" : Return New ColorizedWord With {.Text = token, .Red = 224, .Green = 224, .Blue = 224}
                Case Else : Throw New Exception("bad color name")
            End Select
        End Function

    End Class


    <Extension>
    Function IsKeywordCS(token As SyntaxToken) As Boolean
        Return CSharp.CSharpExtensions.IsKeyword(token)
    End Function

    <Extension>
    Function IsKeywordVB(token As SyntaxToken) As Boolean
        Return VisualBasic.IsKeyword(token)
    End Function

    <Extension>
    Function KindCS(node As SyntaxNode) As CSharp.SyntaxKind
        Return CSharp.CSharpExtensions.Kind(node)
    End Function

    <Extension>
    Function KindCS(token As SyntaxToken) As CSharp.SyntaxKind
        Return CSharp.CSharpExtensions.Kind(token)
    End Function

    <Extension>
    Function KindCS(trivia As SyntaxTrivia) As CSharp.SyntaxKind
        Return CSharp.CSharpExtensions.Kind(trivia)
    End Function

    <Extension>
    Function KindVB(node As SyntaxNode) As VisualBasic.SyntaxKind
        Return VisualBasic.Kind(node)
    End Function


End Module
