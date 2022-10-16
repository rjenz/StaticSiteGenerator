namespace Generator.Data;

public class GalleryItem
{
    public string? Image { get; set; }
    public string? Alt { get; set; }
    public bool LightBox { get; set; }
    public string? Link { get; set; }
    public string? Caption { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? Thumbnail { get; set; }
    public int ThumbWidth { get; set; }
    public int ThumbHeight { get; set; }
}