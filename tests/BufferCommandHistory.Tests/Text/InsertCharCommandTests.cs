using BufferCommandHistory.Text;

namespace BufferCommandHistory.Tests.Text;

public sealed class InsertCharCommandTests
{
    [Fact]
    public void ConstructorRejectsNullBuffer()
    {
        Assert.Throws<ArgumentNullException>(() => new InsertCharCommand(null!, 0, 'A'));
    }

    [Fact]
    public void ExecuteInsertsCharacter()
    {
        var buffer = CreateBuffer("AC");
        var command = new InsertCharCommand(buffer, 1, 'B');

        command.Execute();

        Assert.Equal("ABC", buffer.GetText());
    }

    [Fact]
    public void UndoRemovesInsertedCharacter()
    {
        var buffer = CreateBuffer("AC");
        var command = new InsertCharCommand(buffer, 1, 'B');
        command.Execute();

        command.Undo();

        Assert.Equal("AC", buffer.GetText());
    }

    [Fact]
    public void ExecuteUndoExecuteCycleWorks()
    {
        var buffer = new TextBuffer();
        var command = new InsertCharCommand(buffer, 0, 'A');

        command.Execute();
        command.Undo();
        command.Execute();

        Assert.Equal("A", buffer.GetText());
    }

    [Fact]
    public void DescriptionIdentifiesValueAndPosition()
    {
        var command = new InsertCharCommand(new TextBuffer(), 4, 'X');

        Assert.Equal("Insert 'X' at 4", command.Description);
    }

    private static TextBuffer CreateBuffer(string text)
    {
        var buffer = new TextBuffer();
        foreach (var character in text)
        {
            buffer.Insert(buffer.Length, character);
        }

        return buffer;
    }
}
