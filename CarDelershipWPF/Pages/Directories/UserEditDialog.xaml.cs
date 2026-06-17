using CarDelershipWPF.AppData;
using System;
using System.Linq;
using System.Windows;

namespace CarDelershipWPF.Pages.Directories
{
    public partial class UserEditDialog : Window
    {
        private Users _editingUser;
        private bool _isEditMode = false;

        public UserEditDialog()
        {
            InitializeComponent();
            Title = "Добавление пользователя";
            LoadRoles();
        }

        public UserEditDialog(Users user)
        {
            InitializeComponent();
            Title = "Редактирование пользователя";
            _editingUser = user;
            _isEditMode = true;

            LoadRoles();
            LoadUserData();
        }

        private void LoadRoles()
        {
            try
            {
                var roles = AppConnect.model01.Roles.ToList();
                cmbRole.ItemsSource = roles;
                cmbRole.DisplayMemberPath = "Name";
                cmbRole.SelectedValuePath = "Role_Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
            }
        }

        private void LoadUserData()
        {
            if (_editingUser != null)
            {
                txtLogin.Text = _editingUser.Login;
                txtFullName.Text = _editingUser.FullName;
                txtPhone.Text = _editingUser.Phone;
                txtEmail.Text = _editingUser.Email;
                chkIsActive.IsChecked = _editingUser.IsActive;
                cmbRole.SelectedValue = _editingUser.Role_Id;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_isEditMode && string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbRole.SelectedValue == null)
            {
                MessageBox.Show("Выберите роль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode && _editingUser != null)
                {
                    // Редактирование
                    _editingUser.Login = txtLogin.Text.Trim();
                    _editingUser.FullName = txtFullName.Text.Trim();
                    _editingUser.Phone = txtPhone.Text.Trim();
                    _editingUser.Email = txtEmail.Text.Trim();
                    _editingUser.IsActive = chkIsActive.IsChecked == true;
                    _editingUser.Role_Id = (int)cmbRole.SelectedValue;

                    if (!string.IsNullOrWhiteSpace(txtPassword.Password))
                    {
                        _editingUser.Password = txtPassword.Password;
                    }
                }
                else
                {
                    // Добавление нового пользователя
                    var newUser = new Users
                    {
                        Login = txtLogin.Text.Trim(),
                        Password = txtPassword.Password,
                        FullName = txtFullName.Text.Trim(),
                        Phone = txtPhone.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        IsActive = chkIsActive.IsChecked == true,
                        Role_Id = (int)cmbRole.SelectedValue,
                        RegistrationDate = DateTime.Now,
                        Discount = 0
                    };
                    AppConnect.model01.Users.Add(newUser);
                }

                AppConnect.model01.SaveChanges();

                // Закрываем окно с результатом true
                DialogResult = true;
                // НЕ вызываем Close() - DialogResult сам закроет окно
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем окно с результатом false
            DialogResult = false;
            // НЕ вызываем Close() - DialogResult сам закроет окно
        }
    }
}