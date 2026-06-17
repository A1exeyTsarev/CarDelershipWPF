using CarDelershipWPF.AppData;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace CarDelershipWPF.Pages.Directories
{
    public partial class ColorsPage : Page
    {
        public ColorsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                dgItems.ItemsSource = AppConnect.model01.Colors.OrderBy(c => c.Name).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var name = txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название цвета", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var color = new Colors { Name = name };
                AppConnect.model01.Colors.Add(color);
                AppConnect.model01.SaveChanges();
                txtName.Text = "";
                LoadData();
                MessageBox.Show("Цвет добавлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var color = (sender as Button)?.Tag as Colors;
            if (color == null) return;

            var dialog = new InputDialog("Редактирование", "Введите новое название:", color.Name);
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Answer))
            {
                try
                {
                    color.Name = dialog.Answer.Trim();
                    AppConnect.model01.SaveChanges();
                    LoadData();
                    MessageBox.Show("Цвет обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var color = (sender as Button)?.Tag as Colors;
            if (color == null) return;

            var result = MessageBox.Show($"Удалить цвет '{color.Name}'?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    AppConnect.model01.Colors.Remove(color);
                    AppConnect.model01.SaveChanges();
                    LoadData();
                    MessageBox.Show("Цвет удален", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}\nВозможно, есть связанные записи", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (AppFrame.FrameMain.CanGoBack)
                AppFrame.FrameMain.GoBack();
            else
                AppFrame.FrameMain.Navigate(new DirectoriesMenuPage());
        }
    }
}