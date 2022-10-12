namespace Generator.Logging;

using System;
using System.Diagnostics;
using NLog;
using NLog.Config;
using NLog.Targets;

public class Logger : IDisposable
{
    private const string DEFAULT_LOG_DATE_PATTERN = @"${date:format=dd\:MM\:yyyy HH\:mm\:ss.ff} | ${threadid} | ${level:uppercase=true} | ${message}";
    private const string DEFAULT_LOG_FILE_EXTENSION = ".log";
    private const string DEFAULT_LOG_FILE_DIR = "../logs/";
    private const int DEFAULT_MAX_FILE_SIZE = 10 * 1024 * 1024;

    private static Logger? s_instance;
    private ColoredConsoleTarget? m_consoleTarget;
    private FileTarget? m_fileTarget;

    private NLog.Logger? m_logger;
    private string? m_path;

    private Logger()
    {
    }

    public void Dispose()
    {
        s_instance = null;
        m_logger = null;
        m_fileTarget?.Dispose();
        m_consoleTarget?.Dispose();
        m_path = null;
    }

    public static Logger GetInstance()
    {
        return s_instance ??= new Logger();
    }

    public void Configurate(string name, string dir = DEFAULT_LOG_FILE_DIR, string extension = DEFAULT_LOG_FILE_EXTENSION, string pattern = DEFAULT_LOG_DATE_PATTERN, int maxFileSize = DEFAULT_MAX_FILE_SIZE)
    {
        var config = new LoggingConfiguration();

        m_fileTarget?.Dispose();
        m_consoleTarget?.Dispose();

        m_fileTarget = new FileTarget(nameof(FileTarget));

        m_consoleTarget = new ColoredConsoleTarget(nameof(ColoredConsoleTarget))
        {
            WordHighlightingRules =
            {
                new ConsoleWordHighlightingRule("DEBUG", ConsoleOutputColor.Blue, ConsoleOutputColor.Black),
                new ConsoleWordHighlightingRule("INFO", ConsoleOutputColor.Green, ConsoleOutputColor.Black),
                new ConsoleWordHighlightingRule("WARN", ConsoleOutputColor.DarkYellow, ConsoleOutputColor.Black),
                new ConsoleWordHighlightingRule("FATAL", ConsoleOutputColor.Red, ConsoleOutputColor.Black)
            }
        };

        config.AddTarget(m_fileTarget);
        config.AddTarget(m_consoleTarget);

        m_fileTarget.ArchiveOldFileOnStartup = true;
        m_fileTarget.ArchiveAboveSize = maxFileSize;
        m_fileTarget.ArchiveNumbering = ArchiveNumberingMode.DateAndSequence;
        m_fileTarget.CreateDirs = true;
        m_fileTarget.Layout = pattern;

        m_consoleTarget.Layout = pattern;
        m_path = $"{dir}{name.Trim('.')}.{extension.Trim('.')}";
        m_fileTarget.FileName = m_path;

#if DEBUG
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, nameof(FileTarget));
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, nameof(ColoredConsoleTarget));
#else
                config.AddRule(LogLevel.Info, LogLevel.Fatal, nameof(FileTarget)); 
                config.AddRule(LogLevel.Info, LogLevel.Fatal, nameof(ColoredConsoleTarget));
#endif

        LogManager.Configuration = config;
        m_logger = LogManager.GetLogger(name);
    }

    public string? GetPathToCurrentLogFile()
    {
        return m_path;
    }

    [Conditional("DEBUG")]
    public void Debug(string msg)
    {
        m_logger?.Debug(msg);
    }

    public void Info(string msg)
    {
        m_logger?.Info(msg);
    }

    public void Warning(string msg)
    {
        m_logger?.Warn(msg);
    }

    public void Error(string msg)
    {
        m_logger?.Fatal(msg);
    }
}