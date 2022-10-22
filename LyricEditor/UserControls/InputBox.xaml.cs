using System.Windows;
using System.Windows.Controls;

namespace LyricEditor.UserControls
{
    public partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();
        }

        public static string Show(Window owner, string title, string message, string DefaultText = "")
        {
            InputBox box = new InputBox();
            box.Owner = owner;
            box.Title = title;
            box.Message.Text = message;
            box.Input.Text = DefaultText;
            box.Input.Focus();
            box.Input.SelectAll();
            box.ShowDialog();

            return box.DialogResult.HasValue && box.DialogResult.Value ? box.Input.Text : string.Empty;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            DialogResult = btn.Name == "Yes";
        }
    }
}
