using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CurveEditor.Models;

/// <summary>
/// Represents a complete motor definition including all properties, curves, and metadata.
/// </summary>
public class MotorDefinition
{
    /// <summary>
    /// Schema version for JSON compatibility.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = "1.0";

    // Motor Identification
    /// <summary>
    /// The model name or identifier for the motor.
    /// </summary>
    [JsonPropertyName("motorName")]
    public string MotorName { get; set; } = string.Empty;

    /// <summary>
    /// The company that manufactures the motor.
    /// </summary>
    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// The manufacturer's part number for the motor.
    /// </summary>
    [JsonPropertyName("partNumber")]
    public string PartNumber { get; set; } = string.Empty;

    // Drive Information
    /// <summary>
    /// The name of the servo drive used with the motor.
    /// </summary>
    [JsonPropertyName("driveName")]
    public string DriveName { get; set; } = string.Empty;

    /// <summary>
    /// The manufacturer's part number for the servo drive.
    /// </summary>
    [JsonPropertyName("drivePartNumber")]
    public string DrivePartNumber { get; set; } = string.Empty;

    // Electrical Properties
    /// <summary>
    /// The operating voltage for the motor/drive combination (V).
    /// </summary>
    [JsonPropertyName("voltage")]
    public double Voltage { get; set; }

    /// <summary>
    /// The current draw during continuous operation at rated torque (A).
    /// </summary>
    [JsonPropertyName("continuousAmperage")]
    public double ContinuousAmperage { get; set; }

    /// <summary>
    /// The maximum current draw during peak torque operation (A).
    /// </summary>
    [JsonPropertyName("peakAmperage")]
    public double PeakAmperage { get; set; }

    /// <summary>
    /// The power output of the motor (in the unit specified by Units.Power).
    /// </summary>
    [JsonPropertyName("power")]
    public double Power { get; set; }

    // Speed Properties
    /// <summary>
    /// The maximum rotational speed of the motor (RPM).
    /// </summary>
    [JsonPropertyName("maxRpm")]
    public double MaxRpm { get; set; }

    /// <summary>
    /// The rated continuous operating speed of the motor (RPM).
    /// </summary>
    [JsonPropertyName("ratedRpm")]
    public double RatedRpm { get; set; }

    // Torque Properties
    /// <summary>
    /// The torque the motor can produce continuously without overheating.
    /// </summary>
    [JsonPropertyName("ratedContinuousTorque")]
    public double RatedContinuousTorque { get; set; }

    /// <summary>
    /// The maximum torque the motor can produce for short periods.
    /// </summary>
    [JsonPropertyName("ratedPeakTorque")]
    public double RatedPeakTorque { get; set; }

    // Mechanical Properties
    /// <summary>
    /// The mass of the motor (in the unit specified by Units.Weight).
    /// </summary>
    [JsonPropertyName("weight")]
    public double Weight { get; set; }

    /// <summary>
    /// The moment of inertia of the motor's rotor, affecting acceleration response.
    /// </summary>
    [JsonPropertyName("rotorInertia")]
    public double RotorInertia { get; set; }

    // Brake Properties
    /// <summary>
    /// Indicates whether the motor includes an integral holding brake.
    /// </summary>
    [JsonPropertyName("hasBrake")]
    public bool HasBrake { get; set; }

    /// <summary>
    /// The holding torque of the integral brake (if present).
    /// </summary>
    [JsonPropertyName("brakeTorque")]
    public double BrakeTorque { get; set; }

    /// <summary>
    /// The current draw of the brake (if present) (A).
    /// </summary>
    [JsonPropertyName("brakeAmperage")]
    public double BrakeAmperage { get; set; }

    // Units Configuration
    /// <summary>
    /// The unit settings for this motor definition.
    /// </summary>
    [JsonPropertyName("units")]
    public UnitSettings Units { get; set; } = new();

    // Curve Series
    /// <summary>
    /// The collection of curve series (e.g., "Peak", "Continuous").
    /// </summary>
    [JsonPropertyName("series")]
    public List<CurveSeries> Series { get; set; } = [];

    // Metadata
    /// <summary>
    /// Metadata about the motor definition file.
    /// </summary>
    [JsonPropertyName("metadata")]
    public MotorMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Creates a new MotorDefinition with default values.
    /// </summary>
    public MotorDefinition()
    {
    }

    /// <summary>
    /// Creates a new MotorDefinition with the specified motor name.
    /// </summary>
    /// <param name="motorName">The name of the motor.</param>
    public MotorDefinition(string motorName)
    {
        MotorName = motorName;
    }

    /// <summary>
    /// Validates that the motor definition has at least one series.
    /// </summary>
    /// <returns>True if valid; otherwise false.</returns>
    public bool HasValidSeries()
    {
        return Series.Count > 0;
    }

    /// <summary>
    /// Gets a curve series by name.
    /// </summary>
    /// <param name="name">The name of the series to find.</param>
    /// <returns>The matching series, or null if not found.</returns>
    public CurveSeries? GetSeriesByName(string name)
    {
        return Series.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Adds a new series with the specified name.
    /// </summary>
    /// <param name="name">The name for the new series.</param>
    /// <param name="initializeTorque">The default torque value for all points.</param>
    /// <returns>The newly created series.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a series with the same name already exists.</exception>
    public CurveSeries AddSeries(string name, double initializeTorque = 0)
    {
        if (GetSeriesByName(name) is not null)
        {
            throw new InvalidOperationException($"A series with the name '{name}' already exists.");
        }

        var series = new CurveSeries(name);
        series.InitializeData(MaxRpm, initializeTorque);
        Series.Add(series);
        Metadata.UpdateModified();
        return series;
    }

    /// <summary>
    /// Removes a series by name.
    /// </summary>
    /// <param name="name">The name of the series to remove.</param>
    /// <returns>True if removed; false if not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if attempting to remove the last series.</exception>
    public bool RemoveSeries(string name)
    {
        if (Series.Count <= 1)
        {
            throw new InvalidOperationException("Cannot remove the last series. At least one series must exist.");
        }

        var series = GetSeriesByName(name);
        if (series is null)
        {
            return false;
        }

        Series.Remove(series);
        Metadata.UpdateModified();
        return true;
    }
}
