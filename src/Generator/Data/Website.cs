namespace Generator.Data;

public class Website
{
    public string? Author { get; set; }
    public string? AuthorOccupation { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactIntro { get; set; }
    public string? Description { get; set; }
    public string? Favicon { get; set; }
    public string? ImprintLink { get; set; }
    public string? ImprintText { get; set; }
    public string? LogoDescription { get; set; }
    public string? Logo { get; set; }
    public int LogoWidth { get; set; } = 1280;
    public int LogoHeight { get; set; } = 720;
    public string? ThemeColor { get; set; }
    public string? Title { get; set; }
    public string? Webmanifest { get; set; }
}