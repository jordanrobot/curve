using System;
using System.IO;
using CurveEditor.ViewModels;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public class DirectoryBrowserViewModelTests
{
    [Fact]
    public void Constructor_InitializesWithEmptyCollections()
    {
        var vm = new DirectoryBrowserViewModel();

        Assert.NotNull(vm.Files);
        Assert.Empty(vm.Files);
        Assert.Null(vm.CurrentDirectory);
        Assert.Null(vm.SelectedFile);
    }

    [Fact]
    public void FileSelected_EventRaisedWhenOpenFileCommandExecuted()
    {
        var vm = new DirectoryBrowserViewModel();
        var testFilePath = "/test/path/file.json";
        var fileItem = new FileItem
        {
            FileName = "file.json",
            FullPath = testFilePath
        };

        string? raisedFilePath = null;
        vm.FileSelected += (sender, filePath) => raisedFilePath = filePath;

        vm.OpenFileCommand.Execute(fileItem);

        Assert.Equal(testFilePath, raisedFilePath);
    }

    [Fact]
    public void OpenFileCommand_DoesNotRaiseEvent_WhenFileItemIsNull()
    {
        var vm = new DirectoryBrowserViewModel();
        var eventRaised = false;
        vm.FileSelected += (sender, filePath) => eventRaised = true;

        vm.OpenFileCommand.Execute(null);

        Assert.False(eventRaised);
    }

    [Fact]
    public void OpenFileCommand_DoesNotRaiseEvent_WhenFullPathIsEmpty()
    {
        var vm = new DirectoryBrowserViewModel();
        var fileItem = new FileItem
        {
            FileName = "file.json",
            FullPath = string.Empty
        };

        var eventRaised = false;
        vm.FileSelected += (sender, filePath) => eventRaised = true;

        vm.OpenFileCommand.Execute(fileItem);

        Assert.False(eventRaised);
    }

    [Fact]
    public void FileItem_PropertiesAreSettable()
    {
        var fileItem = new FileItem
        {
            FileName = "test.json",
            FullPath = "/path/to/test.json"
        };

        Assert.Equal("test.json", fileItem.FileName);
        Assert.Equal("/path/to/test.json", fileItem.FullPath);
    }
}
