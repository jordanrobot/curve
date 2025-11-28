using System;
using CurveEditor.Models;
using Xunit;

namespace CurveEditor.Tests.Models;

public class MotorDefinitionTests
{
    [Fact]
    public void Constructor_Default_CreatesEmptyDefinition()
    {
        var motor = new MotorDefinition();

        Assert.Equal(string.Empty, motor.MotorName);
        Assert.Empty(motor.Series);
        Assert.NotNull(motor.Units);
        Assert.NotNull(motor.Metadata);
    }

    [Fact]
    public void Constructor_WithName_CreatesDefinition()
    {
        var motor = new MotorDefinition("Test Motor");

        Assert.Equal("Test Motor", motor.MotorName);
    }

    [Fact]
    public void SchemaVersion_DefaultsTo1_0()
    {
        var motor = new MotorDefinition();

        Assert.Equal("1.0", motor.SchemaVersion);
    }

    [Fact]
    public void HasValidSeries_NoSeries_ReturnsFalse()
    {
        var motor = new MotorDefinition();

        Assert.False(motor.HasValidSeries());
    }

    [Fact]
    public void HasValidSeries_WithSeries_ReturnsTrue()
    {
        var motor = new MotorDefinition { MaxRpm = 5000 };
        motor.Series.Add(new CurveSeries("Peak"));

        Assert.True(motor.HasValidSeries());
    }

    [Fact]
    public void GetSeriesByName_ExistingSeries_ReturnsSeries()
    {
        var motor = new MotorDefinition();
        var series = new CurveSeries("Peak");
        motor.Series.Add(series);

        var result = motor.GetSeriesByName("Peak");

        Assert.Same(series, result);
    }

    [Fact]
    public void GetSeriesByName_CaseInsensitive_ReturnsSeries()
    {
        var motor = new MotorDefinition();
        motor.Series.Add(new CurveSeries("Peak"));

        var result = motor.GetSeriesByName("PEAK");

        Assert.NotNull(result);
        Assert.Equal("Peak", result.Name);
    }

    [Fact]
    public void GetSeriesByName_NonExistentSeries_ReturnsNull()
    {
        var motor = new MotorDefinition();
        motor.Series.Add(new CurveSeries("Peak"));

        var result = motor.GetSeriesByName("Continuous");

        Assert.Null(result);
    }

    [Fact]
    public void AddSeries_NewSeries_AddsAndInitializesSeries()
    {
        var motor = new MotorDefinition { MaxRpm = 5000 };

        var series = motor.AddSeries("Peak", 50);

        Assert.Single(motor.Series);
        Assert.Equal("Peak", series.Name);
        Assert.Equal(101, series.Data.Count);
    }

    [Fact]
    public void AddSeries_DuplicateName_ThrowsInvalidOperationException()
    {
        var motor = new MotorDefinition { MaxRpm = 5000 };
        motor.AddSeries("Peak", 50);

        var exception = Assert.Throws<InvalidOperationException>(() => motor.AddSeries("Peak", 45));
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public void AddSeries_UpdatesMetadataModified()
    {
        var motor = new MotorDefinition { MaxRpm = 5000 };
        var originalModified = motor.Metadata.Modified;

        motor.AddSeries("Peak", 50);

        Assert.True(motor.Metadata.Modified >= originalModified);
    }

    [Fact]
    public void RemoveSeries_ExistingSeries_RemovesSeries()
    {
        var motor = new MotorDefinition { MaxRpm = 5000 };
        motor.AddSeries("Peak", 55);
        motor.AddSeries("Continuous", 45);

        var result = motor.RemoveSeries("Peak");

        Assert.True(result);
        Assert.Single(motor.Series);
        Assert.Equal("Continuous", motor.Series[0].Name);
    }

    [Fact]
    public void RemoveSeries_LastSeries_ThrowsInvalidOperationException()
    {
        var motor = new MotorDefinition { MaxRpm = 5000 };
        motor.AddSeries("Peak", 55);

        var exception = Assert.Throws<InvalidOperationException>(() => motor.RemoveSeries("Peak"));
        Assert.Contains("Cannot remove the last series", exception.Message);
    }

    [Fact]
    public void RemoveSeries_NonExistentSeries_ReturnsFalse()
    {
        var motor = new MotorDefinition { MaxRpm = 5000 };
        motor.AddSeries("Peak", 55);
        motor.AddSeries("Continuous", 45);

        var result = motor.RemoveSeries("Custom");

        Assert.False(result);
    }

    [Fact]
    public void RemoveSeries_UpdatesMetadataModified()
    {
        var motor = new MotorDefinition { MaxRpm = 5000 };
        motor.AddSeries("Peak", 55);
        motor.AddSeries("Continuous", 45);
        var originalModified = motor.Metadata.Modified;

        motor.RemoveSeries("Peak");

        Assert.True(motor.Metadata.Modified >= originalModified);
    }

    [Fact]
    public void Units_DefaultValues_AreCorrect()
    {
        var motor = new MotorDefinition();

        Assert.Equal("Nm", motor.Units.Torque);
        Assert.Equal("rpm", motor.Units.Speed);
        Assert.Equal("W", motor.Units.Power);
        Assert.Equal("kg", motor.Units.Weight);
    }

    [Fact]
    public void Metadata_DefaultValues_AreSet()
    {
        var motor = new MotorDefinition();

        Assert.True(motor.Metadata.Created <= DateTime.UtcNow);
        Assert.True(motor.Metadata.Modified <= DateTime.UtcNow);
        Assert.Equal(string.Empty, motor.Metadata.Notes);
    }
}
