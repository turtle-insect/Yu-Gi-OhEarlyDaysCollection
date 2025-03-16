using System.Windows.Input;

namespace dm2
{
	class ActionCommand : ICommand
	{
#pragma warning disable CS0067
		public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
		private readonly Action<Object?> mAction;

		public ActionCommand(Action<Object?> action) => mAction = action;
		public bool CanExecute(Object? parameter) => true;
		public void Execute(Object? parameter) => mAction(parameter);
	}
}
