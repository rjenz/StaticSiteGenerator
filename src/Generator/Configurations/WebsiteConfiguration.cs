namespace Generator.Configurations;

using Newtonsoft.Json;

public class WebsiteConfiguration
{
    public static WebsiteConfiguration? LoadFromFile(string pathToWebsiteConfiguration)
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

        return JsonConvert.DeserializeObject<WebsiteConfiguration>(json);
    }
}