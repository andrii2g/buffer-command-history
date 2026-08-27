using BufferCommandHistory.Commands;

namespace BufferCommandHistory.History;

public sealed class CommandHistory
{
    private readonly Stack<ICommand> _undo = new();
    private readonly Stack<ICommand> _redo = new();

    public int UndoCount => _undo.Count;

    public int RedoCount => _redo.Count;

    public bool CanUndo => _undo.Count > 0;

    public bool CanRedo => _redo.Count > 0;

    public void Execute(ICommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        command.Execute();
        _undo.Push(command);
        _redo.Clear();
    }

    public bool Undo()
    {
        if (_undo.Count == 0)
        {
            return false;
        }

        var command = _undo.Peek();
        command.Undo();
        _undo.Pop();
        _redo.Push(command);
        return true;
    }

    public bool Redo()
    {
        if (_redo.Count == 0)
        {
            return false;
        }

        var command = _redo.Peek();
        command.Execute();
        _redo.Pop();
        _undo.Push(command);
        return true;
    }

    public IReadOnlyList<string> GetUndoDescriptions() =>
        _undo.Select(command => command.Description).ToArray();

    public IReadOnlyList<string> GetRedoDescriptions() =>
        _redo.Select(command => command.Description).ToArray();
}
