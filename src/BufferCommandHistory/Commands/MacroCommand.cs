namespace BufferCommandHistory.Commands;

public sealed class MacroCommand : ICommand
{
    private readonly IReadOnlyList<ICommand> _commands;

    public MacroCommand(string description, IEnumerable<ICommand> commands)
    {
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(commands);

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("A macro description cannot be blank.", nameof(description));
        }

        var materializedCommands = commands.ToArray();
        if (materializedCommands.Length == 0)
        {
            throw new ArgumentException("A macro must contain at least one command.", nameof(commands));
        }

        if (materializedCommands.Any(static command => command is null))
        {
            throw new ArgumentException("A macro cannot contain a null command.", nameof(commands));
        }

        Description = description;
        _commands = materializedCommands;
    }

    public string Description { get; }

    public void Execute()
    {
        foreach (var command in _commands)
        {
            command.Execute();
        }
    }

    public void Undo()
    {
        for (var index = _commands.Count - 1; index >= 0; index--)
        {
            _commands[index].Undo();
        }
    }
}
