using System.Windows;
using System.Windows.Controls;
using CarDelershipWPF.AppData;

namespace CarDelershipWPF.Pages.Directories
{
    public partial class DirectoriesMenuPage : Page
    {
        public DirectoriesMenuPage()
        {
            InitializeComponent();

            // Скрываем кнопку пользователей для менеджера
            if (!AppFrame.IsAdmin)
            {
                btnUsers.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnManufacturers_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new ManufacturersPage());
        }

        private void BtnModels_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new ModelsPage());
        }

        private void BtnColors_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new ColorsPage());
        }

        private void BtnBodyTypes_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new BodyTypesPage());
        }

        private void BtnEngineTypes_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new EngineTypesPage());
        }

        private void BtnTransmissions_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new TransmissionsPage());
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            // Дополнительная проверка перед переходом
            if (!AppFrame.IsAdmin)
            {
                MessageBox.Show("Доступ запрещен! Только для администраторов.",
                    "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AppFrame.FrameMain.Navigate(new UsersPage());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (AppFrame.FrameMain.CanGoBack)
                AppFrame.FrameMain.GoBack();
            else
                AppFrame.FrameMain.Navigate(new DashboardPage());
        }
    }
}