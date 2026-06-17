using CarDelershipWPF.AppData;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CarDelershipWPF.Pages.Directories
{
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();

            // Проверка прав доступа при загрузке страницы
            if (!AppFrame.IsAdmin)
            {
                MessageBox.Show("Доступ запрещен! Только для администраторов.",
                    "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);

                if (AppFrame.FrameMain.CanGoBack)
                    AppFrame.FrameMain.GoBack();
                else
                    AppFrame.FrameMain.Navigate(new DashboardPage());
                return;
            }

            LoadData();
            dgUsers.MouseDoubleClick += DgUsers_MouseDoubleClick;
        }

        private void LoadData()
        {
            try
            {
                var users = AppConnect.model01.Users.ToList();
                var roles = AppConnect.model01.Roles.ToDictionary(r => r.Role_Id, r => r.Name);

                var result = users.Select(u => new
                {
                    u.User_Id,
                    u.Login,
                    u.FullName,
                    u.Phone,
                    u.Email,
                    u.IsActive,
                    RoleName = roles.ContainsKey(u.Role_Id) ? roles[u.Role_Id] : "Не указана"
                }).OrderBy(u => u.Login).ToList();

                dgUsers.ItemsSource = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!AppFrame.IsAdmin)
            {
                MessageBox.Show("Доступ запрещен!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new UserEditDialog();
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void DgUsers_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!AppFrame.IsAdmin)
            {
                MessageBox.Show("Доступ запрещен!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dgUsers.SelectedItem != null)
            {
                var selected = dgUsers.SelectedItem;
                var id = (int)selected.GetType().GetProperty("User_Id")?.GetValue(selected);

                var user = AppConnect.model01.Users.FirstOrDefault(u => u.User_Id == id);
                if (user != null)
                {
                    var dialog = new UserEditDialog(user);
                    dialog.Owner = Window.GetWindow(this);
                    if (dialog.ShowDialog() == true)
                    {
                        LoadData();
                    }
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