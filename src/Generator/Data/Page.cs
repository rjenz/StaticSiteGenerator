namespace Generator.Data;

public class Page
{
    public string Text { get; set; } = "index";
    public string Link { get; set; } = "index.html";
    public bool IsActive { get; set; } = false;
    public bool IsInNav { get; set; } = true;
}