using DocumentFormat.OpenXml.Wordprocessing;

namespace DocProcessor.TemplateEngine;

public class TemplateEngine
{
    
   private Body Body { get; set; }

   public TemplateEngine(Body body)
   {
       Body = body;
   }

   // TODO: isolate tags
   public void NormalizeDocument()
   {
       
   }

   // TODO: process the document
   public void Process()
   {
       
   }
   
}

    // TODO: completely rewrite this. Documents should be processed twice, and cached after the first process. 
    
    // Evaluates all tags matching the <<[]>> syntax inside the document.
    //public void ProcessDocument(Func<string, string>? getReplacementString, JObject data, IDocumentLogic logic)
    //{
    //    
    //    IEnumerable<Paragraph> paragraphs = Body!.Descendants<Paragraph>();

    //    Paragraph para;
    //    
    //    // loop through each paragraph to find if it has any matches.
    //    // loop backwards because you may have to modify the paragraphs. TODO ?
    //    for (int i = paragraphs.Count() - 1; i >= 0; i--)
    //    {
    //        para = paragraphs.ElementAt(i);
    //        
    //        // if there is no match, continue to next paragraph
    //        if(!Matcher.IsMatch(para.InnerText))
    //        {
    //            continue;
    //        }

    //        // matches are found in Text Elements, 
    //        // which are under the tag structure Paragraph > Run > Text.
    //        // They aren't necessary in a single text element,
    //        // so this function rearranges things so each
    //        // match is in its own Text Element (by itself).
    //        //if (para.Descendants<Text>().Count() > 1)
    //        //{
    //        //}
    //        //IsolatePatternInParagraph(para);
    //        
    //        // loop through each text element that has a potential match
    //        IEnumerable<Text> texts = para!.Descendants<Text>();
    //        Text text;
    //        //foreach (Text text in para.Descendants<Text>())
    //        for(int j = texts.Count() - 1; j >= 0; j--)
    //        {
    //            
    //            text = texts.ElementAt(j);

    //            foreach (Match match in Matcher.Matches(text.Text))
    //            {

    //                string tagType = match.Groups["tagtype"].Value;
    //                string operand = match.Groups["operand"].Value;
    //                string flags = match.Groups["flags"].Value;
    //                
    //                string replacement = getReplacementString!(operand);

    //                switch (tagType.Trim())
    //                {
    //                    
    //                    // TODO: WHAT IF THE TAGS ARE NOT IN THE SAME PARAGRAPH?
    //                   case "if":
    //                       
    //                       bool? result = logic.GetRule(operand);
    //                       int k = j + 1;

    //                       // keep the content in the if
    //                       // traverse until finding the </if>
    //                       if (result is true)
    //                       { 
    //                           Text nextText = texts.ElementAt(k);
    //                           while (nextText.Text != "<</if>>" && nextText.Text != $"<</if [{operand}]>>")
    //                           {
    //                               nextText = texts.ElementAt(++k);
    //                           }
    //                           nextText.Remove();
    //                       }
    //                       else
    //                       {
    //                           Text nextText = texts.ElementAt(k);
    //                           while (nextText.Text != "<</if>>" && nextText.Text != $"<</if [{operand}]>>")
    //                           {
    //                               nextText.Remove();
    //                               k = k - 1;
    //                               nextText = texts.ElementAt(++k);
    //                           }

    //                           nextText.Remove();
    //                       }

    //                       // remove the <<if [...]>>
    //                       text.Remove();
    //                       
    //                       continue;
    //                   
    //                   case "/if":
    //                       continue;
    //                   
    //                   case "image":
    //                       Drawing img = GetImage(new Image(replacement));
    //                       text.InsertAfterSelf(img);
    //                       text.Remove();
    //                       continue;
    //                   case "doc":
    //                       // relative path to parent doc
    //                       string docPath = operand;
    //                       string subDoc = $"{DirPath}/{docPath}";
    //                        
    //                       if (!File.Exists(subDoc))
    //                       {
    //                           text.Text = text.Text.Replace(match.Value, $"<<NULL: {docPath} DOES NOT EXIST>>");
    //                           continue;
    //                       }

    //                       Document toInsert = new Document(subDoc, DocumentType.ExistingDocument);
    //                       this.ReplaceTextWithDocument(match.Value, toInsert, getReplacementString, data, logic);
    //                       toInsert.Dispose();
    //                       continue;
    //                   
    //                   default: // regular tags
    //                       break;
    //                }

    //                DateOnly date;

    //                // this is a date, need to check formatting.
    //                // ex: << [key.date] :f YYYY-mm-dd >>
    //                if (DateOnly.TryParse(replacement, out date) && flags.Contains(":f"))
    //                {
    //                    List<string> switchStrings = flags.Split(' ').ToList();
    //                    int index = switchStrings.FindIndex(s => s.Contains(":f")) + 1;
    //                    string dateFormat = switchStrings[index].Replace('-', ' ');
    //                    text.Text = text.Text.Replace(match.Value, date.ToString(dateFormat));
    //                }
    //                
    //                // a pronoun
    //                // ex: << [key.gender] :p0 >>
    //                // ex: << [key.gender] :p0 :upper >>
    //                // p0, p1, p2, p3 are valid switches
    //                if (operand.Contains("gender"))
    //                {
    //                    
    //                    //temp, isolates the p# switches
    //                    flags = Regex.Replace(flags, @"\s+", "");
    //                    if(flags.Length > 0 && flags[0] == ':') {flags = flags.Substring(1);}
    //                    List<string> allSwitches = flags.Trim().Split(':').ToList(); 
    //                    allSwitches.RemoveAll(s => s.Length < 1);
    //                    
    //                    switch (allSwitches.FindLast(s => s[0] == 'p'))
    //                    {
    //                       case "p0":
    //                           if (replacement == "Male") { replacement = "mr"; }
    //                           else if (replacement == "Female") { replacement = "ms"; }
    //                           else { replacement = "mx"; }
    //                           break;
    //                       case "p1":
    //                           if (replacement == "Male") { replacement = "male"; }
    //                           else if (replacement == "Female") { replacement = "female"; }
    //                           else { replacement = "person"; }
    //                           break;
    //                       case "p2":
    //                           if (replacement == "Male") { replacement = "he"; }
    //                           else if (replacement == "Female") { replacement = "she"; }
    //                           else { replacement = "they"; }
    //                           break;
    //                       case "p3":
    //                           if (replacement == "Male") { replacement = "his"; }
    //                           else if (replacement == "Female") { replacement = "her"; }
    //                           else { replacement = "their"; }
    //                           break;
    //                       case "p4":
    //                           if (replacement == "Male") { replacement = "himself"; }
    //                           else if (replacement == "Female") { replacement = "herself"; }
    //                           else { replacement = "themself"; }
    //                           break;
    //                    }
    //                }
    //                
    //                if (flags.Contains(":upper"))
    //                {
    //                    text.Text = text.Text.Replace(match.Value,
    //                        Utility.ToUpperFirstChar(replacement));
    //                }
    //                else if (flags.Contains(":lower"))
    //                {
    //                    text.Text = text.Text.Replace(match.Value,
    //                        Utility.ToLowerFirstChar(replacement));
    //                }
    //                else
    //                {
    //                    text.Text = text.Text.Replace(match.Value, replacement);
    //                }

    //            }
    //            
    //            text.Space = SpaceProcessingModeValues.Preserve;
    //            
    //        }

    //    }
    //    
    //}
