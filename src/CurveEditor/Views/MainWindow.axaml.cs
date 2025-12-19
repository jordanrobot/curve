using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CurveEditor.Behaviors;
using CurveEditor.Models;
using CurveEditor.ViewModels;

namespace CurveEditor.Views;

public partial class MainWindow : Window
{
    private const double MaxSpeedChangeTolerance = 0.1;
    private double _previousMaxSpeed;
    private string? _previousActiveLeftPanelId;
    private string? _previousActiveRightPanelId;

    public MainWindow()
    {
        InitializeComponent();
        WindowBoundsPersistence.Attach(this, "MainWindow");
        Opened += OnOpened;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Opened -= OnOpened;

        var rootGrid = this.FindControl<Grid>("RootLayoutGrid");
        var mainGrid = this.FindControl<Grid>("MainLayoutGrid");
        var centerGrid = this.FindControl<Grid>("CenterGrid");
        var panelBar = this.FindControl<PanelBar>("PanelBarControl");

        if (rootGrid is null || mainGrid is null || centerGrid is null || DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        // Wire up PanelBar to ViewModel
        if (panelBar is not null)
        {
            panelBar.PanelClicked += (_, panelId) =>
            {
                viewModel.TogglePanel(panelId);
            };

            // Update PanelBar when active panel changes in either zone.
            // Multiple zones may be expanded simultaneously; PanelBar must highlight all expanded panels.
            viewModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MainWindowViewModel.ActivePanelBarPanelId) ||
                    args.PropertyName == nameof(MainWindowViewModel.ActiveLeftPanelId))
                {
                    panelBar.ActivePanelIds = GetPanelBarActivePanelIds(viewModel);
                }
                else if (args.PropertyName == nameof(MainWindowViewModel.PanelBarDockSide))
                {
                    ApplyPanelBarDockSide(rootGrid, panelBar, viewModel.PanelBarDockSide);
                }
            };

            // Set initial state
            panelBar.ActivePanelIds = GetPanelBarActivePanelIds(viewModel);
            ApplyPanelBarDockSide(rootGrid, panelBar, viewModel.PanelBarDockSide);
        }

        // Load persisted state for both zones
        var activeRightPanelId = PanelLayoutPersistence.LoadString("MainWindow.ActivePanelBarPanelId");
        if (activeRightPanelId is not null && PanelRegistry.GetById(activeRightPanelId) is not null)
        {
            var descriptor = PanelRegistry.GetById(activeRightPanelId);
            if (descriptor?.Zone == PanelZone.Right)
            {
                viewModel.ActivePanelBarPanelId = activeRightPanelId;
            }
        }

        var activeLeftPanelId = PanelLayoutPersistence.LoadString("MainWindow.ActiveLeftPanelId");
        if (activeLeftPanelId is not null && PanelRegistry.GetById(activeLeftPanelId) is not null)
        {
            var descriptor = PanelRegistry.GetById(activeLeftPanelId);
            if (descriptor?.Zone == PanelZone.Left)
            {
                viewModel.ActiveLeftPanelId = activeLeftPanelId;
            }
        }

        // Attach persistence for PanelBarDockSide
        var dockSide = PanelLayoutPersistence.LoadDockSide("MainWindow.PanelBarDockSide");
        if (dockSide.HasValue)
        {
            viewModel.PanelBarDockSide = dockSide.Value;
        }

        // Track previous active panel IDs so we can persist per-panel widths when switching.
        _previousActiveLeftPanelId = viewModel.ActiveLeftPanelId;
        _previousActiveRightPanelId = viewModel.ActivePanelBarPanelId;

        // Apply initial layout for expanded/collapsed state and per-panel widths.
        ApplyLeftZoneLayout(mainGrid, viewModel);
        ApplyRightZoneLayout(mainGrid, viewModel);

        // Save panel state on window closing
        Closing += (_, _) =>
        {
            PanelLayoutPersistence.SaveString("MainWindow.ActivePanelBarPanelId", viewModel.ActivePanelBarPanelId);
            PanelLayoutPersistence.SaveString("MainWindow.ActiveLeftPanelId", viewModel.ActiveLeftPanelId);
            PanelLayoutPersistence.SaveDockSide("MainWindow.PanelBarDockSide", viewModel.PanelBarDockSide);

            PersistActiveZoneWidths(mainGrid, viewModel);
        };

        // React to active zone changes.
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.ActiveLeftPanelId))
            {
                PersistLeftZoneWidthIfNeeded(mainGrid);
                _previousActiveLeftPanelId = viewModel.ActiveLeftPanelId;
                ApplyLeftZoneLayout(mainGrid, viewModel);
            }
            else if (args.PropertyName == nameof(MainWindowViewModel.ActivePanelBarPanelId))
            {
                PersistRightZoneWidthIfNeeded(mainGrid);
                _previousActiveRightPanelId = viewModel.ActivePanelBarPanelId;
                ApplyRightZoneLayout(mainGrid, viewModel);
            }
        };
    }

    private static IReadOnlyCollection<string> GetPanelBarActivePanelIds(MainWindowViewModel viewModel)
    {
        var ids = new List<string>(capacity: 2);

        if (!string.IsNullOrWhiteSpace(viewModel.ActiveLeftPanelId))
        {
            ids.Add(viewModel.ActiveLeftPanelId);
        }

        if (!string.IsNullOrWhiteSpace(viewModel.ActivePanelBarPanelId))
        {
            ids.Add(viewModel.ActivePanelBarPanelId);
        }

        return ids.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static void ApplyPanelBarDockSide(Grid rootGrid, PanelBar panelBar, PanelBarDockSide dockSide)
    {
        // Root grid is "Auto,*,Auto". Main layout stays in the center column.
        // PanelBar moves between column 0 and column 2.
        panelBar.SetValue(Grid.ColumnProperty, dockSide == PanelBarDockSide.Right ? 2 : 0);
    }

    private static double GetColumnWidth(GridLength columnWidth, double actualWidth)
    {
        var width = actualWidth > 0 ? actualWidth : columnWidth.Value;
        return width;
    }

    private static string GetPanelWidthKey(string panelId) => $"MainWindow.{panelId}.Width";

    private static double GetPanelDefaultWidth(string panelId, double fallback)
    {
        var descriptor = PanelRegistry.GetById(panelId);
        return descriptor?.DefaultWidth is double width && width > 0 ? width : fallback;
    }

    private static double LoadPanelWidthOrFallback(string panelId, string legacyZoneKey, double defaultWidth)
    {
        var width = PanelLayoutPersistence.LoadWidth(GetPanelWidthKey(panelId));
        if (width is double panelWidth)
        {
            return panelWidth;
        }

        var legacyWidth = PanelLayoutPersistence.LoadWidth(legacyZoneKey);
        if (legacyWidth is double zoneWidth)
        {
            return zoneWidth;
        }

        return GetPanelDefaultWidth(panelId, defaultWidth);
    }

    private void ApplyLeftZoneLayout(Grid mainGrid, MainWindowViewModel viewModel)
    {
        var leftColumn = mainGrid.ColumnDefinitions[0];
        var splitterColumn = mainGrid.ColumnDefinitions[1];

        if (string.IsNullOrWhiteSpace(viewModel.ActiveLeftPanelId))
        {
            leftColumn.Width = new GridLength(0, GridUnitType.Pixel);
            splitterColumn.Width = new GridLength(0, GridUnitType.Pixel);
            return;
        }

        var panelId = viewModel.ActiveLeftPanelId;
        var width = LoadPanelWidthOrFallback(panelId, legacyZoneKey: "MainWindow.LeftPanel", defaultWidth: 200);

        leftColumn.Width = new GridLength(width, GridUnitType.Pixel);
        splitterColumn.Width = new GridLength(4, GridUnitType.Pixel);
    }

    private void ApplyRightZoneLayout(Grid mainGrid, MainWindowViewModel viewModel)
    {
        var splitterColumn = mainGrid.ColumnDefinitions[3];
        var propertiesColumn = mainGrid.ColumnDefinitions[4];

        if (string.IsNullOrWhiteSpace(viewModel.ActivePanelBarPanelId))
        {
            propertiesColumn.Width = new GridLength(0, GridUnitType.Pixel);
            splitterColumn.Width = new GridLength(0, GridUnitType.Pixel);
            return;
        }

        var panelId = viewModel.ActivePanelBarPanelId;
        var width = LoadPanelWidthOrFallback(panelId, legacyZoneKey: "MainWindow.PropertiesPanel", defaultWidth: 280);

        propertiesColumn.Width = new GridLength(width, GridUnitType.Pixel);
        splitterColumn.Width = new GridLength(4, GridUnitType.Pixel);
    }

    private void PersistLeftZoneWidthIfNeeded(Grid mainGrid)
    {
        if (string.IsNullOrWhiteSpace(_previousActiveLeftPanelId))
        {
            return;
        }

        var leftColumn = mainGrid.ColumnDefinitions[0];
        var currentWidth = GetColumnWidth(leftColumn.Width, leftColumn.ActualWidth);
        if (currentWidth <= 0)
        {
            return;
        }

        PanelLayoutPersistence.UpdateColumnWidth(GetPanelWidthKey(_previousActiveLeftPanelId), currentWidth);
    }

    private void PersistRightZoneWidthIfNeeded(Grid mainGrid)
    {
        if (string.IsNullOrWhiteSpace(_previousActiveRightPanelId))
        {
            return;
        }

        var column = mainGrid.ColumnDefinitions[4];
        var currentWidth = GetColumnWidth(column.Width, column.ActualWidth);
        if (currentWidth <= 0)
        {
            return;
        }

        PanelLayoutPersistence.UpdateColumnWidth(GetPanelWidthKey(_previousActiveRightPanelId), currentWidth);
    }

    private void PersistActiveZoneWidths(Grid mainGrid, MainWindowViewModel viewModel)
    {
        if (!string.IsNullOrWhiteSpace(viewModel.ActiveLeftPanelId))
        {
            var leftColumn = mainGrid.ColumnDefinitions[0];
            var width = GetColumnWidth(leftColumn.Width, leftColumn.ActualWidth);
            if (width > 0)
            {
                PanelLayoutPersistence.UpdateColumnWidth(GetPanelWidthKey(viewModel.ActiveLeftPanelId), width);
            }
        }

        if (!string.IsNullOrWhiteSpace(viewModel.ActivePanelBarPanelId))
        {
            var rightColumn = mainGrid.ColumnDefinitions[4];
            var width = GetColumnWidth(rightColumn.Width, rightColumn.ActualWidth);
            if (width > 0)
            {
                PanelLayoutPersistence.UpdateColumnWidth(GetPanelWidthKey(viewModel.ActivePanelBarPanelId), width);
            }
        }
    }

    private void OnMotorNameLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorName(viewModel.MotorNameEditor);
        }
    }

    private void OnManufacturerLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorManufacturer(viewModel.ManufacturerEditor);
        }
    }

    private void OnPartNumberLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorPartNumber(viewModel.PartNumberEditor);
        }
    }

    /// <summary>
    /// Handles the drive max speed field losing focus to commit via command,
    /// optionally show confirmation dialog, and refresh the chart.
    /// </summary>
    private async void OnMaxSpeedLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel && viewModel.SelectedVoltage is not null)
        {
            // First, commit the edit through the undoable command path so it
            // participates consistently in undo/redo just like other fields.
            viewModel.EditSelectedVoltageMaxSpeed();

            var currentMaxSpeed = viewModel.SelectedVoltage.MaxSpeed;
            
            // Only show dialog if max speed actually changed
            if (Math.Abs(currentMaxSpeed - _previousMaxSpeed) > MaxSpeedChangeTolerance && _previousMaxSpeed > 0)
            {
                await viewModel.ConfirmMaxSpeedChangeAsync();
            }
            
            _previousMaxSpeed = currentMaxSpeed;

            // Refresh the chart to update the x-axis
            viewModel.ChartViewModel.RefreshChart();
        }
    }

    /// <summary>
    /// Handles the motor max speed field losing focus to commit via command and refresh chart.
    /// </summary>
    private void OnMotorMaxSpeedLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorMaxSpeed();
            viewModel.ChartViewModel.RefreshChart();
        }
    }

    private void OnHasBrakeChanged(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorHasBrake();
            viewModel.ChartViewModel.RefreshChart();
        }
    }

    private void OnBrakeTorqueLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorBrakeTorque();
            viewModel.ChartViewModel.RefreshChart();
        }
    }

    private void OnRatedSpeedLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorRatedSpeed();
        }
    }

    private void OnRatedPeakTorqueLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorRatedPeakTorque();
        }
    }

    private void OnRatedContinuousTorqueLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorRatedContinuousTorque();
        }
    }

    private void OnPowerLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorPower();
        }
    }

    private void OnWeightLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorWeight();
        }
    }

    private void OnRotorInertiaLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorRotorInertia();
        }
    }

    private void OnFeedbackPprLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorFeedbackPpr();
        }
    }

    private void OnBrakeAmperageLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorBrakeAmperage();
        }
    }

    private void OnBrakeVoltageLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditMotorBrakeVoltage();
        }
    }

    private void OnDriveNameLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditDriveName();
        }
    }

    private void OnDrivePartNumberLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditDrivePartNumber();
        }
    }

    private void OnDriveManufacturerLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditDriveManufacturer();
        }
    }

    private void OnVoltageValueLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditSelectedVoltageValue();
        }
    }

    private void OnVoltagePowerLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditSelectedVoltagePower();
        }
    }

    private void OnVoltagePeakTorqueLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditSelectedVoltagePeakTorque();
        }
    }

    private void OnVoltageContinuousTorqueLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditSelectedVoltageContinuousTorque();
        }
    }

    private void OnVoltageContinuousAmpsLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditSelectedVoltageContinuousAmps();
        }
    }

    private void OnVoltagePeakAmpsLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.EditSelectedVoltagePeakAmps();
        }
    }
}