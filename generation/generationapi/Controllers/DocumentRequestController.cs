using System.Runtime.InteropServices.JavaScript;
using common;
using common.Interfaces;
using common.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using DocKit;
using DocKit.TemplateEngine;
using Document = DocKit.Document;

namespace generationapi.Controllers;

public class DocumentRequestController : Controller
{
    
    private JObject Obj { get; set; } = new();

    private readonly IConfiguration _configuration;
    
    public DocumentRequestController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("DocRequest")]
    public IActionResult DocRequest([FromBody] DocumentRequest data)
    {

        if (data is null)
        {
            return BadRequest();
        }
        
        var json = JsonConvert.SerializeObject(data);
        Obj = JObject.Parse(json); 
        
       // DocumentLogic logic = new(data);
       // 
       // // fix image path (TEMP)
       // string imgPaths = _configuration["ImagesPath"];
       // data.signatureFileName = imgPaths + data.signatureFileName;
       // 
       // DefaultContractResolver contractResolver = new DefaultContractResolver
       // {
       //     NamingStrategy = new SnakeCaseNamingStrategy()
       // };        

       // Obj = JObject.Parse(JsonConvert.SerializeObject(data, new JsonSerializerSettings
       // {
       //     ContractResolver = contractResolver,
       //     Formatting = Formatting.Indented
       // }));
       //     
       // byte[] result = GenerateDocument(Obj, logic, TagReplace);
       
       string filePaths = _configuration["TemplatesPath"];

       Document doc = Document.Open("/Users/ben/Projects/DocKit/Documents_Testing/template_engine/tags.docx");
        
       TemplateEngine eng = new TemplateEngine();
       eng.RunEngine(doc, ReplaceFunc);
        
       Stream docStream = doc.SaveAsStream();

       using MemoryStream ms = new();
       docStream.CopyTo(ms);
       byte[] result = ms.ToArray();
       
        return new FileContentResult(result, "application/octet-stream");
            
    }

    //private byte[] GenerateDocument(JObject data, IDocumentLogic logic, Func<string, string> replacementFunc)
    //{
    //    
    //    // 1. Open stream
    //    
    //    using MemoryStream stream = new();
    //    
    //    // 2. Find doc file name, open document
    //    
    //    //TODO: check if doc exists

    //    string filePaths = _configuration["TemplatesPath"];
    //    string docFileName = (string)data.SelectToken("document.file_name");

    //    Document doc = new Document(filePaths + docFileName, DocumentType.ExistingDocument);

    //    // 3. Resolve conditional statements
    //    
    //    // TODO
    //    
    //    // 3. Find image file name
    //    
    //    //TODO: check if image exists
    //    
    //    
    //    // 4. Insert image into document
    //    
    //    //image replace has to be done first, since the tag matches the text replacement tags.
    //    
    //    // 5. Replace all tags
    //    
    //    //replace the tags
    //    //doc.SearchAndReplaceTextByRegex(@"<([\w \[\]._-]{3,})>", replacementFunc); 
    //    doc.ProcessDocument(replacementFunc, Obj, logic);
    //    
    //    // 6. Save doc into byte array
    //    
    //    doc.SaveAsStream(stream);
    //    doc.Dispose();
    //    
    //    return stream.ToArray();

    //}

    private string ReplaceFunc(string key)
    {

        if (key == "")
        {
            // TODO: handle
            return "";
        }
        
        JToken? token = Obj.SelectToken(key);

        return token != null ? token.ToString() : "";
        
    }

}