using CarDelershipWPF.AppData;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace CarDelershipWPF.Pages
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateFields())
                    return;

                string login = txtLogin.Text.Trim();
                var existingUser = AppConnect.model01.Users
                    .FirstOrDefault(u => u.Login == login);

                if (existingUser != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string email = txtEmail.Text.Trim();
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var existingEmail = AppConnect.model01.Users
                        .FirstOrDefault(u => u.Email == email);
                    if (existingEmail != null)
                    {
                        txtEmailError.Text = "Email уже используется";
                        txtEmailError.Visibility = Visibility.Visible;
                        return;
                    }
                }

                string phone = txtPhone.Text.Trim();
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    var existingPhone = AppConnect.model01.Users
                        .FirstOrDefault(u => u.Phone == phone);
                    if (existingPhone != null)
                    {
                        txtPhoneError.Text = "Телефон уже используется";
                        txtPhoneError.Visibility = Visibility.Visible;
                        return;
                    }
                }

                int clientRoleId = AppConnect.model01.Roles
                    .FirstOrDefault(r => r.Name == "Клиент")?.Role_Id ?? 3;

                var newUser = new Users
                {
                    Login = login,
                    Password = txtPassword.Password,
                    FullName = txtFullName.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    PassportData = string.IsNullOrWhiteSpace(txtPassportData.Text) ? null : txtPassportData.Text.Trim(),
                    Role_Id = clientRoleId,
                    IsActive = true,
                    RegistrationDate = DateTime.Now,
                    Discount = 0
                };

                AppConnect.model01.Users.Add(newUser);
                AppConnect.model01.SaveChanges();

                // Автоматически входим в систему после регистрации
                var role = AppConnect.model01.Roles.FirstOrDefault(r => r.Role_Id == clientRoleId);
                string roleName = role?.Name ?? "Клиент";

                // Сохраняем данные текущего пользователя
                AppFrame.CurrentUser = newUser;
                AppFrame.CurrentUserID = newUser.User_Id;
                AppFrame.CurrentUserName = newUser.FullName;
                AppFrame.CurrentUserRole = roleName;

                MessageBox.Show("Регистрация прошла успешно!\n\n" +
                    "Вы автоматически вошли в систему.",
                    "Успешная регистрация", MessageBoxButton.OK, MessageBoxImage.Information);

                // Переходим на главную страницу с товарами
                AppFrame.FrameMain.Navigate(new MainShopPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateFields()
        {
            bool isValid = true;

            // ФИО
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                txtFullNameError.Text = "Введите ФИО";
                txtFullNameError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                txtFullNameError.Visibility = Visibility.Collapsed;
            }

            // Логин
            string login = txtLogin.Text.Trim();
            if (string.IsNullOrWhiteSpace(login))
            {
                txtLoginError.Text = "Введите логин";
                txtLoginError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (login.Length < 3)
            {
                txtLoginError.Text = "Логин должен содержать минимум 3 символа";
                txtLoginError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!Regex.IsMatch(login, @"^[a-zA-Z0-9]+$"))
            {
                txtLoginError.Text = "Только латинские буквы и цифры";
                txtLoginError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                txtLoginError.Visibility = Visibility.Collapsed;
            }

            // Пароль
            string password = txtPassword.Password;
            if (string.IsNullOrWhiteSpace(password))
            {
                txtPasswordError.Text = "Введите пароль";
                txtPasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (password.Length < 6)
            {
                txtPasswordError.Text = "Пароль должен содержать минимум 6 символов";
                txtPasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!Regex.IsMatch(password, @"^[a-zA-Z0-9]+$"))
            {
                txtPasswordError.Text = "Только латинские буквы и цифры";
                txtPasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                txtPasswordError.Visibility = Visibility.Collapsed;
            }

            // Подтверждение пароля
            if (txtConfirmPassword.Password != password)
            {
                txtConfirmError.Text = "Пароли не совпадают";
                txtConfirmError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                txtConfirmError.Visibility = Visibility.Collapsed;
            }

            // Телефон (необязательно)
            string phone = txtPhone.Text.Trim();
            if (!string.IsNullOrWhiteSpace(phone))
            {
                string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                Regex phoneRegex = new Regex(@"^(\+7|8)[0-9]{10}$");
                if (!phoneRegex.IsMatch(cleanPhone))
                {
                    txtPhoneError.Text = "Введите номер в формате +7XXXXXXXXXX или 8XXXXXXXXXX";
                    txtPhoneError.Visibility = Visibility.Visible;
                    isValid = false;
                }
                else
                {
                    txtPhoneError.Visibility = Visibility.Collapsed;
                }
            }

            // Email (необязательно)
            string email = txtEmail.Text.Trim();
            if (!string.IsNullOrWhiteSpace(email))
            {
                Regex emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailRegex.IsMatch(email))
                {
                    txtEmailError.Text = "Введите корректный email";
                    txtEmailError.Visibility = Visibility.Visible;
                    isValid = false;
                }
                else
                {
                    txtEmailError.Visibility = Visibility.Collapsed;
                }
            }

            return isValid;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Возврат на страницу входа
            AppFrame.FrameMain.Navigate(new PageLogin());
        }
    }
}