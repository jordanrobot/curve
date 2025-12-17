using System;
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
    private double _leftColumnWidth;
    private double _propertiesColumnWidth;
    private bool _leftPanelExpanded;
    private bool _propertiesPanelExpanded;

    public MainWindow()
    {
        InitializeComponent();
        WindowBoundsPersistence.Attach(this, "MainWindow");
        Opened += OnOpened;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Opened -= OnOpened;

        var mainGrid = this.FindControl<Grid>("MainLayoutGrid");
        var centerGrid = this.FindControl<Grid>("CenterGrid");
        var panelBar = this.FindControl<PanelBar>("PanelBarControl");

        if (mainGrid is null || centerGrid is null || DataContext is not MainWindowViewModel viewModel)
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

            // Update PanelBar when active panel changes in either zone
            viewModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MainWindowViewModel.ActivePanelBarPanelId) ||
                    args.PropertyName == nameof(MainWindowViewModel.ActiveLeftPanelId))
                {
                    // Update active panel ID for visual highlighting
                    // PanelBar needs to know about both zones
                    var activeId = viewModel.ActivePanelBarPanelId ?? viewModel.ActiveLeftPanelId;
                    panelBar.ActivePanelId = activeId;
                }
            };

            // Set initial state
            var initialActiveId = viewModel.ActivePanelBarPanelId ?? viewModel.ActiveLeftPanelId;
            panelBar.ActivePanelId = initialActiveId;
        }

        // Left zone panel (column 0)
        PanelLayoutPersistence.AttachColumn(this, mainGrid, 0, "MainWindow.LeftPanel");

        // Right properties panel (column 4)
        PanelLayoutPersistence.AttachColumn(this, mainGrid, 4, "MainWindow.PropertiesPanel");

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

        // Save panel state on window closing
        Closing += (_, _) =>
        {
            PanelLayoutPersistence.SaveString("MainWindow.ActivePanelBarPanelId", viewModel.ActivePanelBarPanelId);
            PanelLayoutPersistence.SaveString("MainWindow.ActiveLeftPanelId", viewModel.ActiveLeftPanelId);
            PanelLayoutPersistence.SaveDockSide("MainWindow.PanelBarDockSide", viewModel.PanelBarDockSide);
        };

        // Capture initial sizes (after any persisted values are restored)
        _leftColumnWidth = mainGrid.ColumnDefinitions[0].Width.Value;
        _propertiesColumnWidth = mainGrid.ColumnDefinitions[4].Width.Value;

        _leftPanelExpanded = viewModel.IsBrowserPanelExpanded || viewModel.IsCurveDataExpanded;
        _propertiesPanelExpanded = viewModel.IsPropertiesPanelExpanded;

        // React to panel expanded/collapsed properties changing.
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.IsBrowserPanelExpanded) ||
                args.PropertyName == nameof(MainWindowViewModel.IsCurveDataExpanded))
            {
                ApplyLeftPanelLayout(mainGrid, viewModel, _leftPanelExpanded);
                _leftPanelExpanded = viewModel.IsBrowserPanelExpanded || viewModel.IsCurveDataExpanded;
            }
            else if (args.PropertyName == nameof(MainWindowViewModel.IsPropertiesPanelExpanded))
            {
                ApplyPropertiesPanelLayout(mainGrid, viewModel, _propertiesPanelExpanded);
                _propertiesPanelExpanded = viewModel.IsPropertiesPanelExpanded;
            }
        };

        // Apply initial layouts based on current expanded/collapsed state.
        ApplyLeftPanelLayout(mainGrid, viewModel, _leftPanelExpanded);
        ApplyPropertiesPanelLayout(mainGrid, viewModel, _propertiesPanelExpanded);
    }

    private void ApplyLeftPanelLayout(Grid mainGrid, MainWindowViewModel viewModel, bool wasExpanded)
    {
        var leftColumn = mainGrid.ColumnDefinitions[0];
        var splitterColumn = mainGrid.ColumnDefinitions[1];

        var isExpanded = viewModel.IsBrowserPanelExpanded || viewModel.IsCurveDataExpanded;

        if (isExpanded)
        {
            if (_leftColumnWidth <= 0)
            {
                var current = leftColumn.ActualWidth > 0 ? leftColumn.ActualWidth : leftColumn.Width.Value;
                _leftColumnWidth = current > 0 ? current : 200;
            }

            leftColumn.Width = new GridLength(_leftColumnWidth, GridUnitType.Pixel);
            splitterColumn.Width = new GridLength(4, GridUnitType.Pixel);
        }
        else
        {
            if (wasExpanded)
            {
                var current = leftColumn.ActualWidth > 0 ? leftColumn.ActualWidth : leftColumn.Width.Value;
                if (current > 0)
                {
                    _leftColumnWidth = current;
                    PanelLayoutPersistence.UpdateColumnWidth("MainWindow.LeftPanel", _leftColumnWidth);
                }
            }

            leftColumn.Width = new GridLength(0, GridUnitType.Pixel);
            splitterColumn.Width = new GridLength(0, GridUnitType.Pixel);
        }
    }

    private void ApplyPropertiesPanelLayout(Grid mainGrid, MainWindowViewModel viewModel, bool wasExpanded)
    {
        var splitterColumn = mainGrid.ColumnDefinitions[3];
        var propertiesColumn = mainGrid.ColumnDefinitions[4];

        var isExpanded = viewModel.IsPropertiesPanelExpanded;

        if (isExpanded)
        {
            if (_propertiesColumnWidth <= 0)
            {
                var current = propertiesColumn.ActualWidth > 0 ? propertiesColumn.ActualWidth : propertiesColumn.Width.Value;
                _propertiesColumnWidth = current > 0 ? current : 280;
            }

            propertiesColumn.Width = new GridLength(_propertiesColumnWidth, GridUnitType.Pixel);
            splitterColumn.Width = new GridLength(4, GridUnitType.Pixel);
        }
        else
        {
            if (wasExpanded)
            {
                var current = propertiesColumn.ActualWidth > 0 ? propertiesColumn.ActualWidth : propertiesColumn.Width.Value;
                if (current > 0)
                {
                    _propertiesColumnWidth = current;
                    PanelLayoutPersistence.UpdateColumnWidth("MainWindow.PropertiesPanel", _propertiesColumnWidth);
                }
            }

            propertiesColumn.Width = new GridLength(0, GridUnitType.Pixel);
            splitterColumn.Width = new GridLength(0, GridUnitType.Pixel);
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