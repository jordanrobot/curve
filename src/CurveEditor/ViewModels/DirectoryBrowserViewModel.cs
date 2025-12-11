using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;

namespace CurveEditor.ViewModels;

/// <summary>
/// ViewModel for the directory browser panel that lists JSON motor definition files.
/// </summary>
public partial class DirectoryBrowserViewModel : ViewModelBase
{
    /// <summary>
    /// Current directory path being browsed.
    /// </summary>
    [ObservableProperty]
    private string? _currentDirectory;

    /// <summary>
    /// Collection of JSON files in the current directory.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<FileItem> _files = [];

    /// <summary>
    /// Currently selected file in the list.
    /// </summary>
    [ObservableProperty]
    private FileItem? _selectedFile;

    /// <summary>
    /// Event raised when a file is selected to be opened.
    /// </summary>
    public event EventHandler<string>? FileSelected;

    [RelayCommand]
    private async Task OpenFolderAsync()
    {
        Log.Information("Opening folder picker for directory browser");

        try
        {
            var storageProvider = GetStorageProvider();
            if (storageProvider is null)
            {
                Log.Warning("Storage provider not available");
                return;
            }

            var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Folder",
                AllowMultiple = false
            });

            if (folders.Count == 0)
            {
                Log.Information("Folder selection cancelled");
                return;
            }

            var folder = folders[0];
            CurrentDirectory = folder.Path.LocalPath;
            
            Log.Information("Selected directory: {Directory}", CurrentDirectory);
            RefreshFileList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open folder picker");
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        Log.Information("Refreshing file list");
        RefreshFileList();
    }

    [RelayCommand]
    private void OpenFile(FileItem? fileItem)
    {
        if (fileItem is null || string.IsNullOrEmpty(fileItem.FullPath))
        {
            Log.Warning("Attempted to open null or invalid file");
            return;
        }

        Log.Information("Opening file from directory browser: {FilePath}", fileItem.FullPath);
        FileSelected?.Invoke(this, fileItem.FullPath);
    }

    /// <summary>
    /// Refreshes the file list from the current directory.
    /// </summary>
    private void RefreshFileList()
    {
        Files.Clear();

        if (string.IsNullOrEmpty(CurrentDirectory) || !Directory.Exists(CurrentDirectory))
        {
            Log.Warning("Current directory is null or does not exist: {Directory}", CurrentDirectory);
            return;
        }

        try
        {
            var jsonFiles = Directory.GetFiles(CurrentDirectory, "*.json", SearchOption.TopDirectoryOnly)
                .OrderBy(f => Path.GetFileName(f))
                .Select(f => new FileItem
                {
                    FileName = Path.GetFileName(f),
                    FullPath = f
                })
                .ToList();

            foreach (var file in jsonFiles)
            {
                Files.Add(file);
            }

            Log.Information("Loaded {Count} JSON files from directory", jsonFiles.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load files from directory: {Directory}", CurrentDirectory);
        }
    }

    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.StorageProvider;
        }
        return null;
    }
}

/// <summary>
/// Represents a file item in the directory browser.
/// </summary>
public class FileItem
{
    /// <summary>
    /// Display name of the file (filename only).
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Full path to the file.
    /// </summary>
    public string FullPath { get; set; } = string.Empty;
}
