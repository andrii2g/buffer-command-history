using BufferCommandHistory.Text;

namespace BufferCommandHistory.Tests.Text;

public sealed class DeleteCharCommandTests
{
    [Fact]
    public void ConstructorRejectsNullBuffer()
    {
        Assert.Throws<ArgumentNullException>(() => new DeleteCharCommand(null!, 0));
    }

    [Fact]
    public void ExecuteRemovesCorrectCharacter()
    {
        var buffer = CreateBuffer("ABC");
        var command = new DeleteCharCommand(buffer, 1);

        command.Execute();

        Assert.Equal("AC", buffer.GetText());
    }

    [Fact]
    public void UndoRestoresExactCharacter()
    {
        var buffer = CreateBuffer("ABC");
        var command = new DeleteCharCommand(buffer, 1);
        command.Execute();

        command.Undo();

        Assert.Equal("ABC", buffer.GetText());
    }

    [Fact]
    public void ExecuteUndoExecuteCycleRefreshesDeletedCharacter()
    {
        var buffer = CreateBuffer("ABC");
        var command = new DeleteCharCommand(buffer, 1);

        command.Execute();
        command.Undo();
        command.Execute();

        Assert.Equal("AC", buffer.GetText());
    }

    [Theory]
    [InlineData(0, "BC")]
    [InlineData(2, "AB")]
    public void ExecuteDeletesAtBufferEdges(int index, string expected)
    {
        var buffer = CreateBuffer("ABC");
        var command = new DeleteCharCommand(buffer, index);

        command.Execute();

        Assert.Equal(expected, buffer.GetText());
    }

    [Fact]
    public void DescriptionIdentifiesPosition()
    {
        var command = new DeleteCharCommand(new TextBuffer(), 3);

        Assert.Equal("Delete char at 3", command.Description);
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
