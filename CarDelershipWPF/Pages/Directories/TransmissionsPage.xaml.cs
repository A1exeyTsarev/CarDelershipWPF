using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CarDelershipWPF.AppData;

namespace CarDelershipWPF.Pages.Directories
{
    public partial class TransmissionsPage : Page
    {
        public TransmissionsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var transmissions = AppConnect.model01.Transmissions
                    .OrderBy(t => t.Name)
                    .ToList();
                dgItems.ItemsSource = transmissions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var newName = txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Введите название коробки передач", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var transmission = new Transmissions
                {
                    Name = newName
                };
                AppConnect.model01.Transmissions.Add(transmission);
                AppConnect.model01.SaveChanges();

                txtName.Text = "";
                LoadData();

                MessageBox.Show("Коробка передач успешно добавлена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var transmission = (sender as Button)?.Tag as Transmissions;
            if (transmission == null) return;

            var dialog = new InputDialog("Редактирование", "Введите новое название:", transmission.Name);
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Answer))
            {
                try
                {
                    transmission.Name = dialog.Answer.Trim();
                    AppConnect.model01.SaveChanges();
                    LoadData();

                    MessageBox.Show("Коробка передач успешно обновлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var transmission = (sender as Button)?.Tag as Transmissions;
            if (transmission == null) return;

            var result = MessageBox.Show($"Удалить коробку передач '{transmission.Name}'?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    AppConnect.model01.Transmissions.Remove(transmission);
                    AppConnect.model01.SaveChanges();
                    LoadData();

                    MessageBox.Show("Коробка передач успешно удалена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}\nВозможно, коробка передач используется в других таблицах", "Ошибка",
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