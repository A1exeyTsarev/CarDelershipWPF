using CarDelershipWPF.ApplicationData;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace CarDelershipWPF.AppData
{
    internal class AppConnect
    {
        public static CarDealershipDBEntities1 model01;

        static AppConnect()
        {
            try
            {
                model01 = new CarDealershipDBEntities1();

                // Отключаем создание прокси-объектов
                model01.Configuration.ProxyCreationEnabled = false;

                // Отключаем ленивую загрузку
                model01.Configuration.LazyLoadingEnabled = false;

                if (model01.Database.Exists())
                {
                    System.Diagnostics.Debug.WriteLine("✅ Подключение к БД успешно!");
                }

                // 🚀 Инициализация YandexGPT (без ожидания - фоном)
                _ = InitYandexGPTAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 🚀 ИНИЦИАЛИЗАЦИЯ YANDEXGPT (асинхронная, не блокирует UI)
        /// </summary>
        public static async Task InitYandexGPTAsync()
        {
            try
            {
                // Загружаем настройки из файла
                YandexGPT.LoadSettings();

                // Проверяем подключение (с таймаутом)
                var timeoutTask = Task.Delay(5000);
                var connectTask = YandexGPT.TestConnectionAsync();
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == connectTask && await connectTask)
                {
                    System.Diagnostics.Debug.WriteLine("✅ YandexGPT готов к работе!");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ YandexGPT не настроен или таймаут. Проверьте yandex_config.json");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Ошибка инициализации YandexGPT: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("Добавьте FolderId и ApiKey в файл yandex_config.json в папке bin/Debug");
            }
        }

        /// <summary>
        /// 🔄 ПРОВЕРКА СТАТУСА YANDEXGPT
        /// </summary>
        public static async Task<bool> IsYandexGPTReady()
        {
            try
            {
                return !string.IsNullOrWhiteSpace(YandexGPT.FolderId) &&
                       !string.IsNullOrWhiteSpace(YandexGPT.ApiKey) &&
                       await YandexGPT.TestConnectionAsync();
            }
            catch
            {
                return false;
            }
        }
    }
}