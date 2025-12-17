using System;
using CurveEditor.Services;
using Moq;
using Xunit;

namespace CurveEditor.Tests.Services;

public class UndoStackTests
{
    [Fact]
    public void PushAndExecute_AddsCommandToUndoStackAndClearsRedo()
    {
        var stack = new UndoStack();
        var command = new Mock<IUndoableCommand>();

        stack.PushAndExecute(command.Object);

        Assert.True(stack.CanUndo);
        Assert.False(stack.CanRedo);
        command.Verify(c => c.Execute(), Times.Once);
    }

    [Fact]
    public void Undo_MovesCommandToRedoStackAndCallsUndo()
    {
        var stack = new UndoStack();
        var command = new Mock<IUndoableCommand>();

        stack.PushAndExecute(command.Object);
        stack.Undo();

        Assert.False(stack.CanUndo);
        Assert.True(stack.CanRedo);
        command.Verify(c => c.Undo(), Times.Once);
    }

    [Fact]
    public void Redo_ReExecutesCommandAndMovesBackToUndoStack()
    {
        var stack = new UndoStack();
        var command = new Mock<IUndoableCommand>();

        stack.PushAndExecute(command.Object);
        stack.Undo();
        stack.Redo();

        Assert.True(stack.CanUndo);
        Assert.False(stack.CanRedo);
        command.Verify(c => c.Execute(), Times.Exactly(2));
    }

    [Fact]
    public void Undo_WhenEmpty_DoesNothing()
    {
        var stack = new UndoStack();

        stack.Undo();

        Assert.False(stack.CanUndo);
        Assert.False(stack.CanRedo);
    }

    [Fact]
    public void Redo_WhenEmpty_DoesNothing()
    {
        var stack = new UndoStack();

        stack.Redo();

        Assert.False(stack.CanUndo);
        Assert.False(stack.CanRedo);
    }
}
