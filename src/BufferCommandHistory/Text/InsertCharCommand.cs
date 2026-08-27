using BufferCommandHistory.Commands;

namespace BufferCommandHistory.Text;

public sealed class InsertCharCommand : ICommand
{
    private readonly TextBuffer _buffer;
    private readonly int _index;
    private readonly char _value;

    public InsertCharCommand(TextBuffer buffer, int index, char value)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        _buffer = buffer;
        _index = index;
        _value = value;
    }

    public string Description => $"Insert '{_value}' at {_index}";

    public void Execute() => _buffer.Insert(_index, _value);

    public void Undo() => _buffer.Delete(_index);
}
