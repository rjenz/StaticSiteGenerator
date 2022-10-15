namespace Generator;

using System.Text.RegularExpressions;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Configurations;
using Data;
using Logging;
using Markdig;
using NaturalSort.Extension;
using Newtonsoft.Json;
using Scriban;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

internal static class Program
{
    private const string REGEX = @"<import\s+(?:[^>]*?\s+)?src=([""'])(.*?)\1>";
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
        config.PagesDir = Path.GetFullPath(config.PagesDir);

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

        DelegatedProcessing(config, config.TemplateDir, config.DistDir, new[] {".bmp", ".jpeg", ".jpg", ".png"}, ProcessImages);
        DelegatedProcessing(config, config.SrcDir, config.DistDir, new[] {".bmp", ".jpeg", ".jpg", ".png"}, ProcessImages);

        DelegatedProcessing(config, config.TemplateDir, config.DistDir, new[] {".css", ".js"}, FileContentProcessing);
        DelegatedProcessing(config, config.SrcDir, config.DistDir, new[] {".css", ".js"}, FileContentProcessing);

        ProcessPages(config);
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

        string[] fileEndingBlacklist = {".md", ".scriban-html", ".css", ".js", ".bmp", ".jpeg", ".jpg", ".png"}; //ingore all files we will process separately

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

            if (File.Exists(destFilePath))
            {
                File.Delete(destFilePath);
            }

            ;

            File.Copy(srcFilePath, destFilePath);
        }
    }

    private static void DelegatedProcessing(GeneratorConfiguration config, string sourceDir, string destDir, string[] fileEndings, FileProcessingDelegate processingDelegate)
    {
        string[] files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

        var filesFound = new List<Tuple<string, string>>();

        foreach (string srcFilePath in files)
        {
            if (!fileEndings.Any(fe => srcFilePath.EndsWith(fe)))
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

            if (File.Exists(destFilePath))
            {
                File.Delete(destFilePath);
            }

            ;

            filesFound.Add(new Tuple<string, string>(srcFilePath, destFilePath));
        }

        Parallel.ForEach(filesFound, new ParallelOptions {MaxDegreeOfParallelism = Environment.ProcessorCount}, tuple => processingDelegate.Invoke(config, tuple.Item1, tuple.Item2));
    }

    private static void ProcessImages(GeneratorConfiguration config, string sourceFilePath, string destFilePath)
    {
        LOGGER.Info($"Processing Image from {sourceFilePath}");

        destFilePath = Path.ChangeExtension(destFilePath, ".webp");
        string destFileThumbnailPath = destFilePath.Replace(".webp", "_thumb.webp");

        if (File.Exists(destFilePath))
        {
            File.Delete(destFilePath);
        }

        if (File.Exists(destFileThumbnailPath))
        {
            File.Delete(destFileThumbnailPath);
        }

        var encoder = new WebpEncoder {Quality = config.WebPQuality};

        Image loadedImage = Image.Load(sourceFilePath);

        int thumbnailWidth = loadedImage.Width / 2;
        int thumbnailHeight = loadedImage.Height / 2;

        loadedImage.Metadata.ExifProfile = null;

        loadedImage.SaveAsWebp(destFilePath, encoder);
        loadedImage.Mutate(image => image.Resize(thumbnailWidth, thumbnailHeight));
        loadedImage.SaveAsWebp(destFileThumbnailPath, encoder);
    }

    private static void FileContentProcessing(GeneratorConfiguration config, string sourceFilePath, string destFilePath)
    {
        LOGGER.Info($"Processing FileContent from {sourceFilePath}");

        if (File.Exists(destFilePath))
        {
            File.Delete(destFilePath);
        }

        string sourceContent = File.ReadAllText(sourceFilePath);
        Template? template = Template.Parse(sourceContent);

        string? result = template.Render(config);

        File.WriteAllText(destFilePath, result);
    }

    private static void ProcessPages(GeneratorConfiguration config)
    {
        string templatePath = Path.Combine(config.TemplateDir, config.ScribanTemplate);

        if (!File.Exists(templatePath))
        {
            throw new Exception($"Scriban Template {config.ScribanTemplate} does not exists at {config.TemplateDir}!");
        }

        foreach (Page page in config.Pages)
        {
            page.IsActive = true;

            string pageDir = Path.Combine(config.PagesDir, Path.GetFileNameWithoutExtension(page.Link));

            if (!Directory.Exists(pageDir))
            {
                throw new Exception($"Page Directory for {page.Link} does not exists at {pageDir}!");
            }

            string destFilePath = Path.Combine(config.DistDir, page.Link);

            if (File.Exists(destFilePath))
            {
                File.Delete(destFilePath);
            }

            config.Articles = GetArticlesForPage(config, pageDir);

            string templateContent = File.ReadAllText(templatePath);
            Template? template = Template.Parse(templateContent);
            string? html = template.Render(config);

            var parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument(html);
            var sw = new StringWriter();
            document.ToHtml(sw, new MinifyMarkupFormatter());
            string htmlPrettified = sw.ToString();

            File.WriteAllText(destFilePath, html);

            page.IsActive = false;
        }
    }

    private static List<string> GetArticlesForPage(GeneratorConfiguration config, string pageDir)
    {
        var articles = new List<string>();

        string[] sourceFiles = Directory.GetFiles(pageDir, "*", SearchOption.AllDirectories);
        sourceFiles = sourceFiles.OrderBy(x => x, StringComparison.OrdinalIgnoreCase.WithNaturalSort()).ToArray();

        foreach (string sourceFile in sourceFiles)
        {
            if (sourceFile.EndsWith(".md"))
            {
                articles.Add(ProcessMarkdownArticle(config, sourceFile));
            }
        }

        return articles;
    }

    private static string ProcessMarkdownArticle(GeneratorConfiguration config, string sourceFile)
    {
        string mdContent = File.ReadAllText(sourceFile);
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

        string html = Markdown.ToHtml(mdContent, pipeline);

        const RegexOptions OPTIONS = RegexOptions.Multiline;

        foreach (Match match in Regex.Matches(html, REGEX, OPTIONS))
        {
            string importedFile = match.Groups[^1].Value;
            string importedFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFile)!, importedFile));

            if (importedFilePath.EndsWith("_gallery.json"))
            {
                string insert = BuildGalleryFromJson(config, importedFilePath);
                html = html.Replace(match.Groups[0].Value, insert);
            }
            else
            {
                if (File.Exists(importedFilePath))
                {
                    string insert = File.ReadAllText(importedFilePath);
                    html = html.Replace(match.Groups[0].Value, insert);
                }
                else
                {
                    throw new Exception($"Cannot import {importedFilePath}");
                }
            }
        }

        return $"<article>{html}</article>";
    }

    private static string BuildGalleryFromJson(GeneratorConfiguration config, string jsonFile)
    {
        if (!File.Exists(jsonFile))
        {
            throw new Exception($"Cannot import {jsonFile}");
        }

        string templatePath = Path.Combine(config.TemplateDir, config.GalleryScribanTemplate);

        if (!File.Exists(templatePath))
        {
            throw new Exception($"Scriban Template {config.GalleryScribanTemplate} does not exists at {config.TemplateDir}!");
        }

        string importedFileContent = File.ReadAllText(jsonFile);
        var gallery = JsonConvert.DeserializeObject<Gallery>(importedFileContent);

        if (gallery == null)
        {
            throw new InvalidOperationException($"Could not parse {jsonFile} to Gallery!");
        }

        foreach (GalleryItem galleryItem in gallery.Items)
        {
            if (galleryItem.Image == null)
            {
                continue;
            }

            string galleryItemFullPath = Path.Combine(config.SrcDir, galleryItem.Image);

            if (!File.Exists(galleryItemFullPath))
            {
                throw new Exception($"Gallery Image {galleryItem.Image} not found!");
            }

            Image loadedImage = Image.Load(galleryItemFullPath);

            galleryItem.Width = loadedImage.Width;
            galleryItem.Height = loadedImage.Height;
            
            galleryItem.ThumbWidth = loadedImage.Width / 2;
            galleryItem.ThumbHeight = loadedImage.Height / 2;

            galleryItem.Image = Path.ChangeExtension(galleryItem.Image, ".webp");
            galleryItem.Thumbnail = galleryItem.Image.Replace(".webp", "_thumb.webp");
        }

        string templateContent = File.ReadAllText(templatePath);
        Template? template = Template.Parse(templateContent);
        string html = template.Render(gallery);

        return html;
    }

    private delegate void FileProcessingDelegate(GeneratorConfiguration config, string sourceFilePath, string destFilePath);
}