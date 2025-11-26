
namespace DocProcessor;

using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2010.Word;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using Checked = DocumentFormat.OpenXml.Wordprocessing.Checked;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

public enum DocumentType
{
    NewDocument,
    ExistingDocument,
    Template
}

public class Document: IDisposable
{
    
    private WordprocessingDocument Doc { get; set; }
    
    private MainDocumentPart MainPart { get; set; }
    
    private Body Body { get; set; }
    
    private string SavePath { get; }
    
    private string TempPath { get; }

    private string DirPath { get; set; }
    
    private uint AltChunkCount { get; set; }

    private Regex Matcher { get; init; }
    
    public Document(string path, DocumentType type)
    {
        
        SavePath = path;

        if (SavePath.EndsWith(".docx"))
        {
            TempPath = SavePath.Replace(".docx", "_temp.docx");
        } else if (SavePath.EndsWith(".dotx"))
        {
            TempPath = SavePath.Replace(".dotx", "_temp.docx");
        }
        else
        {
            throw new ArgumentException("Invalid document path. Only docx and dotx are supported.");
        }

        Matcher = new Regex(
            @"<<(?<tagtype>if|/if|image|doc|logic||) *\[(?<operand>[ \w\[\]\\/._-]{3,})\](?<flags>[ \w\[\]\\/.:_-]*)>>|<<(?<tagtype>/if)>>",
            RegexOptions.Compiled);
        
        DirPath = Path.GetDirectoryName(SavePath);
        AltChunkCount = 0; 
        CreateTempCopyOfDocument(SavePath, TempPath);
        if (type == DocumentType.ExistingDocument || type == DocumentType.Template)
        {
            OpenExistingDocument(TempPath);
        }
        else 
        {
            Doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
            CreateDocument();
        }

    }
    
    private void OpenExistingDocument(string path)
    {
        Doc = WordprocessingDocument.Open(path, true);
        MainPart = Doc.MainDocumentPart ?? Doc.AddMainDocumentPart();
        Body = MainPart.Document.Body ?? MainPart.Document.AppendChild(new Body());
    }
    
    private void CreateDocument()
    {
        MainPart = Doc.AddMainDocumentPart();
        MainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
        Body = MainPart.Document.AppendChild(new Body());
    }

    private static void CreateTempCopyOfDocument(string docPath, string tempPath)
    {
        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
        }
        
        File.Copy(docPath, tempPath);
    }
    
    
    public void InsertText(string newText)
    {
        Paragraph paragraph = Body.AppendChild(new Paragraph());
        Run run = paragraph.AppendChild(new Run());
        run.AppendChild(new Text(newText));
    }

    //for testing
    public Paragraph? GetParagraph(string str)
    {
        foreach (Paragraph para in Body.Descendants<Paragraph>())
        {
            if (para.InnerText.Contains(str))
            {
                return para;
            }
        }

        return null;
    }


    // TODO: fix this, need to isolate the pattern first
    // TODO: IMPLEMENT
    private void SearchAndReplace(string pattern, Func<string, string>? getReplacementString, string? replacementStr, bool isRegex)
    {
        
        Regex matcher = new Regex(pattern);
                
        foreach (Paragraph para in Body!.Descendants<Paragraph>())
        {
            
            if (!matcher.IsMatch(para.InnerText))
            {
                continue;
            }

            if (para.Descendants<Text>().Count() > 1)
            {
                //IsolatePatternInParagraph(para);
            }
            
            foreach (Text text in para.Descendants<Text>())
            {

                if (isRegex)
                {
                    foreach (Match match in matcher.Matches(text.Text))
                    {
                        string key = match.Groups[1].Value;
                        text.Text = text.Text.Replace(match.Value, getReplacementString!(key));
                    }
                }
                else
                {
                    text.Text = text.Text.Replace(pattern, replacementStr);
                }
                
                text.Space = SpaceProcessingModeValues.Preserve;
                
            }

        } 
        
    }
    
    
    // TODO: fix this, call process document somewhere else
    // TODO: IMPLEMENT
    //public void ReplaceTextWithDocument(string text, Document doc, Func<string, string>? getReplacementString, JObject? data, IDocumentLogic logic)
    //{

    //    string altChunkId = $"AltChunkId{++AltChunkCount}";
    //    AlternativeFormatImportPart chunk = MainPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.WordprocessingML, altChunkId);
    //    
    //    if (getReplacementString != null && data != null)
    //    {
    //        doc.ProcessDocument(getReplacementString, data, logic);
    //        doc.Save();
    //        using (FileStream fileSteam = File.Open(doc.TempPath, FileMode.Open))
    //        {
    //            chunk.FeedData(fileSteam);
    //        }
    //    }
    //    else
    //    {
    //        using (FileStream fileSteam = File.Open(doc.SavePath, FileMode.Open))
    //        {
    //            chunk.FeedData(fileSteam);
    //        }
    //    }

    //    AltChunk altChunk = new AltChunk();
    //    altChunk.Id = altChunkId;

    //    /*
    //    MainPart.Document.Body.InsertAfter(altChunk,
    //        MainPart.Document.Body.Elements<Paragraph>().Last());
    //        */

    //    Paragraph? para = Body.Descendants<Paragraph>().FirstOrDefault(p => p.InnerText.Contains(text));

    //    if (para == null) return; //text doesn't exist in doc
    //    
    //    //if (para.Descendants<Text>().Count() > 1)
    //    //{
    //        //IsolatePatternInParagraph(para, text);
    //    //}

    //    //Text? t = para.Descendants<Text>().FirstOrDefault(t => t.Text.Contains(text));

    //    //if (t == null) return;

    //    para.InsertAfterSelf(altChunk);
    //    
    //    // Remove a paragraph if the tag was the only text it in
    //    // This doesn't account for scenarios where a paragraph has intentional styling but no text.
    //    if (para.InnerText == text) para.Remove();
    //    
    //    // TODO
    //    // Right now, the AltChunk gets inserted into the document,
    //    // and that's it. If you open it in word, the resulting document has the Altchunk 
    //    // until it gets saved to a new file. That's when the changes get 'merged'.
    //    // Need to make that 'merging' happen here.
    //    // This will also resolve the issue of Libreoffice treating the file as corrupt.
    //    
    //    //t.Remove();

    //}

    // TODO: IMPLEMENT
    public Drawing GetImage(Image image)
    {
        
        ImagePart imagePart = MainPart!.AddImagePart(ImagePartType.Png); //static png for now
        
        using (FileStream stream = new FileStream(image.File, FileMode.Open))
        {
            imagePart.FeedData(stream);
        }

        return GetImageElement(image, MainPart!.GetIdOfPart(imagePart));
        
    }
    
    // TODO: fix this, isolate tags first
    // TODO: IMPLEMENT
    //public void ReplaceTextWithImage(string text, Image image)
    //{

    //    ImagePart imagePart = MainPart!.AddImagePart(ImagePartType.Png); //static png for now
    //    
    //    using (FileStream stream = new FileStream(image.File, FileMode.Open))
    //    {
    //        imagePart.FeedData(stream);
    //    }

    //    Drawing drawing = GetImageElement(image, MainPart!.GetIdOfPart(imagePart));
    //    
    //    // Text text = Body!.Descendants<Text>().Where(t => t.InnerText)
    //    Paragraph? para = Body.Descendants<Paragraph>().FirstOrDefault(p => p.InnerText.Contains(text));

    //    if (para == null) return; //text doesn't exist in doc
    //    
    //    if (para.Descendants<Text>().Count() > 1)
    //    {
    //        IsolatePatternInParagraph(para);
    //    }

    //    Text? t = para.Descendants<Text>().FirstOrDefault(t => t.Text.Contains(text));

    //    if (t == null) return;
    //    
    //    t.InsertAfterSelf(drawing);
    //    t.Remove();

    //}

    // TODO: IMPLEMENT
    private Drawing GetImageElement(Image image, string relationshipId)
    {
        
         return new Drawing(
             new DW.Inline(
                 new DW.Extent() { Cx = image.Width, Cy = image.Height },
                 new DW.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, 
                     RightEdge = 0L, BottomEdge = 0L },
                 new DW.DocProperties() { Id = (UInt32Value)1U, 
                     Name = "Picture 1" },
                 new DW.NonVisualGraphicFrameDrawingProperties(
                     new A.GraphicFrameLocks() { NoChangeAspect = true }),
                 new A.Graphic(
                     new A.GraphicData(
                         new PIC.Picture(
                             new PIC.NonVisualPictureProperties(
                                 new PIC.NonVisualDrawingProperties() 
                                    { Id = (UInt32Value)0U, 
                                        Name = "New Bitmap Image.jpg" },
                                 new PIC.NonVisualPictureDrawingProperties()),
                             new PIC.BlipFill(
                                 new A.Blip(
                                     new A.BlipExtensionList(
                                         new A.BlipExtension() 
                                            { Uri = 
                                                "{28A0092B-C50C-407E-A947-70E740481C1C}" })
                                 ) 
                                 { Embed = relationshipId, 
                                     CompressionState = 
                                     A.BlipCompressionValues.Print },
                                 new A.Stretch(
                                     new A.FillRectangle())),
                             new PIC.ShapeProperties(
                                 new A.Transform2D(
                                     new A.Offset() { X = 0L, Y = 0L },
                                     new A.Extents() { Cx = image.Width, Cy = image.Height }),
                                 new A.PresetGeometry(
                                     new A.AdjustValueList()
                                 ) { Preset = A.ShapeTypeValues.Rectangle }))
                     ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
             ) { DistanceFromTop = (UInt32Value)0U, 
                 DistanceFromBottom = (UInt32Value)0U, 
                 DistanceFromLeft = (UInt32Value)0U, 
                 DistanceFromRight = (UInt32Value)0U, EditId = "50D07946" });
         
    }

    // TODO: IMPLEMENT
    public void SearchAndReplaceText(string textToReplace, string replaceWith) 
    {
        SearchAndReplace(textToReplace, null, replaceWith, false);
    }
    
    // TODO: IMPLEMENT
    public void SearchAndReplaceTextByRegex(string pattern, Func<string, string> getReplacementString)
    {
        SearchAndReplace(pattern, getReplacementString, null, true); //regex replace
    }

    // TODO: rewrite this
    // A paragraph has a number of run elements which each can have a number
    // of text elements. This function takes every 'pattern' match in the paragraph,
    // and ensures that it is isolated into its own Run element.
    // Paragraph > Run > Text is the tag structure.
    //private void IsolatePatternInParagraph(Paragraph para)
    //{

    //    List<Text> textElements = para.Descendants<Text>().ToList();

    //    List<string> textTexts = new List<string>();

    //    foreach (Text text in textElements)
    //    {
    //        textTexts.Add(text.Text);
    //    }

    //    MatchCollection matches = Matcher.Matches(para.InnerText);

    //    for (int i = 0; i < matches.Count; i++)
    //    {

    //        Match match = matches.ElementAt(i);

    //        MatchIndices mi = FindIndicesInMatch(match, textTexts);
    //        
    //        #region CreateRunWithMatch
    //        
    //        Run matchStartsInRun = (Run)textElements.ElementAt(mi.ElementIndex).Parent;
    //        Run run = new Run();
    //        RunProperties propertiesToMatch = matchStartsInRun.RunProperties;
    //        if (propertiesToMatch != null)
    //        {
    //            run.AppendChild((RunProperties)propertiesToMatch.CloneNode(true));
    //        }
    //        run.AppendChild(new Text(match.Value));
    //        
    //        #endregion

    //        #region RemoveMatchFromOriginalParapgraph
    //        
    //            // int[0]: the index in the string that match starts at
    //            // int[1]: the index in the string that match ends at
    //            // int[2]: the index of the text element that match starts at
    //            // int[3]: the index of the text element that match ends at
    //
    //        // if the match starts and ends in the same text element
    //        if(mi.ElementIndex == mi.ElementEndIndex)
    //        {
    //            
    //            Text text = textElements.ElementAt(mi.ElementIndex);
    //            
    //            // 1) match starts at the start of the string
    //            //  - remove the match and place the run before the current run
    //            if (mi.StringIndex == 0)
    //            {
    //                
    //                // remove the match from the text element
    //                text.Text = text.Text.Remove(mi.StringIndex, match.Length);

    //                matchStartsInRun.InsertBeforeSelf(run);

    //                textElements = para.Descendants<Text>().ToList();
    //                textTexts.Clear();
    //                textElements.ForEach(t => textTexts.Add(t.Text));
    //                continue;

    //            } 
    //            // 2) match ends at the end of the string
    //            //  - remove the match
    //            //  - Insert the run after the current run
    //            else if (mi.StringEndIndex == text.Text.Length - 1)
    //            {
    //                
    //                // remove the match from the text element
    //                text.Text = text.Text.Remove(mi.StringIndex, match.Length);

    //                matchStartsInRun.InsertAfterSelf(run);
    //                
    //                textElements = para.Descendants<Text>().ToList();
    //                textTexts.Clear();
    //                textElements.ForEach(t => textTexts.Add(t.Text));
    //                continue;
    //                
    //            }
    //            // 3) match is in between other text
    //            //  - Save the string that comes after the match.
    //            //  - Remove all the content except for the text before the match.
    //            //  - Insert the run after the current run
    //            //  - Insert a new run with the saved string, after the inserted run.
    //            else
    //            {
    //               
    //                string afterMatchText = text.Text.Substring(mi.StringEndIndex + 1);
    //                
    //                // remove the match and the afterMatchText from the text element
    //                text.Text = text.Text.Remove(mi.StringIndex);
    //                
    //                matchStartsInRun.InsertAfterSelf(run);
    //                
    //                Run runTemp = new Run();
    //                if (propertiesToMatch != null)
    //                {
    //                    runTemp.AppendChild((RunProperties)propertiesToMatch.CloneNode(true));
    //                }
    //                runTemp.AppendChild(new Text(afterMatchText));

    //                run.InsertAfterSelf(runTemp); 
    //                
    //                textElements = para.Descendants<Text>().ToList();
    //                textTexts.Clear();
    //                textElements.ForEach(t => textTexts.Add(t.Text));
    //                continue;
    //                
    //            }
    //        }

    //        // loop through each relevant text element
    //        for (int j = mi.ElementIndex; j <= mi.ElementEndIndex; j++)
    //        {

    //            Text text = textElements.ElementAt(j);
    //            
    //            // SCENARIOS:
    //            
    //            // IN STARTING TEXT ELEMENT
    //            // IN ENDING TEXT ELEMENT
    //            // IN A TEXT ELEMENT WHERE THE ENTIRE TEXT IS JUST TAG

    //            // in starting element
    //            if (j == mi.ElementIndex)
    //            {
    //                text.Text = text.Text.Remove(mi.StringIndex);
    //                textElements = para.Descendants<Text>().ToList();
    //                textTexts.Clear();
    //                textElements.ForEach(t => textTexts.Add(t.Text));
    //                continue;
    //            }
    //            
    //            // in ending element
    //            if (j == mi.ElementEndIndex)
    //            {
    //                text.Text = text.Text.Remove(0, mi.StringEndIndex + 1);
    //                textElements = para.Descendants<Text>().ToList();
    //                textTexts.Clear();
    //                textElements.ForEach(t => textTexts.Add(t.Text));
    //                continue;
    //            }

    //            //if the text contained only the tag and absolutely nothing else
    //            text.Remove();
    //                
    //        }
    //        
    //        //TODO: AFTER MODIFING THE PARAGRAPH, RESET THE TEXTS LISTS
    //        
    //        #endregion
    //        
    //        #region InsertNewRunInParagraph

    //        matchStartsInRun.InsertAfterSelf(run);
    //       
    //        //This is a temporary solution, should really be updating the existing lists
    //        textElements = para.Descendants<Text>().ToList();
    //        textTexts.Clear();
    //        textElements.ForEach(t => textTexts.Add(t.Text));
    //        //indices.Clear();
    //        //indices = IndexPositionsInStrList(textTexts);

    //        #endregion

    //    } 

    //}


    /*
    //NOT COMPLETE
    // TODO: IMPLEMENT
    public void ReplaceTextWithCheckbox(string Text, Checkbox checkbox)
    {

        SdtBlock sdt = new();
        SdtProperties properties = new();
        SdtContentCheckBox cb = new();

        properties.AddChild(new Tag { Val = checkbox.Tag });
        cb.Checked = new DocumentFormat.OpenXml.Office2010.Word.Checked
        {
            Val = checkbox.State ? OnOffValues.True : OnOffValues.False
        };
        
        Paragraph? para = Body.Descendants<Paragraph>().FirstOrDefault(p => p.InnerText.Contains(Text));

        if (para == null) return; //text doesn't exist in doc
        
        if (para.Descendants<Text>().Count() > 1)
        {
            IsolatePatternInParagraph(para, Text);
        }

        Text? t = para.Descendants<Text>().FirstOrDefault(t => t.Text.Contains(Text));

        if (t == null) return;

        sdt.AddChild(properties);
        sdt.AddChild(cb);
        
        t.InsertAfterSelf();
        t.Remove();
    }
    */
    
    // TODO: IMPLEMENT
    public void EditLegacyCheckbox(string tag, bool newState)
    {
        foreach (CheckBox cb in Body!.Descendants<CheckBox>())
        {
            FormFieldName cbName = cb.Parent.ChildElements.First<FormFieldName>();
            if (cbName.Val.Value == tag)
            {
                
                Checked state = cb.GetFirstChild<Checked>();
                
                if (state == null)
                {
                    state = new Checked();
                    cb.AddChild(state);
                }
                
                state.Val = new OnOffValue(newState);
                
            }
        }
    }

    // TODO: IMPLEMENT
    public void EditCheckbox(string tag, bool newState)
    {
        foreach (SdtContentCheckBox cb in Body!.Descendants<SdtContentCheckBox>())
        {

            SdtProperties properties = (SdtProperties)cb.Parent;
            SdtRun parent = (SdtRun)properties.Parent;
            Tag? checkboxTag = properties.Descendants<Tag>().FirstOrDefault();

            if (checkboxTag != null && checkboxTag.Val == tag) //found correct checkbox
            {
                
                cb.Checked!.Val = newState ? OnOffValues.True : OnOffValues.False;
                
                SdtContentRun content = parent.Descendants<SdtContentRun>().FirstOrDefault();
                
                if (content != null)
                {
                    Text text = content.Descendants<Text>().FirstOrDefault();
                    if (text != null)
                    {
                        int unicodeChar = int.Parse(cb.CheckedState.Val.Value, System.Globalization.NumberStyles.HexNumber);
                        text.Text = ((char)unicodeChar).ToString();
                    }
                }
            }
        }
    }

    public void Save()
    {
        Doc.Dispose();
    }

    public void SaveAs(string path)
    {
        Doc.Dispose();
        File.Copy(TempPath, path);
        File.Delete(TempPath);
        
        //so a user can continue modifying the same document
        CreateTempCopyOfDocument(path, TempPath);
        OpenExistingDocument(TempPath);
    }
    
    public void SaveAsStream(Stream stream)
    {
        Doc.Dispose();
        byte[] bytes = File.ReadAllBytes(TempPath);
        stream.Write(bytes, 0, (int)bytes.Length);
        OpenExistingDocument(TempPath);
        // File.Delete(TempPath);
    }

    //NOTE: right now, if Dispose() is not called,
    //the file at TempPath will still be there after the doc is generated.
    public void Dispose()
    {
        Doc.Dispose();
        File.Delete(TempPath);
    }
    
    
}