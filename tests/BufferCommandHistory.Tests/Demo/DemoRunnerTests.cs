using BufferCommandHistory.Demo;

namespace BufferCommandHistory.Tests.Demo;

public sealed class DemoRunnerTests
{
    [Fact]
    public void RunRejectsNullWriter()
    {
        Assert.Throws<ArgumentNullException>(() => DemoRunner.Run(null!));
    }

    [Fact]
    public void RunProducesDeterministicOutputCoveringAllPhases()
    {
        using var firstWriter = new StringWriter();
        using var secondWriter = new StringWriter();

        DemoRunner.Run(firstWriter);
        DemoRunner.Run(secondWriter);

        var output = firstWriter.ToString();
        Assert.Equal(output, secondWriter.ToString());
        Assert.Contains("PHASE 1 - PRIMITIVE TEXT EDITING", output, StringComparison.Ordinal);
        Assert.Contains("PHASE 2 - UNDO AND REDO", output, StringComparison.Ordinal);
        Assert.Contains("PHASE 3 - REDO INVALIDATION", output, StringComparison.Ordinal);
        Assert.Contains("Can redo after divergent edit: False", output, StringComparison.Ordinal);
        Assert.Contains("PHASE 4 - DELETION", output, StringComparison.Ordinal);
        Assert.Contains("PHASE 5 - MACRO COMMAND", output, StringComparison.Ordinal);
        Assert.Contains("Macro occupies 1 undo entry.", output, StringComparison.Ordinal);
        Assert.Contains("Buffer: \"Go Team\"", output, StringComparison.Ordinal);
    }
}
