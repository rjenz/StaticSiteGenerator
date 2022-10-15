namespace Generator;

using Configurations;
using Logging;

internal static class Program
{
    private const string PROJECT_GENERATOR_CONFIG = "generator-config.json";
    private static readonly Logger LOGGER = Logger.GetInstance();

    private static int Main(string[] args)
    {
        LOGGER.Configurate(nameof(Generator));
        LOGGER.Info("Booting!");

        if (args.Length != 1)
        {
            LOGGER.Error("Please provide a project folder when calling Generator. For example: generator.dll \"websites/websiteA\"");
            return 1;
        }

        string pathToProject = args[0];
        string pathToGeneratorConfig = Path.GetFullPath(Path.Combine(pathToProject, PROJECT_GENERATOR_CONFIG));

        GeneratorConfiguration? config = GeneratorConfiguration.LoadFromFile(pathToGeneratorConfig);

        if (config == null)
        {
            LOGGER.Error($"Failed to load generator configuration \"{pathToGeneratorConfig}\"");
            return 2;
        }

        Environment.CurrentDirectory = args[0];
        
        config.TemplateDir = Path.GetFullPath(config.TemplateDir);
        config.DistDir = Path.GetFullPath(config.DistDir);
        config.SrcDir = Path.GetFullPath(config.SrcDir);
        
        LOGGER.Info("Ready!");
        
        Run(config);

        LOGGER.Info("Done!");
        return 0;
    }

    private static void Run(GeneratorConfiguration config)
    {
        ResetDirectory(config.DistDir);
        LOGGER.Info($"DistDir at {config.DistDir} created!");

        ProcessDir(config.TemplateDir, config.DistDir);
        LOGGER.Info($"Template files from {config.TemplateDir} copied!");
        
        ProcessDir(config.SrcDir, config.DistDir);
        LOGGER.Info($"Src files from {config.SrcDir} copied!");
    }
    
    private static void ResetDirectory(string dir)
    {
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }

        Directory.CreateDirectory(dir);
    }

    private static void ProcessDir(string sourceDir, string destDir)
    {
        string[] files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

        string[] fileEndingBlacklist = {".md", ".scriban", ".css", ".js", ".bmp", ".jpeg", ".jpg", ".png"}; //ingore all files we will process separately
        
        foreach (string srcFilePath in files)
        {
            if (srcFilePath.Contains("git"))
            {
                continue;
            }

            if (fileEndingBlacklist.Any(feb => srcFilePath.EndsWith(feb)))
            {
                continue;
            }

            string destFilePath = srcFilePath.Replace(sourceDir, destDir);
            string? destFileDirPath = Path.GetDirectoryName(destFilePath);

            if (destFileDirPath != null)
            {
                if (!Directory.Exists(destFileDirPath))
                {
                    Directory.CreateDirectory(destFileDirPath);
                }
            }
            
            File.Copy(srcFilePath, destFilePath);
        }
    }
}