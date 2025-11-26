namespace DocProcessor;

using SkiaSharp;

public class Image
{

    internal string File { get; set; }
    
    internal int Width { get; set; }
    
    internal int Height { get; set; }
    
    public Image(string fileName)
    {

        File = fileName;
        
        using (var image = SKImage.FromEncodedData(fileName))
        {
            Width = (int)Math.Round((decimal)image.Width * 9525);
            Height = (int)Math.Round((decimal)image.Height * 9525);
        }

    }
    
}