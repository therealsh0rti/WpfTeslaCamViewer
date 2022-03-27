using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfTeslaCamViewer;

public class Command : ICommand
{
    private readonly Func<object?, bool>? canExecuteFunc;
    private readonly Func<object?, Task> executeFunc;

    public Command(Func<object?, Task> executeFunc, Func<object?, bool> canExecuteFunc = null)
    {
        this.executeFunc = executeFunc;
        this.canExecuteFunc = canExecuteFunc;
    }

    public bool CanExecute(object? parameter)
    {
        return canExecuteFunc?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        executeFunc.Invoke(parameter);
    }

    public event EventHandler? CanExecuteChanged;
}
