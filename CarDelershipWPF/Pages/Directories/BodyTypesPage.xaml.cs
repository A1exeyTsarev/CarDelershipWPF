using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CarDelershipWPF.AppData;
using CarDelershipWPF;
namespace CarDelershipWPF.Pages.Directories
{
    public partial class BodyTypesPage : Page
    {
        public BodyTypesPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                dgItems.ItemsSource = AppConnect.model01.BodyTypes.OrderBy(b => b.Name).ToList();
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
                var bodyType = new BodyTypes { Name = name };
                AppConnect.model01.BodyTypes.Add(bodyType);
                AppConnect.model01.SaveChanges();
                txtName.Text = "";
                LoadData();
                MessageBox.Show("Добавлено", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var bodyType = (sender as Button)?.Tag as BodyTypes;
            if (bodyType == null) return;

            var dialog = new InputDialog("Редактирование", "Введите новое название:", bodyType.Name);
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Answer))
            {
                try
                {
                    bodyType.Name = dialog.Answer.Trim();
                    AppConnect.model01.SaveChanges();
                    LoadData();
                    MessageBox.Show("Обновлено", "Успех",
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
            var bodyType = (sender as Button)?.Tag as BodyTypes;
            if (bodyType == null) return;

            var result = MessageBox.Show($"Удалить '{bodyType.Name}'?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    AppConnect.model01.BodyTypes.Remove(bodyType);
                    AppConnect.model01.SaveChanges();
                    LoadData();
                    MessageBox.Show("Удалено", "Успех",
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