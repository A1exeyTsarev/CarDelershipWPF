using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarDelershipWPF.AppData;

namespace CarDelershipWPF.Pages
{
    public partial class PageLogin : Page
    {
        public PageLogin()
        {
            InitializeComponent();
            txtLogin.Focus();

            // Нажатие Enter в поле пароля - вход в систему
            txtPassword.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    BtnLogin_Click(null, null);
            };
        }

        // ==================== БЛОКИРОВКА ВВОДА (Прямо на клавиатуре) ====================

        // Блокируем ввод недопустимых символов для логина
        private void TxtLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только латинские буквы и цифры
            Regex regex = new Regex(@"^[a-zA-Z0-9]+$");
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true; // Блокируем ввод
            }
        }

        // Блокируем пробел для логина
        private void TxtLogin_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true; // Блокируем пробел
            }
        }

        // Блокируем ввод недопустимых символов для пароля
        private void TxtPassword_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только латинские буквы и цифры
            Regex regex = new Regex(@"^[a-zA-Z0-9]+$");
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true; // Блокируем ввод
            }
        }

        // Блокируем пробел для пароля
        private void TxtPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true; // Блокируем пробел
            }
        }

        // ==================== ПРОВЕРКА ПРАВИЛЬНОСТИ ВВОДА ====================

        // Проверка логина
        private void TxtLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            string login = txtLogin.Text;

            // 1. Проверка на пустоту
            if (string.IsNullOrWhiteSpace(login))
            {
                txtLoginError.Text = "Логин не может быть пустым";
                txtLoginError.Visibility = Visibility.Visible;
                return;
            }

            // 2. Проверка длины (минимум 3, максимум 50)
            if (login.Length < 3)
            {
                txtLoginError.Text = "Логин должен содержать минимум 3 символа";
                txtLoginError.Visibility = Visibility.Visible;
                return;
            }

            if (login.Length > 50)
            {
                txtLoginError.Text = "Логин не может превышать 50 символов";
                txtLoginError.Visibility = Visibility.Visible;
                return;
            }

            // 3. Проверка на допустимые символы
            Regex regex = new Regex(@"^[a-zA-Z0-9]+$");
            if (!regex.IsMatch(login))
            {
                txtLoginError.Text = "Логин может содержать только латинские буквы (A-Z, a-z) и цифры (0-9)";
                txtLoginError.Visibility = Visibility.Visible;
                return;
            }

            // Если все проверки пройдены - скрываем ошибку
            txtLoginError.Visibility = Visibility.Collapsed;
        }

        // Проверка пароля
        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string password = txtPassword.Password;

            // 1. Проверка на пустоту
            if (string.IsNullOrWhiteSpace(password))
            {
                txtPasswordError.Text = "Пароль не может быть пустым";
                txtPasswordError.Visibility = Visibility.Visible;
                return;
            }

            // 2. Проверка длины (минимум 6, максимум 50)
            if (password.Length < 6)
            {
                txtPasswordError.Text = "Пароль должен содержать минимум 6 символов";
                txtPasswordError.Visibility = Visibility.Visible;
                return;
            }

            if (password.Length > 50)
            {
                txtPasswordError.Text = "Пароль не может превышать 50 символов";
                txtPasswordError.Visibility = Visibility.Visible;
                return;
            }

            // 3. Проверка на допустимые символы
            Regex regex = new Regex(@"^[a-zA-Z0-9]+$");
            if (!regex.IsMatch(password))
            {
                txtPasswordError.Text = "Пароль может содержать только латинские буквы (A-Z, a-z) и цифры (0-9)";
                txtPasswordError.Visibility = Visibility.Visible;
                return;
            }

            // Если все проверки пройдены - скрываем ошибку
            txtPasswordError.Visibility = Visibility.Collapsed;
        }

        // ==================== ВХОД В СИСТЕМУ ====================

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, нет ли ошибок валидации
                if (txtLoginError.Visibility == Visibility.Visible)
                {
                    MessageBox.Show("Пожалуйста, исправьте ошибки в поле 'Логин'",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtLogin.Focus();
                    return;
                }

                if (txtPasswordError.Visibility == Visibility.Visible)
                {
                    MessageBox.Show("Пожалуйста, исправьте ошибки в поле 'Пароль'",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return;
                }

                string login = txtLogin.Text.Trim();
                string password = txtPassword.Password;

                // Проверка на пустые поля
                if (string.IsNullOrWhiteSpace(login))
                {
                    MessageBox.Show("Введите логин", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtLogin.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Введите пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return;
                }

                // Ищем пользователя в базе данных
                var user = AppConnect.model01.Users
                    .FirstOrDefault(u => u.Login == login && u.Password == password);

                // Проверка: существует ли пользователь
                if (user == null)
                {
                    MessageBox.Show("Неверный логин или пароль!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtPassword.Clear();
                    txtLogin.Focus();
                    return;
                }

                // Проверка: активен ли аккаунт
                if (!user.IsActive)
                {
                    MessageBox.Show("Ваш аккаунт заблокирован. Обратитесь к администратору.", "Доступ запрещен",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем роль пользователя
                var role = AppConnect.model01.Roles
                    .FirstOrDefault(r => r.Role_Id == user.Role_Id);

                string roleName = role?.Name ?? "Клиент";

                // Сохраняем данные текущего пользователя
                AppFrame.CurrentUser = user;
                AppFrame.CurrentUserID = user.User_Id;
                AppFrame.CurrentUserName = user.FullName;
                AppFrame.CurrentUserRole = roleName;

                // ВСЕ пользователи попадают на главную страницу с товарами
                AppFrame.FrameMain.Navigate(new MainShopPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== РЕГИСТРАЦИЯ ПОЛЬЗОВАТЕЛЯ ====================

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new RegisterPage());
        }
            
    }
}