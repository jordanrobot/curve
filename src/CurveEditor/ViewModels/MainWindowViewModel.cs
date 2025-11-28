using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurveEditor.Models;
using CurveEditor.Services;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CurveEditor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly FileService _fileService;
    private readonly CurveGeneratorService _curveGeneratorService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private MotorDefinition? _currentMotor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private bool _isDirty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public MainWindowViewModel()
    {
        _fileService = new FileService();
        _curveGeneratorService = new CurveGeneratorService();
    }

    public string WindowTitle
    {
        get
        {
            var fileName = _fileService.CurrentFilePath is not null
                ? Path.GetFileName(_fileService.CurrentFilePath)
                : CurrentMotor?.MotorName ?? "Untitled";

            var dirtyIndicator = IsDirty ? " *" : "";
            return $"{fileName}{dirtyIndicator} - Curve Editor";
        }
    }

    [RelayCommand]
    private void NewMotor()
    {
        Log.Information("Creating new motor definition");

        // Create a new motor with default values
        CurrentMotor = _fileService.CreateNew(
            motorName: "New Motor",
            maxRpm: 5000,
            maxTorque: 50,
            maxPower: 1500);

        IsDirty = _fileService.IsDirty;
        StatusMessage = "Created new motor definition";
    }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        Log.Information("Opening file dialog");

        try
        {
            // For now, we'll load a sample file - in a real implementation,
            // this would open a file dialog
            var samplePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "..",
                "samples", "example-motor.json");

            if (File.Exists(samplePath))
            {
                CurrentMotor = await _fileService.LoadAsync(samplePath);
                IsDirty = _fileService.IsDirty;
                StatusMessage = $"Loaded: {Path.GetFileName(samplePath)}";
                OnPropertyChanged(nameof(WindowTitle));
            }
            else
            {
                StatusMessage = "Sample file not found";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (CurrentMotor is null) return;

        try
        {
            if (_fileService.CurrentFilePath is null)
            {
                // No file path yet, would need SaveAs dialog
                StatusMessage = "Use Save As to save new file";
                return;
            }

            await _fileService.SaveAsync(CurrentMotor);
            IsDirty = false;
            StatusMessage = "File saved successfully";
            OnPropertyChanged(nameof(WindowTitle));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save file");
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }

    private bool CanSave() => CurrentMotor is not null;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsAsync()
    {
        if (CurrentMotor is null) return;

        try
        {
            // For now, save to a temp location - in real implementation, show dialog
            var tempPath = Path.Combine(Path.GetTempPath(), $"{CurrentMotor.MotorName}.json");
            await _fileService.SaveAsAsync(CurrentMotor, tempPath);
            IsDirty = false;
            StatusMessage = $"Saved to: {tempPath}";
            OnPropertyChanged(nameof(WindowTitle));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save file");
            StatusMessage = $"Error saving: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveCopyAsAsync()
    {
        if (CurrentMotor is null) return;

        try
        {
            // Save a copy without changing the current file
            var copyPath = Path.Combine(Path.GetTempPath(), $"{CurrentMotor.MotorName}_copy.json");
            await _fileService.SaveCopyAsAsync(CurrentMotor, copyPath);
            StatusMessage = $"Copy saved to: {copyPath}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save copy");
            StatusMessage = $"Error saving copy: {ex.Message}";
        }
    }

    [RelayCommand]
    private static void Exit()
    {
        Log.Information("Exiting application");
        Environment.Exit(0);
    }

    /// <summary>
    /// Called when any property on the motor changes.
    /// </summary>
    public void MarkDirty()
    {
        _fileService.MarkDirty();
        IsDirty = true;
    }
}
