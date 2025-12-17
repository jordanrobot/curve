using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using CurveEditor.Models;

namespace CurveEditor.Views;

public partial class PanelBar : UserControl
{
    public PanelBar()
    {
        InitializeComponent();
        PanelClickCommand = new RelayCommand<string>(OnPanelClick);
        
        // Set the panel items from the registry
        var items = this.FindControl<ItemsControl>("PanelItems");
        if (items is not null)
        {
            items.ItemsSource = PanelRegistry.PanelBarPanels;
        }
    }

    public ICommand PanelClickCommand { get; }

    private string? _activePanelId;
    public string? ActivePanelId 
    { 
        get => _activePanelId;
        set
        {
            if (_activePanelId != value)
            {
                _activePanelId = value;
                UpdateButtonStyles();
            }
        }
    }

    public event EventHandler<string>? PanelClicked;

    private void OnPanelClick(string? panelId)
    {
        if (panelId is not null)
        {
            PanelClicked?.Invoke(this, panelId);
        }
    }

    private void UpdateButtonStyles()
    {
        var items = this.FindControl<ItemsControl>("PanelItems");
        if (items is null)
        {
            return;
        }

        // Find all buttons in the visual tree and update their classes
        var buttons = items.GetVisualDescendants().OfType<Button>();
        foreach (var button in buttons)
        {
            var panelId = button.CommandParameter as string;
            var isActive = string.Equals(panelId, _activePanelId, StringComparison.Ordinal);
            
            if (isActive && !button.Classes.Contains("Active"))
            {
                button.Classes.Add("Active");
            }
            else if (!isActive && button.Classes.Contains("Active"))
            {
                button.Classes.Remove("Active");
            }
        }
    }
}

