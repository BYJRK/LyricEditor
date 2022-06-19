using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LyricEditor.UserControls
{
    /// <summary>
    /// InputBox.xaml 的交互逻辑
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();
        }

        public bool InputBoxResult { get; private set; }

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

            return box.InputBoxResult ? box.Input.Text : string.Empty;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Name == "Yes")
                InputBoxResult = true;
            else
                InputBoxResult = false;
            Close();
        }

        private void Input_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                InputBoxResult = true;
                Close();
            }
        }
    }
}
