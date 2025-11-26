using System.Text.RegularExpressions;

namespace docProcessor.Utility;

public static class Extensions
{
    
   public static int EndIndex(this Match match) => match.Index + match.Length - 1;
   
}
