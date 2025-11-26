using DocumentFormat.OpenXml.Office2010.Word;
using DocumentFormat.OpenXml.Wordprocessing;
using Checked = DocumentFormat.OpenXml.Wordprocessing.Checked;

namespace docProcessor.Checkboxes;

public enum CheckboxType
{
    Legacy,
    Modern
}

public class Checkbox
{

    public string Identifier { get; }

    private bool _value;
    public bool Value
    {
        get => _value;
        set
        {
            _value = value;
            // TODO: update checkbox state
        }
    }
    
    public CheckboxType Type { get; }
    
    private CheckBox _legacyCheckbox;
    
    private SdtContentCheckBox _modernCheckBox;

    private Checkbox(string identifier, CheckboxType type, object checkbox)
    {
        
        Identifier = identifier;
        Type = type;
        
        if (type == CheckboxType.Modern)
            _modernCheckBox = (SdtContentCheckBox)checkbox;
        else
            _legacyCheckbox = (CheckBox)checkbox;

    }

    // TODO: finish this.
    public static Checkbox Find(Body body, string identifier)
    {
        
        // Look for modern, if found, return with modern
        var modern = body.Descendants<FormFieldName>()
            .FirstOrDefault(field => field.Val.Value == identifier);
        // Look for legacy, if found, return with legacy
        // if not found, throw
    }
    
    private void EditLegacyCheckbox(string tag, bool newState)
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

    private void EditCheckbox(string tag, bool newState)
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
}