using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace docProcessor;

public enum DocumentType
{
    NewDocument,
    ExistingDocument, // docx
    Template // dotx
}


public class Document: IDisposable
{
    
    private static class FileExtensions
    {
        public const string Document = ".docx";
        public const string Template = ".dotx";
    }
    
    public string DocPath { get; set; }
    
    private MemoryStream WorkingDoc { get; set; }
    
    private string DirPath { get; set; }
    
    private uint AltChunkCount { get; set; }
    
    private Regex Matcher { get; init; }
    
    public DocumentType DocumentType { get; set; }
    
    private WordprocessingDocument Doc { get; set; }
    
    private MainDocumentPart MainPart { get; set; }
    
    private Body Body { get; set; }

    public Document(string path, DocumentType documentType)
    {

        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));
        
        DocPath = path;
        DocumentType = documentType;
        DirPath = Path.GetDirectoryName(DocPath)!;
        AltChunkCount = 0;
        
        Matcher = new Regex(
            @"<<(?<tagtype>if|/if|image|doc|logic||) *\[(?<operand>[ \w\[\]\\/._-]{3,})\](?<flags>[ \w\[\]\\/.:_-]*)>>|<<(?<tagtype>/if)>>",
            RegexOptions.Compiled);

        if (DocumentType == DocumentType.NewDocument)
        {
            InitializeNewDocument();
        }
        else
        {
            InitializeExistingDocument();
        }

    }

    // Open a file and auto-detect type
    public static Document Open(string path)
    {

        if (File.Exists(path) == false)
            throw new FileNotFoundException("File not found", path);
        
        String fileExtension = Path.GetExtension(path).ToLowerInvariant();

        DocumentType type = fileExtension switch
        {
            FileExtensions.Document => DocumentType.ExistingDocument,
            FileExtensions.Template => DocumentType.Template,
            _ => throw new NotSupportedException($"File type {fileExtension} is not supported.")
        };

        return new Document(path, type); 
        
    }

    // Open a document and allow dev to specify type
    public static Document Open(string path, DocumentType documentType)
    {
        return new Document(path, documentType);
    }

    // Create a new document, at path
    public static Document Create(string path)
    {
        
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));
        
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        
        if (File.Exists(path))
            throw new IOException($"File already exists: {path}"); 
        
        return new Document(path, DocumentType.NewDocument);
        
    }

    private void InitializeExistingDocument()
    { 
        
        WorkingDoc = new MemoryStream(File.ReadAllBytes(DocPath));
        Doc = WordprocessingDocument.Open(WorkingDoc, true);
        
        MainPart = Doc.MainDocumentPart ?? Doc.AddMainDocumentPart();
        Body = MainPart.Document.Body ?? MainPart.Document.AppendChild(new Body());
        
    }

    private void InitializeNewDocument()
    {
        
        WorkingDoc = new MemoryStream();
        Doc = WordprocessingDocument.Create(WorkingDoc, WordprocessingDocumentType.Document);
    
        MainPart = Doc.AddMainDocumentPart();
        MainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
        Body = MainPart.Document.AppendChild(new Body());
        
    }

    public void InsertText(string text)
    {
        Body.AppendChild(new Paragraph(new Run(new Text(text))));
    }

    public void EditCheckbox(string identifier, bool value)
    {
        Checkboxes.Checkbox checkBox = new(identifier, value);
    }

    public void Save()
    {
        
        if (string.IsNullOrWhiteSpace(DocPath))
            throw new InvalidOperationException("Document path is empty. Use SaveAs() instead.");

        Doc.Save();
        
        WorkingDoc.Seek(0, SeekOrigin.Begin);

        using (FileStream fs = new FileStream(DocPath, FileMode.Create, FileAccess.Write))
        {
            WorkingDoc.CopyTo(fs);
        }

    }

    public void SaveAs(string path)
    {
        
        if (string.IsNullOrWhiteSpace(DocPath))
            throw new ArgumentNullException(nameof(path));
        
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        
        Doc.Save();
        
        WorkingDoc.Seek(0, SeekOrigin.Begin);

        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            WorkingDoc.CopyTo(fs);
        }

        DocPath = path;
        DirPath = Path.GetDirectoryName(DocPath)!;

    }

    public Stream SaveAsStream()
    {
        
        Doc.Save();
        
        WorkingDoc.Seek(0, SeekOrigin.Begin);
        MemoryStream stream = new();
        WorkingDoc.CopyTo(stream);
        stream.Seek(0, SeekOrigin.Begin);

        return stream;

    }

    public void Dispose()
    {
        WorkingDoc.Dispose();
        Doc.Dispose();
    }
}