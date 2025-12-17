using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls;
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

    public string? ActivePanelId { get; set; }

    public event EventHandler<string>? PanelClicked;

    private void OnPanelClick(string? panelId)
    {
        if (panelId is not null)
        {
            PanelClicked?.Invoke(this, panelId);
        }
    }
}
