Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing
Imports FSharp.Markdown
Imports Microsoft.FSharp.Core

Class MarkdownSpec
    Private s As String
    Private files As IEnumerable(Of String)
    Public Grammar As New Grammar
    Public Sections As New List(Of SectionRef)

    Class SectionRef
        Public Number As String       ' "10.1.2"
        Public Title As String        ' "Goto Statement"
        Public Level As Integer       ' 1-based level, e.g. 3
        Public Url As String          ' statements.md#goto-statement
        Public BookmarkName As String ' _Toc00023
        Public Shared count As Integer = 1

        Sub New(mdh As MarkdownParagraph.Heading, filename As String)
            Level = mdh.Item1
            Dim spans = mdh.Item2
            If spans.Length = 1 AndAlso spans.First.IsLiteral Then
                Title = mdunescape(CType(spans.First, MarkdownSpan.Literal)).Trim()
                If Title.Contains("&amp;") Then Stop
            ElseIf spans.Length = 1 AndAlso spans.First.IsInlineCode Then
                Title = CType(spans.First, MarkdownSpan.InlineCode).Item.Trim()
            Else
                Throw New NotSupportedException("Heading must be a single literal/inlinecode")
            End If
            For Each c In Title
                If c >= "a"c AndAlso c <= "z"c Then Url &= c
                If c >= "A"c AndAlso c <= "Z"c Then Url &= Char.ToLowerInvariant(c)
                If c >= "0"c AndAlso c <= "9"c Then Url &= c
                If c = "-"c OrElse c = "_"c Then Url &= c
                If c = " "c Then Url &= "-"c
            Next
            Url = filename & "#" & Url
            BookmarkName = $"_Toc{count:00000}" : count += 1
        End Sub
    End Class

    Public Shared Function ReadString(s As String) As MarkdownSpec
        Dim md As New MarkdownSpec With {.s = s}
        md.Init()
        Return md
    End Function

    Public Shared Function ReadFiles(files As IEnumerable(Of String)) As MarkdownSpec
        Dim md As New MarkdownSpec With {.files = files}
        md.Init()
        Return md
    End Function

    Private Sub Init()
        ' (1) Add sections into the dictionary
        Dim h1 = 0, h2 = 0, h3 = 0, h4 = 0
        Dim url = "", title = ""

        ' (2) Turn all the antlr code blocks into a grammar
        Dim sbantlr As New StringBuilder

        For Each src In Sources()
            Dim filename = Path.GetFileName(src.Item1), md = Markdown.Parse(src.Item2)

            For Each mdp In md.Paragraphs
                If mdp.IsHeading Then
                    Dim sr = New SectionRef(CType(mdp, MarkdownParagraph.Heading), filename)
                    If sr.Level = 1 Then h1 += 1 : h2 = 0 : h3 = 0 : h4 = 0 : sr.Number = $"{h1}"
                    If sr.Level = 2 Then h2 += 1 : h3 = 0 : h4 = 0 : sr.Number = $"{h1}.{h2}"
                    If sr.Level = 3 Then h3 += 1 : h4 = 0 : sr.Number = $"{h1}.{h2}.{h3}"
                    If sr.Level = 4 Then h4 += 1 : sr.Number = $"{h1}.{h2}.{h3}.{h4}"
                    Sections.Add(sr)
                    url = sr.Url
                    title = sr.Title

                ElseIf mdp.IsCodeBlock Then
                    Dim mdc = CType(mdp, MarkdownParagraph.CodeBlock), code = mdc.Item1, lang = mdc.Item2
                    If lang <> "antlr" Then Continue For
                    Dim g = Antlr.ReadString(code, "")
                    For Each p In g.Productions
                        p.Link = url : p.LinkName = title
                        Grammar.Productions.Add(p)
                    Next
                End If
            Next

        Next
    End Sub

    Private Iterator Function Sources() As IEnumerable(Of Tuple(Of String, String))
        If s IsNot Nothing Then Yield Tuple.Create("", s)
        If files IsNot Nothing Then
            For Each fn In files
                Yield Tuple.Create(fn, File.ReadAllText(fn))
            Next
        End If
    End Function

    Function FindToc(body As Body, ByRef iFirst As Integer, ByRef iLast As Integer, ByRef instr As String, ByRef secBreak As Paragraph) As Boolean
        iFirst = -1 : iLast = -1 : instr = Nothing : secBreak = Nothing

        For i = 0 To body.ChildElements.Count - 1
            Dim p = TryCast(body.ChildElements.GetItem(i), Paragraph)
            If p Is Nothing Then Continue For

            ' The TOC might be a simple field
            Dim sf = p.OfType(Of SimpleField).FirstOrDefault
            If sf IsNot Nothing AndAlso sf.Instruction.Value.Contains("TOC") Then
                If iFirst <> -1 Then Throw New Exception("Found start of TOC and then another simple TOC")
                iFirst = i : iLast = i : instr = sf.Instruction.Value
                Exit For
            End If

            ' or it might be a complex field
            Dim runElements = (From r In p.OfType(Of Run) From e In r Select e).ToList
            Dim f1 = runElements.FindIndex(Function(f) TypeOf f Is FieldChar AndAlso CType(f, FieldChar).FieldCharType.Value = FieldCharValues.Begin)
            Dim f2 = runElements.FindIndex(Function(f) TypeOf f Is FieldCode AndAlso CType(f, FieldCode).Text.Contains("TOC"))
            Dim f3 = runElements.FindIndex(Function(f) TypeOf f Is FieldChar AndAlso CType(f, FieldChar).FieldCharType.Value = FieldCharValues.Separate)
            Dim f4 = runElements.FindIndex(Function(f) TypeOf f Is FieldChar AndAlso CType(f, FieldChar).FieldCharType.Value = FieldCharValues.End)

            If f1 <> -1 AndAlso f2 <> -1 AndAlso f3 <> -1 AndAlso f2 > f1 AndAlso f3 > f2 Then
                If iFirst <> -1 Then Throw New Exception("Found start of TOC and then another start of TOC")
                iFirst = i : instr = CType(runElements(f2), FieldCode).Text
            End If
            If f4 <> -1 AndAlso f4 > f1 AndAlso f4 > f2 AndAlso f4 > f3 Then
                iLast = i
                If iFirst <> -1 Then Exit For
            End If


        Next

        If iFirst = -1 Then Return False
        If iLast = -1 Then Throw New Exception("Found start of TOC field, but not end")
        For i = iFirst To iLast
            Dim p = TryCast(body.ChildElements.GetItem(i), Paragraph)
            If p Is Nothing Then Continue For
            Dim sp = p.ParagraphProperties.OfType(Of SectionProperties).FirstOrDefault
            If sp Is Nothing Then Continue For
            If i <> iLast Then Throw New Exception("Found section break within TOC field")
            secBreak = New Paragraph(New Run(New Text(""))) With {.ParagraphProperties = New ParagraphProperties(sp.CloneNode(True))}
        Next
        Return True
    End Function

    Public Sub WriteFile(basedOn As String, fn As String)
        Using templateDoc = WordprocessingDocument.Open(basedOn, False),
                resultDoc = WordprocessingDocument.Create(fn, WordprocessingDocumentType.Document)

            For Each part In templateDoc.Parts
                resultDoc.AddPart(part.OpenXmlPart, part.RelationshipId)
            Next
            Dim body = resultDoc.MainDocumentPart.Document.Body

            ' We have to find the TOC, if one exists, and replace it...
            Dim tocFirst = -1, tocLast = -1, tocInstr = "", tocSec As Paragraph = Nothing

            If FindToc(body, tocFirst, tocLast, tocInstr, tocSec) Then
                Dim tocRunFirst As New Run(New FieldChar With {.FieldCharType = FieldCharValues.Begin},
                                       New FieldCode With {.Text = tocInstr, .Space = SpaceProcessingModeValues.Preserve},
                                       New FieldChar With {.FieldCharType = FieldCharValues.Separate})
                Dim tocRunLast As New Run(New FieldChar With {.FieldCharType = FieldCharValues.End})
                '
                For i = tocLast To tocFirst Step -1
                    body.RemoveChild(body.ChildElements(i))
                Next
                Dim afterToc = body.ChildElements(tocFirst)
                '
                For i = 0 To Sections.Count - 1
                    Dim section = Sections(i)
                    If section.Level > 3 Then Continue For
                    Dim p As New Paragraph
                    If i = 0 Then p.AppendChild(tocRunFirst)
                    p.AppendChild(New Hyperlink(New Run(New Text(section.Number & " " & section.Title))) With {.Anchor = section.BookmarkName})
                    If i = Sections.Count - 1 Then p.AppendChild(tocRunLast)
                    p.ParagraphProperties = New ParagraphProperties(New ParagraphStyleId With {.Val = $"TOC{section.Level}"})
                    body.InsertBefore(p, afterToc)
                Next
                If tocSec IsNot Nothing Then body.InsertBefore(tocSec, afterToc)
            End If

            Dim sectionDictionary = Sections.ToDictionary(Function(sr) sr.Url)
            Dim maxBookmarkId = New StrongBox(Of Integer)(1 + Aggregate bookmark In body.Descendants(Of BookmarkStart)
                                                           Select CInt(bookmark.Id) Into Max)
            For Each src In Sources()
                For Each p In New MarkdownConverter(src.Item2, Path.GetFileName(src.Item1), resultDoc, sectionDictionary, maxBookmarkId).Paragraphs
                    body.AppendChild(p)
                Next
            Next


        End Using
    End Sub

    Shared Function mdunescape(literal As MarkdownSpan.Literal) As String
        Dim s = literal.Item
        s = s.Replace("&amp;", "&")
        s = s.Replace("&lt;", "<")
        s = s.Replace("&gt;", ">")
        s = s.Replace("&reg;", "®")
        Return s
    End Function


    Private Class MarkdownConverter
        Dim mddoc As MarkdownDocument
        Dim wdoc As WordprocessingDocument
        Dim sections As Dictionary(Of String, SectionRef)
        Dim maxBookmarkId As StrongBox(Of Integer)
        Dim filename As String

        Sub New(s As String, filename As String, wdoc As WordprocessingDocument, sections As Dictionary(Of String, SectionRef), maxBookmarkId As StrongBox(Of Integer))
            Me.mddoc = Markdown.Parse(s)
            Me.wdoc = wdoc
            Me.sections = sections
            Me.maxBookmarkId = maxBookmarkId
            Me.filename = filename
        End Sub

        Function Paragraphs() As IEnumerable(Of OpenXmlCompositeElement)
            Return Paragraphs2Paragraphs(mddoc.Paragraphs)
        End Function

        Iterator Function Paragraphs2Paragraphs(pars As IEnumerable(Of MarkdownParagraph)) As IEnumerable(Of OpenXmlCompositeElement)
            For Each md In pars
                For Each p In Paragraph2Paragraphs(md)
                    Yield p
                Next
            Next
        End Function

        Dim re_grammar As New Regex("^([a-zA-Z]+):?$")

        Iterator Function Paragraph2Paragraphs(md As MarkdownParagraph) As IEnumerable(Of OpenXmlCompositeElement)
            If md.IsHeading Then
                Dim mdh = CType(md, MarkdownParagraph.Heading), level = mdh.Item1, spans = mdh.Item2
                Dim sr = sections(New SectionRef(mdh, filename).Url)
                Dim props As New ParagraphProperties(New ParagraphStyleId() With {.Val = $"Heading{level}"})
                Dim p As New Paragraph With {.ParagraphProperties = props}
                maxBookmarkId.Value += 1
                p.AppendChild(New BookmarkStart With {.Name = sr.BookmarkName, .Id = CStr(maxBookmarkId.Value)})
                p.Append(Spans2Elements(spans))
                p.AppendChild(New BookmarkEnd With {.Id = CStr(maxBookmarkId.Value)})
                Yield p
                Return

            ElseIf md.IsParagraph Then
                Dim mdp = CType(md, MarkdownParagraph.Paragraph), spans = mdp.Item
                Yield New Paragraph(Spans2Elements(spans))
                Return

            ElseIf md.IsListBlock Then
                Dim mdl = CType(md, MarkdownParagraph.ListBlock)
                Dim flat = FlattenList(mdl)

                ' Let's figure out what kind of list it is - ordered or unordered? nested?
                Dim format0 = {"1", "1", "1"}
                For Each item In flat
                    format0(item.Level) = If(item.IsBulletOrdered, "1", "o")
                Next
                Dim format = String.Join("", format0)

                Dim numberingPart = If(wdoc.MainDocumentPart.NumberingDefinitionsPart, wdoc.MainDocumentPart.AddNewPart(Of NumberingDefinitionsPart)("NumberingDefinitionsPart001"))
                If numberingPart.Numbering Is Nothing Then numberingPart.Numbering = New Numbering()

                Dim createLevel = Function(level As Integer, isOrdered As Boolean) As Level
                                      Dim numFormat = NumberFormatValues.Bullet, levelText = "·"
                                      If isOrdered AndAlso level = 0 Then numFormat = NumberFormatValues.Decimal : levelText = "%1."
                                      If isOrdered AndAlso level = 1 Then numFormat = NumberFormatValues.LowerLetter : levelText = "%2."
                                      If isOrdered AndAlso level = 2 Then numFormat = NumberFormatValues.LowerRoman : levelText = "%3."
                                      Return New Level(New StartNumberingValue With {.Val = 1}, New NumberingFormat With {.Val = numFormat}, New LevelText With {.Val = levelText}, New ParagraphProperties(New Indentation With {.Left = CStr(540 + 360 * level), .Hanging = "360"})) With {.LevelIndex = level}
                                  End Function
                Dim level0 = createLevel(0, format(0) = "1")
                Dim level1 = createLevel(1, format(1) = "1")
                Dim level2 = createLevel(2, format(2) = "1")

                Dim abstracts = numberingPart.Numbering.OfType(Of AbstractNum).Select(Function(an) an.AbstractNumberId.Value).ToList()
                Dim aid = If(abstracts.Count = 0, 1, abstracts.Max() + 1)
                Dim abstract As New AbstractNum(New MultiLevelType() With {.Val = MultiLevelValues.Multilevel}, level0, level1, level2) With {.AbstractNumberId = aid}
                numberingPart.Numbering.InsertAt(abstract, 0)

                Dim instances = numberingPart.Numbering.OfType(Of NumberingInstance).Select(Function(ni) ni.NumberID.Value)
                Dim nid = If(instances.Count = 0, 1, instances.Max() + 1)
                Dim numInstance As New NumberingInstance(New AbstractNumId With {.Val = aid}) With {.NumberID = nid}
                numberingPart.Numbering.AppendChild(numInstance)

                ' We'll also figure out the indentation (for the benefit of those paragraphs that should be
                ' indendent with the list but aren't numbered). I'm not sure what the indent comes from.
                ' In the docx, each AbstractNum that I created has an indent for each of its levels,
                ' defaulted at 900, 1260, 1620, ... but I can't see where in the above code that's created?
                Dim calcIndent = Function(level%) CStr(540 + level * 360)

                For Each item In flat
                    Dim content = item.Paragraph
                    If content.IsParagraph OrElse content.IsSpan Then
                        Dim spans = If(content.IsParagraph, CType(content, MarkdownParagraph.Paragraph).Item, CType(content, MarkdownParagraph.Span).Item)
                        If item.HasBullet Then
                            Yield New Paragraph(Spans2Elements(spans)) With {.ParagraphProperties = New ParagraphProperties(New NumberingProperties(New ParagraphStyleId With {.Val = "ListParagraph"}, New NumberingLevelReference With {.Val = item.Level}, New NumberingId With {.Val = nid}))}
                        Else
                            Yield New Paragraph(Spans2Elements(spans)) With {.ParagraphProperties = New ParagraphProperties(New Indentation With {.Left = calcIndent(item.Level)})}
                        End If
                    ElseIf content.IsQuotedBlock OrElse content.IsCodeBlock Then
                        For Each p In Paragraph2Paragraphs(content)
                            Yield p
                        Next
                    End If
                Next

            ElseIf md.IsCodeBlock Then
                Dim mdc = CType(md, MarkdownParagraph.CodeBlock), code = mdc.Item1, lang = mdc.Item2, ignoredAfterLang = mdc.Item3
                Dim lines = code.Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.None).ToList()
                If String.IsNullOrWhiteSpace(lines.Last) Then lines.RemoveAt(lines.Count - 1)
                If String.IsNullOrWhiteSpace(lines.First) Then lines.RemoveAt(0)
                If lang = "antlr" Then
                    Dim run As Run = Nothing
                    For Each line In lines
                        If re_grammar.IsMatch(line) Then
                            If run IsNot Nothing Then Yield New Paragraph(run) With {.ParagraphProperties = New ParagraphProperties(New ParagraphStyleId With {.Val = "Grammar"})}
                            run = New Run(New Text(line) With {.Space = SpaceProcessingModeValues.Preserve})
                        Else
                            If run Is Nothing Then run = New Run()
                            run.Append(New Break)
                            run.Append(New Text(line) With {.Space = SpaceProcessingModeValues.Preserve})
                        End If
                    Next
                    If run IsNot Nothing Then Yield New Paragraph(run) With {.ParagraphProperties = New ParagraphProperties(New ParagraphStyleId With {.Val = "Grammar"})}
                    Return
                Else
                    Dim run As New Run, onFirstLine = True
                    For Each line In lines
                        If onFirstLine Then onFirstLine = False Else run.AppendChild(New Break)
                        run.AppendChild(New Text(line) With {.Space = SpaceProcessingModeValues.Preserve})
                    Next
                    Dim style As New ParagraphStyleId With {.Val = "Code"}
                    Yield New Paragraph(run) With {.ParagraphProperties = New ParagraphProperties(style)}
                    Return
                End If

            ElseIf md.IsQuotedBlock Then
                Dim mdq = CType(md, MarkdownParagraph.QuotedBlock), quoteds = mdq.Item
                Dim kind = ""
                For Each quoted In quoteds
                    If quoted.IsParagraph Then
                        Dim p = CType(quoted, MarkdownParagraph.Paragraph), spans = p.Item
                        If spans.FirstOrDefault?.IsStrong Then
                            Dim strong = CType(spans.First, MarkdownSpan.Strong).Item
                            If strong.FirstOrDefault?.IsLiteral Then
                                Dim literal = mdunescape(CType(strong.First, MarkdownSpan.Literal))
                                If literal = "Annotation" Then
                                    kind = "Annotation"
                                    Yield New Paragraph(Span2Elements(spans.Head)) With {.ParagraphProperties = New ParagraphProperties(New ParagraphStyleId With {.Val = kind})}
                                    If spans.Tail.FirstOrDefault?.IsLiteral Then
                                        Dim s = mdunescape(CType(spans.Tail.First, MarkdownSpan.Literal))
                                        quoted = MarkdownParagraph.NewParagraph(
                                            New Microsoft.FSharp.Collections.FSharpList(Of MarkdownSpan)(
                                                MarkdownSpan.NewLiteral(s.TrimStart()),
                                                spans.Tail.Tail))
                                    Else
                                        quoted = MarkdownParagraph.NewParagraph(spans.Tail)
                                    End If
                                ElseIf literal = "Note" Then
                                    kind = "AlertText"
                                ElseIf literal = "Comment" Then
                                    kind = "Comment"
                                End If
                            End If
                        End If
                        '
                        For Each qp In Paragraph2Paragraphs(quoted)
                            Dim qpp = TryCast(qp, Paragraph)
                            If qpp IsNot Nothing Then
                                Dim props As New ParagraphProperties(New ParagraphStyleId() With {.Val = kind})
                                qpp.ParagraphProperties = props
                            End If
                            If kind <> "Comment" Then Yield qp
                        Next

                    ElseIf quoted.IsCodeBlock Then
                        Dim mdc = CType(quoted, MarkdownParagraph.CodeBlock), code = mdc.Item1, lang = mdc.Item2, ignoredAfterLang = mdc.Item3
                        Dim lines = code.Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.None).ToList()
                        If String.IsNullOrWhiteSpace(lines.Last) Then lines.RemoveAt(lines.Count - 1)
                        Dim run As New Run() With {.RunProperties = New RunProperties(New RunStyle With {.Val = "CodeEmbedded"})}
                        For Each line In lines
                            If run.ChildElements.Count > 1 Then run.Append(New Break)
                            run.Append(New Text("    " & line) With {.Space = SpaceProcessingModeValues.Preserve})
                        Next
                        Yield New Paragraph(run) With {.ParagraphProperties = New ParagraphProperties(New ParagraphStyleId With {.Val = kind})}

                    ElseIf quoted.IsListBlock Then
                        If Not CType(quoted, MarkdownParagraph.ListBlock).Item1.IsOrdered Then Throw New NotImplementedException("unordered list inside annotation")
                        Dim count = 1
                        For Each qp In Paragraph2Paragraphs(quoted)
                            Dim qpp = TryCast(qp, Paragraph)
                            If qpp IsNot Nothing Then
                                qp.InsertAt(New Run(New Text($"{count}. ") With {.Space = SpaceProcessingModeValues.Preserve}), 0)
                                count += 1
                                Dim props As New ParagraphProperties(New ParagraphStyleId() With {.Val = kind})
                                qpp.ParagraphProperties = props
                            End If
                            Yield qp
                        Next

                    End If

                    '
                Next
                Return

            ElseIf md.IsTableBlock Then
                Dim mdt = CType(md, MarkdownParagraph.TableBlock), header = mdt.Item1.Option, align = mdt.Item2, rows = mdt.Item3
                Dim table As New Table()
                Dim tstyle As New TableStyle With {.Val = "TableGrid"}
                Dim borders As New TableBorders()
                borders.TopBorder = New TopBorder With {.Val = BorderValues.Single}
                borders.BottomBorder = New BottomBorder With {.Val = BorderValues.Single}
                borders.LeftBorder = New LeftBorder With {.Val = BorderValues.Single}
                borders.RightBorder = New RightBorder With {.Val = BorderValues.Single}
                borders.InsideHorizontalBorder = New InsideHorizontalBorder With {.Val = BorderValues.Single}
                borders.InsideVerticalBorder = New InsideVerticalBorder With {.Val = BorderValues.Single}
                table.Append(New TableProperties(tstyle, borders))
                Dim ncols = align.Length
                For irow = -1 To rows.Length - 1
                    If irow = -1 And header Is Nothing Then Continue For
                    Dim mdrow = If(irow = -1, header, rows(irow))
                    Dim row As New TableRow
                    For icol = 0 To System.Math.Min(ncols, mdrow.Length) - 1
                        Dim mdcell = mdrow(icol)
                        Dim cell As New TableCell
                        Dim pars = Paragraphs2Paragraphs(mdcell).ToList()
                        For ip = 0 To pars.Count - 1
                            Dim p = TryCast(pars(ip), Paragraph)
                            If p IsNot Nothing Then
                                Dim props As New ParagraphProperties
                                If align(icol).IsAlignCenter Then props.Append(New Justification With {.Val = JustificationValues.Center})
                                If align(icol).IsAlignRight Then props.Append(New Justification With {.Val = JustificationValues.Right})
                                Dim spacing As SpacingBetweenLines = Nothing
                                If ip = pars.Count - 1 Then props.Append(New SpacingBetweenLines With {.After = "0"})
                                If props.HasChildren Then p.InsertAt(props, 0)
                            End If
                            cell.Append(pars(ip))
                        Next
                        If pars.Count = 0 Then cell.Append(New Paragraph(New ParagraphProperties(New SpacingBetweenLines With {.After = "0"}), New Run(New Text(""))))
                        row.Append(cell)
                    Next
                    table.Append(row)
                Next
                Yield table
                Return

            Else
                Yield New Paragraph(New Run(New Text($"[{md.GetType.Name}]")))
                Return
            End If
        End Function

        Class FlatItem
            Public Level As Integer
            Public HasBullet As Boolean
            Public IsBulletOrdered As Boolean
            Public Paragraph As MarkdownParagraph
        End Class

        Function FlattenList(md As MarkdownParagraph.ListBlock) As IEnumerable(Of FlatItem)
            Dim flat = FlattenList(md, 0).ToList()
            Dim isOrdered As New Dictionary(Of Integer, Boolean)
            For Each item In flat
                Dim level = item.Level, isItemOrdered = item.IsBulletOrdered, content = item.Paragraph
                If isOrdered.ContainsKey(level) AndAlso isOrdered(level) <> isItemOrdered Then Throw New NotImplementedException("List can't mix ordered and unordered items at same level")
                If Not isOrdered.ContainsKey(level) Then isOrdered(level) = isItemOrdered
                If Not content.IsParagraph AndAlso Not content.IsSpan AndAlso Not content.IsQuotedBlock AndAlso Not content.IsCodeBlock Then Throw New NotImplementedException("List can only have text, quoted-blocks and code-blocks")
                If level > 2 Then Throw New Exception("Can't have more than 3 levels in a list")
            Next
            Return flat
        End Function

        Iterator Function FlattenList(md As MarkdownParagraph.ListBlock, level As Integer) As IEnumerable(Of FlatItem)
            Dim isOrdered = md.Item1.IsOrdered, items = md.Item2
            For Each mdpars In items
                Dim isFirstParagraph = True
                For Each mdp In mdpars
                    If mdp.IsParagraph OrElse mdp.IsSpan Then
                        Yield New FlatItem With {.Level = level, .HasBullet = isFirstParagraph, .IsBulletOrdered = isOrdered, .Paragraph = mdp}
                        isFirstParagraph = False
                    ElseIf mdp.IsQuotedBlock OrElse mdp.IsCodeBlock Then
                        Yield New FlatItem With {.Level = level, .HasBullet = False, .IsBulletOrdered = isOrdered, .Paragraph = mdp}
                        isFirstParagraph = False
                    ElseIf mdp.IsListBlock Then
                        For Each subitem In FlattenList(CType(mdp, MarkdownParagraph.ListBlock), level + 1)
                            Yield subitem
                        Next
                        isFirstParagraph = False
                    Else
                        Throw New NotImplementedException("Nothing fancy allowed in lists")
                    End If
                Next
            Next
        End Function


        Iterator Function Spans2Elements(mds As IEnumerable(Of MarkdownSpan)) As IEnumerable(Of OpenXmlElement)
            For Each md In mds
                For Each e In Span2Elements(md)
                    Yield e
                Next
            Next
        End Function

        Iterator Function Span2Elements(md As MarkdownSpan) As IEnumerable(Of OpenXmlElement)
            If md.IsLiteral Then
                Dim mdl = CType(md, MarkdownSpan.Literal), s = mdunescape(mdl)
                Yield New Run(New Text(s) With {.Space = SpaceProcessingModeValues.Preserve})

            ElseIf md.IsStrong OrElse md.IsEmphasis Then
                Dim spans = If(md.IsStrong, CType(md, MarkdownSpan.Strong).Item, CType(md, MarkdownSpan.Emphasis).Item)
                For Each e In Spans2Elements(spans)
                    Dim style = If(md.IsStrong, CType(New Bold, OpenXmlElement), New Italic)
                    Dim run = CType(e, Run)
                    If run IsNot Nothing Then run.InsertAt(New RunProperties(style), 0)
                    Yield e
                Next
                Return

            ElseIf md.IsInlineCode Then
                Dim mdi = CType(md, MarkdownSpan.InlineCode), code = mdi.Item

                Dim txt As New Text(code) With {.Space = SpaceProcessingModeValues.Preserve}
                Dim props As New RunProperties(New RunStyle With {.Val = "CodeEmbedded"})
                Dim run As New Run(txt) With {.RunProperties = props}
                Yield run
                Return

            ElseIf md.IsDirectLink Or md.IsIndirectLink Then
                Dim spans As IEnumerable(Of MarkdownSpan), url = "", alt = ""
                If md.IsDirectLink Then
                    Dim mddl = CType(md, MarkdownSpan.DirectLink)
                    spans = mddl.Item1
                    url = mddl.Item2.Item1
                    alt = mddl.Item2.Item2.Option()
                Else
                    Dim mdil = CType(md, MarkdownSpan.IndirectLink), original = mdil.Item2, id = mdil.Item3
                    spans = mdil.Item1
                    If mddoc.DefinedLinks.ContainsKey(id) Then
                        url = mddoc.DefinedLinks(id).Item1
                        alt = mddoc.DefinedLinks(id).Item2.Option()
                    End If
                End If

                Dim anchor = ""
                If spans.Count = 1 AndAlso spans.First.IsLiteral Then
                    anchor = mdunescape(CType(spans.First, MarkdownSpan.Literal))
                ElseIf spans.Count = 1 AndAlso spans.First.IsInlineCode Then
                    anchor = CType(spans.First, MarkdownSpan.InlineCode).Item
                Else
                    Throw New NotImplementedException("Link anchor must be Literal or InlineCode, not " & md.ToString())
                End If

                If sections.ContainsKey(url) Then
                    Dim section = sections(url)
                    If anchor <> section.Title Then Throw New Exception($"Mismatch: link anchor is '{anchor}', should be '{section.Title}'")
                    Dim txt As New Text(section.Number) With {.Space = SpaceProcessingModeValues.Preserve}
                    Dim run As New Hyperlink(New Run(txt)) With {.Anchor = section.BookmarkName}
                    Yield run

                Else
                    If Not url.StartsWith("http:") AndAlso Not url.StartsWith("https:") Then Throw New Exception("Absent hyperlink in " & md.ToString())
                    Dim style = New RunStyle With {.Val = "Hyperlink"}
                    Dim hyperlink As New Hyperlink With {.DocLocation = url, .Tooltip = alt}
                    For Each element In Spans2Elements(spans)
                        Dim run = TryCast(element, Run)
                        If run IsNot Nothing Then run.InsertAt(New RunProperties(style), 0)
                        hyperlink.AppendChild(run)
                    Next
                    Yield hyperlink
                End If
                Return

            ElseIf md.IsHardLineBreak
                ' I've only ever seen this arise from dodgy markdown parsing, so I'll ignore it...
                Return

            Else
                Yield New Run(New Text($"[{md.GetType.Name}]"))
                Return
            End If
        End Function


    End Class

End Class


Module ExtensionMethods
    <Extension>
    Public Function [Option](Of T As Class)(this As FSharpOption(Of T)) As T
        If FSharpOption(Of T).GetTag(this) = FSharpOption(Of T).Tags.None Then Return Nothing
        Return this.Value
    End Function

End Module
