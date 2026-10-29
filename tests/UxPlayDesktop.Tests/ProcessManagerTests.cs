using UxPlayDesktop.Models;
using UxPlayDesktop.Services;
using Xunit;

namespace UxPlayDesktop.Tests;

public class ProcessManagerTests
{
    [Fact]
    public void InitialState_IsStopped()
    {
        var argBuilder = new ArgumentBuilder();
        var parser = new LogParser();
        using var manager = new CoreProcessManager(argBuilder, parser);

        Assert.False(manager.IsRunning);
        Assert.Equal(ServerState.Stopped, manager.CurrentSession.State);
        Assert.Null(manager.CurrentSession.ClientName);
    }

    [Fact]
    public void FindCoreExecutablePath_DoesNotThrow()
    {
        var argBuilder = new ArgumentBuilder();
        var parser = new LogParser();
        using var manager = new CoreProcessManager(argBuilder, parser);

        // Should execute cleanly without unhandled exceptions
        var path = manager.FindCoreExecutablePath();
        // Path might be null in CI / test before build, but method must not crash
    }
}
