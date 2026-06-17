using System.Windows;
using System.Windows.Input;

namespace CarDelershipWPF
{
    public partial class InputDialog : Window
    {
        public string Answer { get; private set; }

        public InputDialog(string title, string prompt, string defaultValue = "")
        {
            InitializeComponent();
            Title = title;
            lblPrompt.Text = prompt;
            txtInput.Text = defaultValue;
            txtInput.Focus();

            txtInput.KeyDown += (s, ev) =>
            {
                if (ev.Key == Key.Enter)
                    BtnOk_Click(null, null);
            };
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Answer = txtInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(Answer))
            {
                MessageBox.Show("Введите значение", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}