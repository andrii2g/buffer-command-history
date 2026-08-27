using BufferCommandHistory.Commands;
using BufferCommandHistory.History;

namespace BufferCommandHistory.Tests.History;

public sealed class CommandHistoryTests
{
    [Fact]
    public void StartsEmpty()
    {
        var history = new CommandHistory();

        Assert.False(history.CanUndo);
        Assert.False(history.CanRedo);
        Assert.Equal(0, history.UndoCount);
        Assert.Equal(0, history.RedoCount);
        Assert.Empty(history.GetUndoDescriptions());
        Assert.Empty(history.GetRedoDescriptions());
    }

    [Fact]
    public void ExecuteRejectsNullCommand()
    {
        var history = new CommandHistory();

        Assert.Throws<ArgumentNullException>(() => history.Execute(null!));
    }

    [Fact]
    public void ExecuteRunsNonTextCommandAndAddsUndoEntry()
    {
        var value = new MutableInteger();
        var history = new CommandHistory();

        history.Execute(new AddCommand(value, 5, "Add five"));

        Assert.Equal(5, value.Value);
        Assert.True(history.CanUndo);
        Assert.Equal(1, history.UndoCount);
        Assert.Equal(["Add five"], history.GetUndoDescriptions());
    }

    [Fact]
    public void UndoMovesCommandToRedoStack()
    {
        var value = new MutableInteger();
        var history = new CommandHistory();
        history.Execute(new AddCommand(value, 3, "Add three"));

        Assert.True(history.Undo());

        Assert.Equal(0, value.Value);
        Assert.Equal(0, history.UndoCount);
        Assert.Equal(1, history.RedoCount);
        Assert.Equal(["Add three"], history.GetRedoDescriptions());
    }

    [Fact]
    public void RedoExecutesCommandAndMovesItBackToUndoStack()
    {
        var value = new MutableInteger();
        var history = new CommandHistory();
        history.Execute(new AddCommand(value, 7, "Add seven"));
        history.Undo();

        Assert.True(history.Redo());

        Assert.Equal(7, value.Value);
        Assert.Equal(1, history.UndoCount);
        Assert.Equal(0, history.RedoCount);
    }

    [Fact]
    public void MultipleUndoAndRedoOperationsUseLifoOrder()
    {
        var value = new MutableInteger();
        var history = new CommandHistory();
        history.Execute(new AddCommand(value, 1, "A"));
        history.Execute(new AddCommand(value, 10, "B"));
        history.Execute(new AddCommand(value, 100, "C"));

        history.Undo();
        history.Undo();

        Assert.Equal(1, value.Value);
        Assert.Equal(["A"], history.GetUndoDescriptions());
        Assert.Equal(["B", "C"], history.GetRedoDescriptions());

        history.Redo();

        Assert.Equal(11, value.Value);
        Assert.Equal(["B", "A"], history.GetUndoDescriptions());
        Assert.Equal(["C"], history.GetRedoDescriptions());
    }

    [Fact]
    public void UndoAndRedoReturnFalseWhenTheirStacksAreEmpty()
    {
        var history = new CommandHistory();

        Assert.False(history.Undo());
        Assert.False(history.Redo());
    }

    [Fact]
    public void ExecuteAfterUndoClearsAllRedoEntries()
    {
        var value = new MutableInteger();
        var history = new CommandHistory();
        history.Execute(new AddCommand(value, 1, "A"));
        history.Execute(new AddCommand(value, 2, "B"));
        history.Execute(new AddCommand(value, 3, "C"));
        history.Undo();
        history.Undo();

        history.Execute(new AddCommand(value, 10, "X"));

        Assert.Equal(11, value.Value);
        Assert.Equal(2, history.UndoCount);
        Assert.False(history.CanRedo);
        Assert.Equal(["X", "A"], history.GetUndoDescriptions());
    }

    [Fact]
    public void FailedExecuteDoesNotChangeStacksOrInvalidateRedo()
    {
        var value = new MutableInteger();
        var history = new CommandHistory();
        history.Execute(new AddCommand(value, 1, "A"));
        history.Undo();

        Assert.Throws<InvalidOperationException>(() =>
            history.Execute(new ThrowingCommand("Failure", throwOnExecute: true)));

        Assert.Equal(0, history.UndoCount);
        Assert.Equal(1, history.RedoCount);
        Assert.Equal(["A"], history.GetRedoDescriptions());
    }

    [Fact]
    public void FailedUndoPreservesCommandOnUndoStack()
    {
        var history = new CommandHistory();
        history.Execute(new ThrowingCommand("Failure", throwOnUndo: true));

        Assert.Throws<InvalidOperationException>(() => history.Undo());

        Assert.Equal(1, history.UndoCount);
        Assert.Equal(0, history.RedoCount);
        Assert.Equal(["Failure"], history.GetUndoDescriptions());
    }

    [Fact]
    public void FailedRedoPreservesCommandOnRedoStack()
    {
        var command = new ThrowingCommand("Failure");
        var history = new CommandHistory();
        history.Execute(command);
        history.Undo();
        command.ThrowOnExecute = true;

        Assert.Throws<InvalidOperationException>(() => history.Redo());

        Assert.Equal(0, history.UndoCount);
        Assert.Equal(1, history.RedoCount);
        Assert.Equal(["Failure"], history.GetRedoDescriptions());
    }

    [Fact]
    public void DescriptionResultsAreSnapshots()
    {
        var value = new MutableInteger();
        var history = new CommandHistory();
        history.Execute(new AddCommand(value, 1, "A"));
        var snapshot = history.GetUndoDescriptions();

        history.Execute(new AddCommand(value, 2, "B"));

        Assert.Equal(["A"], snapshot);
        Assert.Equal(["B", "A"], history.GetUndoDescriptions());
    }

    private sealed class MutableInteger
    {
        public int Value { get; set; }
    }

    private sealed class AddCommand(MutableInteger target, int amount, string description) : ICommand
    {
        public string Description { get; } = description;

        public void Execute() => target.Value += amount;

        public void Undo() => target.Value -= amount;
    }

    private sealed class ThrowingCommand(
        string description,
        bool throwOnExecute = false,
        bool throwOnUndo = false) : ICommand
    {
        public string Description { get; } = description;

        public bool ThrowOnExecute { get; set; } = throwOnExecute;

        public bool ThrowOnUndo { get; set; } = throwOnUndo;

        public void Execute()
        {
            if (ThrowOnExecute)
            {
                throw new InvalidOperationException("Execute failed.");
            }
        }

        public void Undo()
        {
            if (ThrowOnUndo)
            {
                throw new InvalidOperationException("Undo failed.");
            }
        }
    }
}
