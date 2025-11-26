using System.Text.RegularExpressions;
using docProcessor.Utility;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace docProcessor;

internal class MatchIsolator
{
    private readonly Body _body;
    private readonly Regex _tagPattern;
    
    public MatchIsolator(Body body, Regex matchPattern)
    {
        _body = body;
        _tagPattern = matchPattern;
    }
    
    public void IsolateAllTags()
    {
        // 1. Flatten
        var flattened = DocumentFlattener.Flatten(_body);
        
        // 2. Find all tag matches
        MatchCollection matches = _tagPattern.Matches(flattened.Text);
        
        if (matches.Count == 0)
            return;
        
        // 3. Process right-to-left
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            Match match = matches[i];
            IsolateTag(match, flattened.PositionMap);
        }
        
        // 4. Cleanup
        CleanupEmptyNodes();
    }
    
    private void IsolateTag(Match match, List<PositionInfo> positionMap)
    {
        // Find the nodes which will be affected by the isolation
        var affectedNodes = GetAffectedNodes(match, positionMap);
        
        // Create new run with tag text
        Run newRun = CreateIsolatedRun(match, affectedNodes);
        
        // Remove tag text from original nodes
        RemoveTagFromOriginalNodes(match, affectedNodes, positionMap);
        
        // Insert new run in correct location
        InsertRun(newRun, affectedNodes);
    }
    
    private AffectedNodes GetAffectedNodes(Match match, List<PositionInfo> positionMap)
    {
        HashSet<Text> texts = new();
        HashSet<Run> runs = new();
        HashSet<Paragraph> paragraphs = new();
        
        // Record every node which is 'linked' to the match
        for (int pos = match.Index; pos <= match.EndIndex(); pos++)
        {
            PositionInfo info = positionMap[pos];
            texts.Add(info.TextNode);
            runs.Add(info.RunNode);
            paragraphs.Add(info.ParagraphNode);
        }
        
        PositionInfo firstInfo = positionMap[match.Index];
        
        return new AffectedNodes(
            AllTexts: texts.ToList(),
            //AllRuns: runs.ToList(),
            //AllParagraphs: paragraphs.ToList(),
            FirstRun: firstInfo.RunNode
        );
    }
    
    private Run CreateIsolatedRun(Match match, AffectedNodes affected)
    {
        
        Run newRun = new Run();
        
        newRun.SetAttribute(new OpenXmlAttribute());
        
        // Clone formatting from first run
        if (affected.FirstRun.RunProperties != null)
        {
            newRun.RunProperties = 
                (RunProperties)affected.FirstRun.RunProperties.CloneNode(true);
        }
        
        // Mark run with custom attribute
        // Note: could use this to store any variable on the run; like tag type
        newRun.SetAttribute(new OpenXmlAttribute(
            "tmplTag", "isTag", "jstg", "true"));
        
        //Note: to find a tag with the 'isTag' Attribute, do
        //var taggedRuns = _body.Descendants<Run>()
        //    .Where(r => r.GetAttribute("isTag", "jstg")?.Value == "true")
        //    .ToList();
        
        // Add tag text
        newRun.AppendChild(new Text(match.Value));
        
        return newRun;
    }
    
    private void RemoveTagFromOriginalNodes(
        Match match, 
        AffectedNodes affected, 
        List<PositionInfo> positionMap)
    {
        // For each affected text node, calculate what portion to remove
        foreach (Text text in affected.AllTexts)
        {
            // Find this text's position range within the entire flattened string
            int textStartPos = FindFirstPosition(text, positionMap);
            int textEndPos = textStartPos + text.Text.Length - 1;
            
            // Calculate overlap with match
            int overlapStart = Math.Max(match.Index, textStartPos);
            int overlapEnd = Math.Min(match.Index + match.Length - 1, textEndPos);
            
            // Convert to local coordinates (coordinates if string was just the current text)
            int localStart = overlapStart - textStartPos;
            int localEnd = overlapEnd - textStartPos;
            
            // Modify the text
            ModifyTextNode(text, localStart, localEnd);
        }
    }
    
    // localStart: where the match starts in the text node
    // localEnd: where the match ends in the text node
    // This function removes the match from the text
    private void ModifyTextNode(Text text, int localStart, int localEnd)
    {
        int textLength = text.Text.Length;
        
        // if the match spans the entire text
        if (localStart == 0 && localEnd == textLength - 1)
        {
            text.Remove();
        }
        
        // if the match starts at the beginning of the text, but doesn't reach the end
        else if (localStart == 0)
        {
            // Remove from start, keep suffix
            text.Text = text.Text.Substring(localEnd + 1);
        }
        
        // if the match doesn't start at the beginning of the text, but does reach the end
        else if (localEnd == textLength - 1)
        {
            // Remove from end, keep prefix
            text.Text = text.Text.Substring(0, localStart);
        }
        
        // the match doesn't start at the beginning and doesn't reach the end
        else
        {
            // Remove from middle - need to split
            string prefix = text.Text.Substring(0, localStart);
            string suffix = text.Text.Substring(localEnd + 1);
            
            // Keep prefix in current text
            text.Text = prefix;
            
            // Create new run for suffix
            Run? parentRun = (Run?)text.Parent;
            Run newRun = new Run();
            
            if (parentRun?.RunProperties != null)
            {
                newRun.RunProperties = 
                    (RunProperties)parentRun.RunProperties.CloneNode(true);
            }
            
            newRun.AppendChild(new Text(suffix));
            parentRun?.InsertAfterSelf(newRun);
        }
    }
    
    private void InsertRun(Run newRun, AffectedNodes affected)
    {
        // Insert after the first affected run
        affected.FirstRun.InsertAfterSelf(newRun);
    }
    
    private int FindFirstPosition(Text text, List<PositionInfo> positionMap)
    {
        for (int i = 0; i < positionMap.Count; i++)
        {
            if (positionMap[i].TextNode == text)
                return i;
        }
        return -1;
    }
    
    private void CleanupEmptyNodes()
    {
        // Remove empty Text nodes
        var emptyTexts = _body.Descendants<Text>()
            .Where(t => string.IsNullOrEmpty(t.Text))
            .ToList();
        
        foreach (var text in emptyTexts)
        {
            text.Remove();
        }
        
        // Remove empty Runs
        var emptyRuns = _body.Descendants<Run>()
            .Where(r => !r.Descendants<Text>().Any() || 
                        r.Descendants<Text>().All(t => string.IsNullOrEmpty(t.Text)))
            .ToList();
        
        foreach (var run in emptyRuns)
        {
            run.Remove();
        }
    }
}

record AffectedNodes(
    List<Text> AllTexts,
    //List<Run> AllRuns,
    //List<Paragraph> AllParagraphs,
    Run FirstRun
);
