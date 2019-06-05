using System.Windows;
using System.Windows.Input;

namespace GUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		//	DataContext = new MainViewModel();
		}

		private void OnlyNumbersTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !int.TryParse(e.Text, out var num) && num >= 0;
		}

		private void OnlyPercentsTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !int.TryParse(e.Text, out var num) && num >= 0 && num <= 100;
		}
	}
}
