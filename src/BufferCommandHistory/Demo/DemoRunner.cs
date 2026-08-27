using BufferCommandHistory.Commands;
using BufferCommandHistory.History;
using BufferCommandHistory.Text;

namespace BufferCommandHistory.Demo;

public static class DemoRunner
{
    public static void Run(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteLine("BUFFER COMMAND HISTORY");
        writer.WriteLine("======================");

        RunPrimitiveEditing(writer);
        RunDeletion(writer);
        RunMacro(writer);
    }

    private static void RunPrimitiveEditing(TextWriter writer)
    {
        var buffer = new TextBuffer();
        var history = new CommandHistory();
        var step = 0;

        WritePhase(writer, "PHASE 1 - PRIMITIVE TEXT EDITING");
        foreach (var character in "Hello!")
        {
            Execute(writer, history, buffer, new InsertCharCommand(buffer, buffer.Length, character), ref step);
        }

        WritePhase(writer, "PHASE 2 - UNDO AND REDO");
        Undo(writer, history, buffer, ref step);
        Undo(writer, history, buffer, ref step);
        Redo(writer, history, buffer, ref step);

        WritePhase(writer, "PHASE 3 - REDO INVALIDATION");
        Undo(writer, history, buffer, ref step);
        Execute(writer, history, buffer, new InsertCharCommand(buffer, buffer.Length, '?'), ref step);
        writer.WriteLine($"Can redo after divergent edit: {history.CanRedo}");
        WriteHistory(writer, history);
    }

    private static void RunDeletion(TextWriter writer)
    {
        var buffer = CreateBuffer("ABCDE");
        var history = new CommandHistory();
        var step = 0;

        WritePhase(writer, "PHASE 4 - DELETION");
        writer.WriteLine($"Starting buffer: \"{buffer}\"");
        Execute(writer, history, buffer, new DeleteCharCommand(buffer, 2), ref step);
        Undo(writer, history, buffer, ref step);
        Redo(writer, history, buffer, ref step);
    }

    private static void RunMacro(TextWriter writer)
    {
        var buffer = CreateBuffer("Go");
        var history = new CommandHistory();
        var step = 0;
        var macro = new MacroCommand(
            "Insert ' Team'",
            " Team".Select((character, offset) =>
                (ICommand)new InsertCharCommand(buffer, buffer.Length + offset, character)));

        WritePhase(writer, "PHASE 5 - MACRO COMMAND");
        writer.WriteLine($"Before macro: \"{buffer}\"");
        Execute(writer, history, buffer, macro, ref step);
        writer.WriteLine($"Macro occupies {history.UndoCount} undo entry.");
        Undo(writer, history, buffer, ref step);
        Redo(writer, history, buffer, ref step);
        WriteHistory(writer, history);
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

    private static void Execute(
        TextWriter writer,
        CommandHistory history,
        TextBuffer buffer,
        ICommand command,
        ref int step)
    {
        history.Execute(command);
        WriteState(writer, ++step, $"Execute: {command.Description}", buffer, history);
    }

    private static void Undo(
        TextWriter writer,
        CommandHistory history,
        TextBuffer buffer,
        ref int step)
    {
        var description = history.GetUndoDescriptions()[0];
        history.Undo();
        WriteState(writer, ++step, $"Undo: {description}", buffer, history);
    }

    private static void Redo(
        TextWriter writer,
        CommandHistory history,
        TextBuffer buffer,
        ref int step)
    {
        var description = history.GetRedoDescriptions()[0];
        history.Redo();
        WriteState(writer, ++step, $"Redo: {description}", buffer, history);
    }

    private static void WriteState(
        TextWriter writer,
        int step,
        string action,
        TextBuffer buffer,
        CommandHistory history)
    {
        writer.WriteLine($"[{step:00}] {action}");
        writer.WriteLine($"Buffer: \"{buffer}\"");
        writer.WriteLine($"Undo: {history.UndoCount} | Redo: {history.RedoCount}");
    }

    private static void WritePhase(TextWriter writer, string title)
    {
        writer.WriteLine();
        writer.WriteLine(title);
        writer.WriteLine(new string('-', title.Length));
    }

    private static void WriteHistory(TextWriter writer, CommandHistory history)
    {
        WriteStack(writer, "Undo stack", history.GetUndoDescriptions());
        WriteStack(writer, "Redo stack", history.GetRedoDescriptions());
    }

    private static void WriteStack(TextWriter writer, string label, IReadOnlyList<string> descriptions)
    {
        writer.WriteLine($"{label}:");
        if (descriptions.Count == 0)
        {
            writer.WriteLine("  <empty>");
            return;
        }

        foreach (var description in descriptions)
        {
            writer.WriteLine($"  {description}");
        }
    }
}
