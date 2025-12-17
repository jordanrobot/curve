using System.Collections.Generic;
using System.Linq;

namespace CurveEditor.Models;

/// <summary>
/// Registry of all panel descriptors in the application.
/// Provides a central location for panel configuration.
/// </summary>
public static class PanelRegistry
{
    /// <summary>
    /// Stable panel IDs (must not change after release for persistence compatibility).
    /// </summary>
    public static class PanelIds
    {
        public const string DirectoryBrowser = "DirectoryBrowser";
        public const string MotorProperties = "MotorProperties";
        public const string CurveData = "CurveData";
        public const string CurveGraph = "CurveGraph";
    }

    /// <summary>
    /// All panel descriptors in the application.
    /// </summary>
    public static IReadOnlyList<PanelDescriptor> AllPanels { get; } = new List<PanelDescriptor>
    {
        new PanelDescriptor
        {
            PanelId = PanelIds.DirectoryBrowser,
            DisplayName = "Directory Browser",
            EnableIcon = true,
            EnableCollapse = true,
            Zone = PanelZone.Left,
            DefaultWidth = 200,
            IconGlyph = "📁"
        },
        new PanelDescriptor
        {
            PanelId = PanelIds.MotorProperties,
            DisplayName = "Motor Properties",
            EnableIcon = true,
            EnableCollapse = true,
            Zone = PanelZone.Right,
            DefaultWidth = 280,
            IconGlyph = "⚙"
        },
        new PanelDescriptor
        {
            PanelId = PanelIds.CurveData,
            DisplayName = "Curve Data",
            EnableIcon = true,
            EnableCollapse = true,
            Zone = PanelZone.Bottom,
            DefaultHeight = 250,
            IconGlyph = "📊"
        },
        new PanelDescriptor
        {
            PanelId = PanelIds.CurveGraph,
            DisplayName = "Curve Graph",
            EnableIcon = false,
            EnableCollapse = false,
            Zone = PanelZone.Center,
            MinSize = 400
        }
    };

    /// <summary>
    /// Gets panels that should appear in the Panel Bar.
    /// </summary>
    public static IEnumerable<PanelDescriptor> PanelBarPanels =>
        AllPanels.Where(p => p.EnableIcon);

    /// <summary>
    /// Gets a panel descriptor by its ID.
    /// </summary>
    public static PanelDescriptor? GetById(string panelId) =>
        AllPanels.FirstOrDefault(p => p.PanelId == panelId);
}
