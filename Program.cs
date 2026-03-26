#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

string? providedGameDirectory = null;
string? providedOutputDirectory = null;

foreach (var arg in args)
{
    if (arg.StartsWith("--game-dir="))
    {
        providedGameDirectory = arg.Replace("--game-dir=", "");
    }
    else if (arg.StartsWith("--output-dir="))
    {
        providedOutputDirectory = arg.Replace("--output-dir=", "");
    }
}

var eveOnlineGameDirectory = ResolveGameDirectory(providedGameDirectory);
if (eveOnlineGameDirectory == null)
{
    Console.WriteLine("Could not find an EVE installation. Use --game-dir to set it explicitly.");
    return;
}

var outputDirectory = ResolveOutputDirectory(providedOutputDirectory);

var resourceFile = Path.Combine(eveOnlineGameDirectory, "tq", "resfileindex.txt");
var resourceFilePrefetch = Path.Combine(eveOnlineGameDirectory, "tq", "resfileindex_prefetch.txt");
var eveOnlineResourceDirectory = Path.Combine(eveOnlineGameDirectory, "ResFiles");

// create the output directory
Directory.CreateDirectory(outputDirectory);

List<ResourceRow> rows = [];

Console.WriteLine("EVE Online Asset Extractor");
Console.WriteLine($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine("Created by EVE Workbench");
Console.WriteLine("---");

if (File.Exists(resourceFile) && File.Exists(resourceFilePrefetch))
{
    // Read all lines from the CSV file
    rows.AddRange(ReadLines(resourceFile));
    rows.AddRange(ReadLines(resourceFilePrefetch));
    
    foreach (var image in rows)
    {
        var mediaType = GetMediaType(image.FileName);
        if (mediaType == null) continue;

        var mediaOutputDirectory = Path.Combine(outputDirectory, mediaType);
        Directory.CreateDirectory(mediaOutputDirectory);

        var sourceFile = Path.Combine(eveOnlineResourceDirectory, image.Path.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(sourceFile))
        {
            var outputFileName = image.FileName
                .Replace("res:/", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("/", "_")
                .Replace("\\", "_");
            var outputFile = Path.Combine(mediaOutputDirectory, outputFileName);
            File.Copy(sourceFile, outputFile, true);
            Console.WriteLine($"Copies {sourceFile} to {outputFile}");
        }
    }
}

List<ResourceRow> ReadLines(string file)
{
    var lines = File.ReadAllLines(file);
    return lines.Select(line => line.Split(',')).Select(values => new ResourceRow { FileName = values[0], Path = values[1] }).ToList();
}

string? GetMediaType(string fileName)
{
    if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
        fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
    {
        return "image";
    }

    if (fileName.EndsWith(".webm", StringComparison.OrdinalIgnoreCase) ||
        fileName.EndsWith(".srt", StringComparison.OrdinalIgnoreCase))
    {
        return "video";
    }

    return null;
}

string ResolveOutputDirectory(string? cliOutputDirectory)
{
    if (!string.IsNullOrWhiteSpace(cliOutputDirectory))
    {
        return cliOutputDirectory;
    }

    // Place extracted assets next to the executable when no output path is provided.
    return Path.Combine(AppContext.BaseDirectory, "assets");
}

string? ResolveGameDirectory(string? cliGameDirectory)
{
    if (!string.IsNullOrWhiteSpace(cliGameDirectory))
    {
        return cliGameDirectory;
    }

    var candidates = GetCandidateGameDirectories();
    foreach (var candidate in candidates.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct())
    {
        if (HasRequiredEveFiles(candidate))
        {
            return candidate;
        }
    }

    return null;
}

IEnumerable<string> GetCandidateGameDirectories()
{
    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    var candidates = new List<string>();

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        candidates.Add(Path.Combine("C:", "CCP", "EVE"));
        candidates.Add(Path.Combine("D:", "CCP", "EVE"));
        candidates.Add(Path.Combine("E:", "CCP", "EVE"));
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        // Need a mac first to determine this
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        candidates.Add(Path.Combine(home, ".steam", "steam", "steamapps", "compatdata", "8500", "pfx", "drive_c", "CCP", "EVE"));
        candidates.Add(Path.Combine(home, ".local", "share", "Steam", "steamapps", "compatdata", "8500", "pfx", "drive_c", "CCP", "EVE"));
    }

    return candidates;
}

bool HasRequiredEveFiles(string gameDirectory)
{
    if (!Directory.Exists(gameDirectory))
    {
        return false;
    }

    var indexFile = Path.Combine(gameDirectory, "tq", "resfileindex.txt");
    var prefetchFile = Path.Combine(gameDirectory, "tq", "resfileindex_prefetch.txt");
    var resFilesDirectory = Path.Combine(gameDirectory, "ResFiles");

    return File.Exists(indexFile) && File.Exists(prefetchFile) && Directory.Exists(resFilesDirectory);
}

internal record ResourceRow
{
    public required string FileName { get; set; }
    public required string Path { get; set; }
}