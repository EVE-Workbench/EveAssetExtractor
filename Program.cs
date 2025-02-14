﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

var eveOnlineGameDirectory = @"c:\CCP\EVE";
var outputDirectory = @"c:\temp\export";

foreach (var arg in args)
{
    if (arg.StartsWith("--game-dir"))
    {
        eveOnlineGameDirectory = arg.Replace("--game-dir=", "");
    } else if (arg.StartsWith("--output-dir"))
    {
        outputDirectory = arg.Replace("--output-dir=", "");
    }
}

var resourceFile = $@"{eveOnlineGameDirectory}\tq\resfileindex.txt";
var resourceFilePrefetch = $@"{eveOnlineGameDirectory}\tq\resfileindex_prefetch.txt";
var eveOnlineResourceDirectory = $@"{eveOnlineGameDirectory}\ResFiles";

// create the output directory
Directory.CreateDirectory(outputDirectory);

List<ResourceRow> rows = [];

Console.WriteLine("EVE Online Asset Extractor");
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
        if(mediaType == null) continue;
        
        Directory.CreateDirectory($@"{outputDirectory}\{mediaType}");
        
        if (File.Exists($@"{eveOnlineResourceDirectory}\{image.Path}"))
        {
            var outputFile = $@"{outputDirectory}\{mediaType}\{image.FileName.Replace(@"res:/", string.Empty).Replace(@"/", "_")}";
            File.Copy($@"{eveOnlineResourceDirectory}\{image.Path}", outputFile, true);
            Console.WriteLine($@"Copies {eveOnlineResourceDirectory}\{image.Path} to {outputFile}");
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
    if (fileName.EndsWith(".png") || fileName.EndsWith(".jpg"))
    {
        return "image";
    }
    
    if (fileName.EndsWith(".webm") || fileName.EndsWith(".srt"))
    {
        return "video";
    }

    return null;
}

internal record ResourceRow
{
    public required string FileName { get; set; }
    public required string Path { get; set; }
}