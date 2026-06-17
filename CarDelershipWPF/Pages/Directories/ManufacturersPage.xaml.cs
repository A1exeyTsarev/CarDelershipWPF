using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CarDelershipWPF.AppData;

namespace CarDelershipWPF.Pages.Directories
{
    public partial class ManufacturersPage : Page
    {
        public ManufacturersPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                dgItems.ItemsSource = AppConnect.model01.Manufacturers
                    .OrderBy(m => m.Name)
                    .ToList();
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
                MessageBox.Show("Введите название", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var manufacturer = new Manufacturers { Name = name, Country_Id = 1 };
                AppConnect.model01.Manufacturers.Add(manufacturer);
                AppConnect.model01.SaveChanges();
                txtName.Text = "";
                LoadData();
                MessageBox.Show("Производитель добавлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var manufacturer = (sender as Button)?.Tag as Manufacturers;
            if (manufacturer == null) return;

            var dialog = new InputDialog("Редактирование", "Введите новое название:", manufacturer.Name);
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Answer))
            {
                try
                {
                    manufacturer.Name = dialog.Answer.Trim();
                    AppConnect.model01.SaveChanges();
                    LoadData();
                    MessageBox.Show("Производитель обновлен", "Успех",
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
            var manufacturer = (sender as Button)?.Tag as Manufacturers;
            if (manufacturer == null) return;

            var result = MessageBox.Show($"Удалить '{manufacturer.Name}'?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    AppConnect.model01.Manufacturers.Remove(manufacturer);
                    AppConnect.model01.SaveChanges();
                    LoadData();
                    MessageBox.Show("Производитель удален", "Успех",
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