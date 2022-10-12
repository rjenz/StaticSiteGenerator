namespace Generator;

using Configurations;
using Logging;

internal static class Program
{
    private const string PROJECT_WEBSITE_CONFIG = "website.json";
    private static readonly Logger LOGGER = Logger.GetInstance();

    private static int Main(string[] args)
    {
        LOGGER.Configurate(nameof(Generator));

        if (args.Length != 1)
        {
            LOGGER.Error("Please provide a project folder when calling Generator. For example: generator.dll \"websites/websiteA\"");
            return 1;
        }

        string pathToWebsiteProject = args[0];
        string pathToWebsiteConfiguration = Path.GetFullPath(Path.Combine(pathToWebsiteProject, PROJECT_WEBSITE_CONFIG));

        WebsiteConfiguration? websiteConfig = WebsiteConfiguration.LoadFromFile(pathToWebsiteConfiguration);

        if (websiteConfig == null)
        {
            LOGGER.Error($"Failed to load website configuration \"{pathToWebsiteConfiguration}\"");
            return 2;
        }

        //TODO: Run Generation

        return 0;
    }
}