using ynac.Commands;

namespace ynac;

public interface IYnacConsole
{
    public Task RunAsync(BudgetCommand.Settings settings);
}