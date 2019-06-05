using System;
using System.Windows.Input;

namespace GUI.Helpers
{
	public class CommandWrapper : ICommand
	{
		private readonly Func<object, bool> _canExecute;
		private readonly Action<object> _onExecute;

		/// <inheritdoc />
		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		/// <summary>Конструктор команды</summary>
		/// <param name="execute">Выполняемый метод команды</param>
		/// <param name="canExecute">Метод разрешающий выполнение команды</param>
		public CommandWrapper(Action<object> execute, Func<object, bool> canExecute = null)
		{
			_onExecute = execute;
			_canExecute = canExecute;
		}

		/// <inheritdoc />
		public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

		/// <inheritdoc />
		public void Execute(object parameter) => _onExecute?.Invoke(parameter);
	}
}