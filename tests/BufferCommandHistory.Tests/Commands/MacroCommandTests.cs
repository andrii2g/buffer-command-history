using BufferCommandHistory.Commands;
using BufferCommandHistory.History;
using BufferCommandHistory.Text;

namespace BufferCommandHistory.Tests.Commands;

public sealed class MacroCommandTests
{
    [Fact]
    public void ExecuteInvokesChildrenInForwardOrder()
    {
        var calls = new List<string>();
        var macro = new MacroCommand(
            "Forward",
            [new RecordingCommand("A", calls), new RecordingCommand("B", calls), new RecordingCommand("C", calls)]);

        macro.Execute();

        Assert.Equal(["Execute A", "Execute B", "Execute C"], calls);
    }

    [Fact]
    public void UndoInvokesChildrenInReverseOrder()
    {
        var calls = new List<string>();
        var macro = new MacroCommand(
            "Reverse",
            [new RecordingCommand("A", calls), new RecordingCommand("B", calls), new RecordingCommand("C", calls)]);
        macro.Execute();
        calls.Clear();

        macro.Undo();

        Assert.Equal(["Undo C", "Undo B", "Undo A"], calls);
    }

    [Fact]
    public void MacroCountsAsOneHistoryEntry()
    {
        var buffer = new TextBuffer();
        var macro = new MacroCommand(
            "Insert ABC",
            [
                new InsertCharCommand(buffer, 0, 'A'),
                new InsertCharCommand(buffer, 1, 'B'),
                new InsertCharCommand(buffer, 2, 'C'),
            ]);
        var history = new CommandHistory();

        history.Execute(macro);

        Assert.Equal("ABC", buffer.GetText());
        Assert.Equal(1, history.UndoCount);
        Assert.Equal(["Insert ABC"], history.GetUndoDescriptions());

        Assert.True(history.Undo());
        Assert.Equal(string.Empty, buffer.GetText());
        Assert.Equal(1, history.RedoCount);

        Assert.True(history.Redo());
        Assert.Equal("ABC", buffer.GetText());
    }

    [Fact]
    public void CanContainCommandsFromDifferentDomains()
    {
        var calls = new List<string>();
        var buffer = new TextBuffer();
        var macro = new MacroCommand(
            "Mixed commands",
            [new RecordingCommand("Non-text", calls), new InsertCharCommand(buffer, 0, 'X')]);

        macro.Execute();

        Assert.Equal(["Execute Non-text"], calls);
        Assert.Equal("X", buffer.GetText());
    }

    [Fact]
    public void ConstructorMaterializesCommandEnumerationOnce()
    {
        var enumerationCount = 0;
        var command = new RecordingCommand("A", []);

        IEnumerable<ICommand> Commands()
        {
            enumerationCount++;
            yield return command;
        }

        var macro = new MacroCommand("Once", Commands());
        macro.Execute();

        Assert.Equal(1, enumerationCount);
    }

    [Fact]
    public void ConstructorRejectsNullDescription()
    {
        Assert.Throws<ArgumentNullException>(() => new MacroCommand(null!, [new RecordingCommand("A", [])]));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ConstructorRejectsBlankDescription(string description)
    {
        Assert.Throws<ArgumentException>(() => new MacroCommand(description, [new RecordingCommand("A", [])]));
    }

    [Fact]
    public void ConstructorRejectsNullCommandCollection()
    {
        Assert.Throws<ArgumentNullException>(() => new MacroCommand("Null", null!));
    }

    [Fact]
    public void ConstructorRejectsNullChild()
    {
        Assert.Throws<ArgumentException>(() => new MacroCommand("Null child", [null!]));
    }

    [Fact]
    public void ConstructorRejectsEmptyCommandCollection()
    {
        Assert.Throws<ArgumentException>(() => new MacroCommand("Empty", []));
    }

    [Fact]
    public void ChildExecutionFailureIsPropagated()
    {
        var macro = new MacroCommand("Failure", [new ThrowingCommand()]);

        Assert.Throws<InvalidOperationException>(() => macro.Execute());
    }

    private sealed class RecordingCommand(string name, List<string> calls) : ICommand
    {
        public string Description => name;

        public void Execute() => calls.Add($"Execute {name}");

        public void Undo() => calls.Add($"Undo {name}");
    }

    private sealed class ThrowingCommand : ICommand
    {
        public string Description => "Throw";

        public void Execute() => throw new InvalidOperationException("Child execution failed.");

        public void Undo()
        {
        }
    }
}
