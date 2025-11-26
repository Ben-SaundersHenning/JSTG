using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;

namespace docProcessor;

record struct PositionInfo(
    int Position,
    Text TextNode,
    Run RunNode,
    Paragraph ParagraphNode,
    int LocalOffset
);

internal class DocumentFlattener
{
    public record FlattenedDocument(
        string Text,
        List<PositionInfo> PositionMap
    );
    
    public static FlattenedDocument Flatten(Body body)
    {
        List<PositionInfo> positionMap = [];
        StringBuilder flattenedText = new();
        
        Text[] texts = body.Descendants<Text>().ToArray();
        int currentPosition = 0;
        
        foreach (Text text in texts)
        {
            if (string.IsNullOrEmpty(text.Text))
                continue;
                
            Run? run = text.Parent as Run;
            if (run == null) continue;
            
            Paragraph? para = run.Parent as Paragraph;
            if (para == null) continue;
            
            for (int i = 0; i < text.Text.Length; i++)
            {
                positionMap.Add(new PositionInfo(
                    Position: currentPosition,
                    TextNode: text,
                    RunNode: run,
                    ParagraphNode: para,
                    LocalOffset: i
                ));
                
                flattenedText.Append(text.Text[i]);
                currentPosition++;
            }
        }
        
        return new FlattenedDocument(
            flattenedText.ToString(),
            positionMap
        );
    }
}
