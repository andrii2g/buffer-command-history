using BufferCommandHistory.Text;

namespace BufferCommandHistory.Tests.Text;

public sealed class TextBufferTests
{
    [Fact]
    public void StartsEmpty()
    {
        var buffer = new TextBuffer();

        Assert.Equal(0, buffer.Length);
        Assert.Equal(string.Empty, buffer.GetText());
    }

    [Fact]
    public void InsertIntoEmptyBuffer()
    {
        var buffer = new TextBuffer();

        buffer.Insert(0, 'A');

        Assert.Equal(1, buffer.Length);
        Assert.Equal('A', buffer[0]);
        Assert.Equal("A", buffer.GetText());
    }

    [Fact]
    public void InsertAtBeginning()
    {
        var buffer = CreateBuffer("BC");

        buffer.Insert(0, 'A');

        Assert.Equal("ABC", buffer.GetText());
    }

    [Fact]
    public void InsertInMiddle()
    {
        var buffer = CreateBuffer("AC");

        buffer.Insert(1, 'B');

        Assert.Equal("ABC", buffer.GetText());
    }

    [Fact]
    public void InsertAtEnd()
    {
        var buffer = CreateBuffer("AB");

        buffer.Insert(buffer.Length, 'C');

        Assert.Equal("ABC", buffer.GetText());
    }

    [Theory]
    [InlineData(0, "BC")]
    [InlineData(1, "AC")]
    [InlineData(2, "AB")]
    public void DeleteAtValidPosition(int index, string expected)
    {
        var buffer = CreateBuffer("ABC");

        buffer.Delete(index);

        Assert.Equal(expected, buffer.GetText());
    }

    [Fact]
    public void DeleteReturnsRemovedCharacter()
    {
        var buffer = CreateBuffer("ABC");

        var removed = buffer.Delete(1);

        Assert.Equal('B', removed);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidInsertIndexThrows(int index)
    {
        var buffer = CreateBuffer("A");

        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Insert(index, 'X'));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void InvalidDeleteIndexThrows(int index)
    {
        var buffer = CreateBuffer("A");

        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Delete(index));
    }

    [Fact]
    public void DeleteFromEmptyBufferThrows()
    {
        var buffer = new TextBuffer();

        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Delete(0));
    }

    [Fact]
    public void GetTextAndToStringReturnCurrentContents()
    {
        var buffer = CreateBuffer("AB");
        buffer.Insert(2, 'C');

        Assert.Equal("ABC", buffer.GetText());
        Assert.Equal("ABC", buffer.ToString());
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
