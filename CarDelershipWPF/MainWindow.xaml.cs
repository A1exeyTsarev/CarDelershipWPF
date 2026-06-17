using System;
using System.Windows;
using CarDelershipWPF.AppData;
using CarDelershipWPF.Pages;

namespace CarDelershipWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Инициализация подключения к базе данных
            InitializeDatabaseConnection();

            // Настройка навигации
            SetupNavigationSystem();

            // Загрузка страницы авторизации
            LoadAuthorizationPage();
        }

        /// <summary>
        /// 🧪 Инициализация подключения к базе данных
        /// </summary>
        private void InitializeDatabaseConnection()
        {
            try
            {
                // Создаем экземпляр контекста базы данных
                AppConnect.model01 = new CarDealershipDBEntities1();

                // Проверка подключения
                if (AppConnect.model01.Database.Exists())
                {
                    System.Diagnostics.Debug.WriteLine("Подключение к БД успешно!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться к базе данных:\n{ex.Message}",
                    "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 🔮 Настройка системы навигации
        /// </summary>
        private void SetupNavigationSystem()
        {
            // Инициализируем статический класс для работы с фреймом
            AppFrame.FrameMain = MainFrame;
        }

        /// <summary>
        /// 🚪 Загрузка страницы авторизации
        /// </summary>
        private void LoadAuthorizationPage()
        {
            // Создаем страницу авторизации
            MainFrame.Navigate(new PageLogin());
        }
    }
}