namespace Generator.Configurations;

using Newtonsoft.Json;

public class GeneratorConfiguration
{
    public string TemplateDir { get; set; } = "template";
    public string DistDir { get; set; } = "dist";
    public string SrcDir { get; set; } = "src";
    public string PagesDir { get; set; } = "pages";
    public string ScribanTemplate { get; set; } = "template.scriban";
    public int WebPQuality { get; set; } = 80;
    public Style Style { get; set; } = new Style();
    public Auto Auto { get; set; } = new Auto();
    public Website Website { get; set; } = new Website();

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