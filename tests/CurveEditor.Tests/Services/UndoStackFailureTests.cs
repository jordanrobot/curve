using System;
using CurveEditor.Services;
using Xunit;

namespace CurveEditor.Tests.Services;

public sealed class UndoStackFailureTests
{
    private sealed class FailingCommand : IUndoableCommand
    {
        private readonly bool _failOnExecute;
        private readonly bool _failOnUndo;

        public FailingCommand(bool failOnExecute = false, bool failOnUndo = false)
        {
            _failOnExecute = failOnExecute;
            _failOnUndo = failOnUndo;
        }

        public string Description => "Failing command for testing";

        public void Execute()
        {
            if (_failOnExecute)
            {
                throw new InvalidOperationException("Execute failure");
            }
        }

        public void Undo()
        {
            if (_failOnUndo)
            {
                throw new InvalidOperationException("Undo failure");
            }
        }
    }

    [Fact]
    public void PushAndExecute_WhenExecuteThrows_DoesNotChangeStacks()
    {
        var stack = new UndoStack();
        var command = new FailingCommand(failOnExecute: true);

        Assert.Equal(0, stack.UndoDepth);
        Assert.False(stack.CanUndo);
        Assert.False(stack.CanRedo);

        Assert.Throws<InvalidOperationException>(() => stack.PushAndExecute(command));

        Assert.Equal(0, stack.UndoDepth);
        Assert.False(stack.CanUndo);
        Assert.False(stack.CanRedo);
    }

    [Fact]
    public void Undo_WhenUndoThrows_KeepsCommandOnUndoStack()
    {
        var stack = new UndoStack();
        var command = new FailingCommand(failOnUndo: true);

        // First execution should succeed.
        stack.PushAndExecute(command);
        Assert.Equal(1, stack.UndoDepth);
        Assert.True(stack.CanUndo);
        Assert.False(stack.CanRedo);

        Assert.Throws<InvalidOperationException>(() => stack.Undo());

        // Command should be placed back on the undo stack.
        Assert.Equal(1, stack.UndoDepth);
        Assert.True(stack.CanUndo);
        Assert.False(stack.CanRedo);
    }

    [Fact]
    public void Redo_WhenExecuteThrows_KeepsCommandOnRedoStack()
    {
        var stack = new UndoStack();
        var failingOnExecute = new FailingCommand(failOnExecute: true);

        // Manually push the failing command onto the redo stack via reflection
        // to avoid changing the public API just for tests.
        var redoField = typeof(UndoStack).GetField("_redoStack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (redoField is null)
        {
            throw new InvalidOperationException("Could not access _redoStack field via reflection.");
        }

        if (redoField.GetValue(stack) is not System.Collections.Generic.Stack<IUndoableCommand> redoStack)
        {
            throw new InvalidOperationException("_redoStack field is not of expected type.");
        }

        redoStack.Clear();
        redoStack.Push(failingOnExecute);

        Assert.Throws<InvalidOperationException>(() => stack.Redo());

        // Command should still be present on the redo stack.
        Assert.False(stack.CanUndo);
        Assert.True(stack.CanRedo);
    }
}
