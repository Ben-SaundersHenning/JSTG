using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Office2021.OfficeExtLst;

namespace DocProcessor;

public static class Utility
{

   public static string ToUpperFirstChar(string s)
   {
      return char.ToUpper(s[0]) + s.Substring(1);
   }

   public static string ToLowerFirstChar(string s)
   {
      return char.ToLower(s[0]) + s.Substring(1);
   }
    
}

public static class UtilityExtensions
{
   public static int EndIndex(this Match match) => match.Index + match.Length - 1;

}