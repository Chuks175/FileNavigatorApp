using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FileNavigationApp.Models;
using FileAttributes = System.IO.FileAttributes;

namespace FileNavigationApp.Services.Cross_Platform_Service;

public record SearchResultItem(
    string Name,    
    string FullPath,
    bool IsFolder,
    DateTime CreationTime,
    DateTime ModificationTime
)
{
    // Pre-formatting dates explicitly in C# solves all XAML cross-platform localization bugs
    public string CreatedDisplay => CreationTime.ToString("g"); // e.g., "5/16/2026 11:30 PM"
    public string ModifiedDisplay => ModificationTime.ToString("g");
}



public record SearchSession(
    List<SearchResultItem> AllItems, 
    long ElapsedMilliseconds
);

public class FileSearchService
{
    private readonly List<SearchResultItem> _cachedResults = new();
    public long LastSearchDurationMs { get; private set; }

    public SearchSession PerformSearch(string rootPath, string searchPattern, AppSettings settings)
    {
        _cachedResults.Clear();
        if (!Directory.Exists(rootPath)) return new SearchSession(_cachedResults, 0);

        var watch = Stopwatch.StartNew();
        try
        {
            var directoryInfo = new DirectoryInfo(rootPath);
            var options = new EnumerationOptions 
            { 
                IgnoreInaccessible = true, 
                //IgnoreInaccessible = false,
                AttributesToSkip = FileAttributes.None,
                RecurseSubdirectories = true, 
                MatchCasing = settings.IsCaseInsensitive ? MatchCasing.CaseInsensitive : MatchCasing.CaseSensitive
            };

            // 1. Find Matching Folders
            foreach (var dir in directoryInfo.EnumerateDirectories(searchPattern, options))
            {
                bool isHidden = dir.Name.StartsWith("*") || (dir.Attributes & FileAttributes.Hidden) != 0;
                if (isHidden && !settings.IncludeHiddenFiles) continue;
                
                _cachedResults.Add(new SearchResultItem(dir.Name, dir.FullName, true, dir.CreationTime, dir.LastWriteTime));
            }

            // 2. Find Matching Files
            foreach (var file in directoryInfo.EnumerateFiles(searchPattern, options))
            {
                bool isHidden = file.Name.StartsWith("*") || (file.Attributes & FileAttributes.Hidden) != 0;
                if (isHidden && !settings.IncludeHiddenFiles) continue;
                
                _cachedResults.Add(new SearchResultItem(file.Name, file.FullName, false, file.CreationTime, file.LastWriteTime));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Search error: {ex.Message}");
        }

        watch.Stop();
        LastSearchDurationMs = watch.ElapsedMilliseconds;
        return new SearchSession(_cachedResults, LastSearchDurationMs);
    }
    
    public List<SearchResultItem> GetPaginatedPage(int pageNumber, int pageSize)
    {
        return _cachedResults
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public int GetTotalResultsCount() => _cachedResults.Count;
    
}
