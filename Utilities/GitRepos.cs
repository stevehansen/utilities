// Checks which Project folder aren't in the correct folder structure
// All code projects are on the P:\ driver
// Inside P:\GH we have the different GitHub accounts by folder name
// P:\GH\DCG is for https://github.com/DeCronosGroep
// P:\GH\HC is for https://github.com/hansen-consultancy
// P:\GH\SH is for https://github.com/stevehansen
// P:\GH\2sky is for https://github.com/2sky
// P:\Various is for projects without a .git folder

// This program should ask for each folder before moving them

using System;
using System.Collections.Generic;
using System.IO;
using LibGit2Sharp;
using Spectre.Console;

AnsiConsole.Write(new FigletText("Git Repos").Color(Color.Green));

Dictionary<string, string> remoteMappings = new()
{
    { "https://github.com/DeCronosGroep/", @"P:\GH\DCG" },
    { "https://github.com/hansen-consultancy/", @"P:\GH\HC" },
    { "https://github.com/stevehansen/", @"P:\GH\SH" },
    { "https://github.com/2sky/", @"P:\GH\2sky" },
};

// Get all folders in P:\

foreach (var directory in Directory.EnumerateDirectories("P:\\"))
{
    if (directory is "P:\\GH" or "P:\\Various" or "P:\\$RECYCLE.BIN" or "P:\\packages" or "P:\\System Volume Information")
        continue;

    AnsiConsole.MarkupLine($"[bold]{directory}[/]");

    // Check if the folder has a .git folder

    if (Directory.Exists(Path.Combine(directory, ".git")))
    {
        // Use libgit2sharp to get the remote URL
        var repo = new Repository(directory);
        var remote = repo.Network.Remotes["origin"];
        if (remote is null)
        {
            AnsiConsole.MarkupLine("[red]No remote found[/]");
            continue;
        }

        // Find matching remote
        var remoteUrl = remote.Url;
        // Remote Url can also be in ssh format so convert it a bit to match the url format
        if (remoteUrl.StartsWith("git@"))
            remoteUrl = remoteUrl.Replace(":", "/").Replace("git@", "https://");

        var found = false;
        foreach (var remoteMapping in remoteMappings)
        {
            if (remoteUrl.StartsWith(remoteMapping.Key))
            {
                found = true;

                Directory.Move(directory, Path.Combine(remoteMapping.Value, Path.GetFileName(directory)));

                AnsiConsole.MarkupLine($"[green]Folder moved to {remoteMapping.Value}[/]");

                break;
            }
        }

        if (!found)
        {
            // Warn for unknown origin with remote url
            AnsiConsole.MarkupLine($"[red]Unknown origin: {remoteUrl}[/]");
        }
    }
    // Suggest to move to Various
    else if (AnsiConsole.Confirm("Do you want to move this folder to P:\\Various?"))
    {
        try
        {
            Directory.Move(directory, Path.Combine("p:\\Various", Path.GetFileName(directory)));

            AnsiConsole.MarkupLine("[green]Folder moved to P:\\Various[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }
}