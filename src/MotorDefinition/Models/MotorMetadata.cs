using System;
using System.Text.Json.Serialization;

namespace JordanRobot.MotorDefinition.Model;

/// <summary>
/// Contains metadata about the motor definition file.
/// </summary>
public class MotorMetadata
{
    /// <summary>
    /// Gets or sets the date and time when the motor definition was created.
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the motor definition was last modified.
    /// </summary>
    [JsonPropertyName("modified")]
    public DateTime Modified { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets notes about the motor definition.
    /// </summary>
    /// <remarks>
    /// This can include test conditions or other free-form information.
    /// </remarks>
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Updates the modified timestamp to the current UTC time.
    /// </summary>
    public void UpdateModified()
    {
        Modified = DateTime.UtcNow;
    }
}
