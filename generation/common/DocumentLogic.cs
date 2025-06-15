using common.Interfaces;
using common.Models;

namespace common;

public class DocumentLogic: IDocumentLogic
{

    private Dictionary<string, Boolean> rules = new();

    // compute all the rules in the constructor
    public DocumentLogic(DocumentRequest _data)
    {
        // CLAIMANT IS MALE OR FEMALE
        if (_data.claimant.gender == "Male")
        {
            rules.Add("isMale", true);
        }
        else
        {
            rules.Add("isMale", false);
        }
        
    }

    public bool? GetRule(string key)
    {
        var x = rules.TryGetValue(key, out bool value);
        if (x) return value;
        return null;
    }
    
    

}