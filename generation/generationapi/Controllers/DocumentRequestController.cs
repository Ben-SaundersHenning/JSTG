using System.Runtime.InteropServices.JavaScript;
using DocProcessor;
using DocumentFormat.OpenXml.Presentation;
using generationapi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Document = DocProcessor.Document;

namespace generationapi.Controllers;

public class DocumentRequestController(IConfiguration configuration) : Controller
{
    
    private JObject Obj { get; set; } = new();

    private readonly IConfiguration Configuration = configuration;

    [HttpPost("DocRequest")]
    public IActionResult DocRequest([FromBody] DocumentRequest data)
    {

        if (data is null)
        {
            return BadRequest();
        }
        
        DefaultContractResolver contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        };        

        Obj = JObject.Parse(JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            ContractResolver = contractResolver,
            Formatting = Formatting.Indented
        }));
            
        byte[] result = GenerateDocument(Obj, TagReplace);
            
        return new FileContentResult(result, "application/octet-stream");
            
    }

    private byte[] GenerateDocument(JObject data, Func<string, string> replacementFunc)
    {
        
        // 1. Open stream
        
        using MemoryStream stream = new();
        
        // 2. Find doc file name, open document
        
        //TODO: check if doc exists

        string filePaths = configuration["TemplatesPath"];
        string docFileName = (string)data.SelectToken("document.file_name");

        Document doc = new Document(filePaths + docFileName, DocumentType.ExistingDocument);

        // 3. Resolve conditional statements
        
        // TODO
        
        // 3. Find image file name
        
        //TODO: check if image exists
        
        string imgPaths = configuration["ImagesPath"];
        string imgFileName = (string)data.SelectToken("signature_file_name");
        
        // 4. Insert image into document
        
        //image replace has to be done first, since the tag matches the text replacement tags.
        Image image = new(imgPaths + imgFileName);
        doc.ReplaceTextWithImage("<assessor.signature>", image); 
        
        // 5. Replace all tags
        
        //replace the tags
        //doc.SearchAndReplaceTextByRegex(@"<([\w \[\]._-]{3,})>", replacementFunc); 
        doc.ProcessDocument(replacementFunc, Obj);
        
        // 6. Save doc into byte array
        
        doc.SaveAsStream(stream);
        doc.Dispose();
        
        return stream.ToArray();

    }
    
    // Given a JSON path and returns the value that is to be inserted 
    // at the position of the path
    private string TagReplace(string objPath)
    {
        
        // 1. Try to get the token from Obj
        // 2. if the value has formatting (like a date), format it and then replace it
        // 3. otherwise just return the value
        // 4. if the value does not exist, return {ERR: key}

        JToken? token = Obj.SelectToken(objPath);

        if (token != null)
        {

            return token.ToString();

        }

        return $"{{ERR: {objPath}}}";

    }

}