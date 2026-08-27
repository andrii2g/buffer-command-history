namespace BufferCommandHistory.Text;

public sealed class TextBuffer
{
    private readonly List<char> _characters = [];

    public int Length => _characters.Count;

    public char this[int index] => _characters[index];

    public void Insert(int index, char value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Length);

        _characters.Insert(index, value);
    }

    public char Delete(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Length);

        var removed = _characters[index];
        _characters.RemoveAt(index);
        return removed;
    }

    public string GetText() => new(_characters.ToArray());

    public override string ToString() => GetText();
}
