using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace AiDesignTool.LCommands
{
    class LCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public LCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
    //
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
    public abstract class AsyncCommandBase : IAsyncCommand
    {
        public abstract bool CanExecute(object parameter);
        public abstract Task ExecuteAsync(object parameter);
        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        protected void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
    public class AsyncCommand : AsyncCommandBase
    {
        private readonly Func<Task> _command;
        public AsyncCommand(Func<Task> command)
        {
            _command = command;
        }
        public override bool CanExecute(object parameter)
        {
            return true;
        }
        public override Task ExecuteAsync(object parameter)
        {
            return _command();
        }
    }
    //
    public class RGBToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string dValue)
            {
                return dValue;
            }
            return value?.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hex = value?.ToString();
            if (string.IsNullOrWhiteSpace(hex) || !hex.StartsWith("#") || (hex.Length != 7 && hex.Length != 4))
            {
                return string.Empty;
            }
            return hex;
        }
    }

}
