using CarDelershipWPF.AppData;
using CarDelershipWPF.Pages.Directories;
using System.Windows;
using System.Windows.Controls;

namespace CarDelershipWPF.Pages
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            txtWelcome.Text = $"Здравствуйте, {AppFrame.CurrentUserName} ({AppFrame.CurrentUserRole})";
        }

        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new ProductsPage());
        }

        // Удален метод BtnPriceHistory_Click

        private void BtnDirectories_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new DirectoriesMenuPage());
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new OrdersPage());
        }

        private void BtnSupplies_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new SuppliesPage());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AppFrame.Logout();
                AppFrame.FrameMain.Navigate(new PageLogin());
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Возврат на главную страницу с товарами
            AppFrame.FrameMain.Navigate(new MainShopPage());
        }
    }
}