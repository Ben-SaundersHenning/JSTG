using System.Web;
using Newtonsoft.Json.Linq;

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
    ExistingDocument
}

public class Document: IDisposable
{
    
    private WordprocessingDocument Doc { get; set; }
    
    private MainDocumentPart MainPart { get; set; }
    
    private Body Body { get; set; }
    
    private string SavePath { get; set; }
    private string TempPath { get; set; }
    
    private uint AltChunkCount { get; set; }
    
    
    public Document(string path, DocumentType type)
    {
        
        SavePath = path;
        TempPath = SavePath.Replace(".docx", "_temp.docx");
        AltChunkCount = 0; 
        CreateTempCopyOfDocument(SavePath, TempPath);
        if (type == DocumentType.ExistingDocument)
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

    // Evaluates all tags matching the <<[]>> syntax inside the document.
    public void ProcessDocument(Func<string, string>? getReplacementString, JObject data)
    {
        
        string regexp = @"<<((?:if|\/if||) {0,})\[([\w \[\]._-]{3,})\]([\w \[\].:_-]{0,})>>";
        Regex matcher = new Regex(regexp);
        
        foreach (Paragraph para in Body!.Descendants<Paragraph>())
        {
            
            if (!matcher.IsMatch(para.InnerText))
            {
                continue;
            }

            if (para.Descendants<Text>().Count() > 1)
            {
                IsolatePatternInParagraph(para, regexp);
            }
            
            foreach (Text text in para.Descendants<Text>())
            {

                foreach (Match match in matcher.Matches(text.Text))
                {
                    
                    string key = match.Groups[2].Value;
                    string replacement = getReplacementString!(key);
                    string[] keys = key.Split(' ');
                    string switches = match.Groups[3].Value;

                    /*
                    // <<if [a == b]>>
                    if (match.Groups[1].Value.Contains("if") && keys.Length == 3)
                    {
                        switch (keys[1])
                        {
                            case "==":
                                if obj[a] == obj[b]
                                {
                                    // keep if content
                                }
                                break;
                            case "!=":
                                if obj[a] != obj[b]
                                {
                                    // remove if content
                                }
                                break;
                        } 

                    }
                    else
                    { */
                    
                    DateOnly date;

                    // this is a date, need to check formatting.
                    // ex: << [key.date] :f YYYY-mm-dd >>
                    if (DateOnly.TryParse(replacement, out date) && switches.Contains(":f"))
                    {
                        List<string> switchStrings = switches.Split(' ').ToList();
                        int index = switchStrings.FindIndex(s => s.Contains(":f")) + 1;
                        string dateFormat = switchStrings[index].Replace('-', ' ');
                        text.Text = text.Text.Replace(match.Value, date.ToString(dateFormat));
                    }
                    
                    if (switches.Contains(":upper"))
                    {
                        text.Text = text.Text.Replace(match.Value,
                            Utility.ToUpperFirstChar(replacement));
                    }
                    else if (switches.Contains(":lower"))
                    {
                        text.Text = text.Text.Replace(match.Value,
                            Utility.ToLowerFirstChar(replacement));
                    }
                    else
                    {
                        text.Text = text.Text.Replace(match.Value, replacement);
                    }

                }
                
                text.Space = SpaceProcessingModeValues.Preserve;
                
            }

        } 
        
    }

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
                IsolatePatternInParagraph(para, pattern);
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
    
    public void ReplaceTextWithDocument(string text, Document doc)
    {

        string altChunkId = $"AltChunkId{++AltChunkCount}";
        AlternativeFormatImportPart chunk = MainPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.WordprocessingML, altChunkId);

        using (FileStream fileSteam = File.Open(doc.SavePath, FileMode.Open))
        {
            chunk.FeedData(fileSteam);
        }

        AltChunk altChunk = new AltChunk();
        altChunk.Id = altChunkId;

        /*
        MainPart.Document.Body.InsertAfter(altChunk,
            MainPart.Document.Body.Elements<Paragraph>().Last());
            */

        Paragraph? para = Body.Descendants<Paragraph>().FirstOrDefault(p => p.InnerText.Contains(text));

        if (para == null) return; //text doesn't exist in doc
        
        //if (para.Descendants<Text>().Count() > 1)
        //{
            //IsolatePatternInParagraph(para, text);
        //}

        //Text? t = para.Descendants<Text>().FirstOrDefault(t => t.Text.Contains(text));

        //if (t == null) return;

        para.InsertAfterSelf(altChunk);
        
        // Remove a paragraph if the tag was the only text it in
        // This doesn't account for scenarios where a paragraph has intentional styling but no text.
        if (para.InnerText == text) para.Remove();
        
        // TODO
        // Right now, the AltChunk gets inserted into the document,
        // and that's it. If you open it in word, the resulting document has the Altchunk 
        // until it gets saved to a new file. That's when the changes get 'merged'.
        // Need to make that 'merging' happen here.
        // This will also resolve the issue of Libreoffice treating the file as corrupt.
        
        //t.Remove();

    } 
    
    public void ReplaceTextWithImage(string text, Image image)
    {

        ImagePart imagePart = MainPart!.AddImagePart(ImagePartType.Png); //static png for now
        
        using (FileStream stream = new FileStream(image.File, FileMode.Open))
        {
            imagePart.FeedData(stream);
        }

        Drawing drawing = GetImageElement(image, MainPart!.GetIdOfPart(imagePart));
        
        // Text text = Body!.Descendants<Text>().Where(t => t.InnerText)
        Paragraph? para = Body.Descendants<Paragraph>().FirstOrDefault(p => p.InnerText.Contains(text));

        if (para == null) return; //text doesn't exist in doc
        
        if (para.Descendants<Text>().Count() > 1)
        {
            IsolatePatternInParagraph(para, text);
        }

        Text? t = para.Descendants<Text>().FirstOrDefault(t => t.Text.Contains(text));

        if (t == null) return;
        
        t.InsertAfterSelf(drawing);
        t.Remove();

    }

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

    public void SearchAndReplaceText(string textToReplace, string replaceWith) 
    {
        SearchAndReplace(textToReplace, null, replaceWith, false);
    }
    
    public void SearchAndReplaceTextByRegex(string pattern, Func<string, string> getReplacementString)
    {
        SearchAndReplace(pattern, getReplacementString, null, true); //regex replace
    }

    private void IsolatePatternInParagraph(Paragraph para, string pattern)
    {

        List<Text> textElements = para.Descendants<Text>().ToList();

        List<string> textTexts = new List<string>();

        foreach (Text text in textElements)
        {
            textTexts.Add(text.Text);
        }

        List<int> indices = IndexPositionsInStrList(textTexts);
        
        Regex matcher = new Regex(pattern);
        MatchCollection matches = matcher.Matches(para.InnerText);

        for (int i = 0; i < matches.Count; i++)
        {

            Match match = matches.ElementAt(i);

            int matchStartsInText = WhatPositionIsIndexIn(indices, match.Index);
            Run matchStartsInRun = (Run)textElements.ElementAt(matchStartsInText).Parent;
            int matchEndsInText = WhatPositionIsIndexIn(indices, match.Index + match.Value.Length - 1);

            if (matchStartsInText == matchEndsInText)
            { 
                continue; //match is over a single text element already.
            }

            #region CreateRunWithMatch
            
            Run run = new Run();
            RunProperties propertiesToMatch = matchStartsInRun.RunProperties;
            if (propertiesToMatch != null)
            {
                run.AppendChild((RunProperties)propertiesToMatch.CloneNode(true));
            }
            run.AppendChild(new Text(match.Value));
            
            #endregion

            #region RemoveMatchFromOriginalParapgraph

            int matchStartsAtIndex = match.Index - indices.ElementAt(matchStartsInText);
            int matchEndsAtIndex = match.Index + match.Length - 1 - indices.ElementAt(matchEndsInText);

            for (int j = matchStartsInText; j <= matchEndsInText; j++)
            {

                Text text = textElements.ElementAt(j);

                if (j == matchStartsInText)
                {
                    text.Text = text.Text.Remove(matchStartsAtIndex);
                    continue;
                }
                
                if (j == matchEndsInText)
                {
                    text.Text = text.Text.Remove(0, matchEndsAtIndex + 1);
                    continue;
                }

                //if the text contained only the tag and absolutely nothing else..
                text.Remove();
                    
            }
            
            #endregion
            
            #region InsertNewRunInParagraph

            matchStartsInRun.InsertAfterSelf(run);
           
            //This is a temporary solution, should really be updating the existing lists
            textElements = para.Descendants<Text>().ToList();
            textTexts.Clear();
            foreach (Text text in textElements)
            {
                textTexts.Add(text.Text);
            }
            indices.Clear();
            indices = IndexPositionsInStrList(textTexts);

            #endregion

        } 

    }

    /*
    //NOT COMPLETE
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

    private List<int> IndexPositionsInStrList(List<string> texts)
    {
        
        List<int> indices = new List<int> { 0 };

        for (int i = 0; i < texts.Count - 1; i++)
        {
            indices.Add(indices[i] + texts[i].Length);
        }
        
        return indices;
        
    }

    //This func assumes index IS in a valid index of indices.
    //It wont work properly if it isnt.
    private int WhatPositionIsIndexIn(List<int> indices, int index)
    {

        for (int i = 0; i < indices.Count; i++)
        {
            if (index < indices.ElementAt(i))
            {
                return i - 1;
            }
        }
        
        return indices.Count - 1;
        
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