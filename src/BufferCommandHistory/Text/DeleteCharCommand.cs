using BufferCommandHistory.Commands;

namespace BufferCommandHistory.Text;

public sealed class DeleteCharCommand : ICommand
{
    private readonly TextBuffer _buffer;
    private readonly int _index;
    private char _deletedCharacter;

    public DeleteCharCommand(TextBuffer buffer, int index)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        _buffer = buffer;
        _index = index;
    }

    public string Description => $"Delete char at {_index}";

    public void Execute() => _deletedCharacter = _buffer.Delete(_index);

    public void Undo() => _buffer.Insert(_index, _deletedCharacter);
}
