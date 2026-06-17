using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CarDelershipWPF.AppData;

namespace CarDelershipWPF.Pages.Directories
{
    public partial class EngineTypesPage : Page
    {
        public EngineTypesPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var engineTypes = AppConnect.model01.EngineTypes
                    .OrderBy(e => e.Name)
                    .ToList();
                dgItems.ItemsSource = engineTypes;
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
                MessageBox.Show("Введите название типа двигателя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var engineType = new EngineTypes
                {
                    Name = newName
                };
                AppConnect.model01.EngineTypes.Add(engineType);
                AppConnect.model01.SaveChanges();

                txtName.Text = "";
                LoadData();

                MessageBox.Show("Тип двигателя успешно добавлен", "Успех",
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
            var engineType = (sender as Button)?.Tag as EngineTypes;
            if (engineType == null) return;

            var dialog = new InputDialog("Редактирование", "Введите новое название:", engineType.Name);
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Answer))
            {
                try
                {
                    engineType.Name = dialog.Answer.Trim();
                    AppConnect.model01.SaveChanges();
                    LoadData();

                    MessageBox.Show("Тип двигателя успешно обновлен", "Успех",
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
            var engineType = (sender as Button)?.Tag as EngineTypes;
            if (engineType == null) return;

            var result = MessageBox.Show($"Удалить тип двигателя '{engineType.Name}'?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    AppConnect.model01.EngineTypes.Remove(engineType);
                    AppConnect.model01.SaveChanges();
                    LoadData();

                    MessageBox.Show("Тип двигателя успешно удален", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}\nВозможно, тип двигателя используется в других таблицах", "Ошибка",
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