namespace Generator.Configurations;

using Data;
using Newtonsoft.Json;

public class GeneratorConfiguration
{
    public string TemplateDir { get; set; } = "template";
    public string DistDir { get; set; } = "dist";
    public string SrcDir { get; set; } = "src";
    public List<string> DirsToScripInProcessing { get; set; } = new() {"src/browser"};
    public string PagesDir { get; set; } = "pages";
    public string ScribanTemplate { get; set; } = "template.scriban-html";
    public string GalleryScribanTemplate { get; set; } = "gallery.scriban-html";
    public string AreaHTMLTag { get; set; } = "article";
    public int WebPQuality { get; set; } = 90;
    public int WebPQualityThumbnails { get; set; } = 80;
    public Style Style { get; set; } = new();
    public Auto Auto { get; set; } = new();
    public Website Website { get; set; } = new();
    public List<Page> Pages { get; set; } = new();
    public List<string> Articles { get; set; } = new();

    public static GeneratorConfiguration? LoadFromFile(string pathToWebsiteConfiguration)
    {
        if (!File.Exists(pathToWebsiteConfiguration))
        {
            return null;
        }

        string json = File.ReadAllText(pathToWebsiteConfiguration);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonConvert.DeserializeObject<GeneratorConfiguration>(json);
    }
}