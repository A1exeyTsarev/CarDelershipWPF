using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarDelershipWPF.ApplicationData
{
    public static class YandexGPT
    {
        // ==========================================
        // 🔧 НАСТРОЙКИ - ЗАПОЛНИ СВОИМИ ДАННЫМИ!
        // ==========================================
        public static string FolderId { get; set; } = "b1g9hcj023vdfv6etl9i";
        public static string ApiKey { get; set; } = "AQVN0dIe5GKaSNI8Zp8Z7Fcfm2jjH1_ndT3k-Vib";
        // ==========================================

        private static string SystemPrompt =
            "Ты — AI помощник автосалона CarDelership. " +
            "Помогай клиентам выбирать автомобили, отвечай на вопросы о характеристиках, ценах, кредитовании. " +
            "Отвечай на русском, кратко, полезно, с эмодзи.";

        public static int TotalRequests { get; private set; } = 0;
        public static DateTime LastRequestTime { get; private set; }

        public static async Task<bool> TestConnectionAsync()
        {
            try
            {
                string response = await SendMessageAsync("Привет! Напиши 'OK'");
                return response.Contains("OK") || !string.IsNullOrEmpty(response);
            }
            catch
            {
                return false;
            }
        }

        public static async Task<string> SendMessageAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(FolderId) || string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new Exception("❌ Не указан FolderId или ApiKey! Отредактируйте yandex_config.json");
            }

            try
            {
                TotalRequests++;
                LastRequestTime = DateTime.Now;

                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var request = new
                {
                    modelUri = $"gpt://{FolderId}/yandexgpt/latest",
                    completionOptions = new
                    {
                        stream = false,
                        temperature = 0.6,
                        maxTokens = 1000
                    },
                    messages = new[]
                    {
                        new { role = "system", text = SystemPrompt },
                        new { role = "user", text = userMessage }
                    }
                };

                string json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Api-Key {ApiKey}");

                var response = await client.PostAsync(
                    "https://llm.api.cloud.yandex.net/foundationModels/v1/completion",
                    content);

                string responseJson = await response.Content.ReadAsStringAsync();
                client.Dispose();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Ошибка API: {response.StatusCode}");
                }

                var doc = JsonDocument.Parse(responseJson);
                var result = doc.RootElement
                    .GetProperty("result")
                    .GetProperty("alternatives")[0]
                    .GetProperty("message")
                    .GetProperty("text")
                    .GetString();
                doc.Dispose();

                return result ?? "...";
            }
            catch (Exception ex)
            {
                throw new Exception($"🧙 Ошибка YandexGPT: {ex.Message}");
            }
        }

        // 🚗 МЕТОДЫ ДЛЯ АВТОСАЛОНА
        public static async Task<string> RecommendCarAsync(string preferences)
        {
            return await SendMessageAsync(
                $"Рекомендуй автомобиль по предпочтениям: {preferences}. " +
                $"Бренды: LADA, BMW, Toyota, Hyundai, Kia, Mercedes-Benz. Ответь кратко.");
        }

        public static async Task<string> ImproveDescriptionAsync(string description)
        {
            return await SendMessageAsync($"Улучши описание автомобиля: \"{description}\"");
        }

        public static async Task<string> AskAboutCreditAsync(string question)
        {
            return await SendMessageAsync($"Ответь на вопрос о кредитовании: {question}. Кратко.");
        }

        // 💾 СОХРАНЕНИЕ НАСТРОЕК
        public static void SaveSettings()
        {
            var settings = new { FolderId, ApiKey };
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("yandex_config.json", json);
        }

        // 📂 ЗАГРУЗКА НАСТРОЕК
        public static void LoadSettings()
        {
            try
            {
                if (File.Exists("yandex_config.json"))
                {
                    string json = File.ReadAllText("yandex_config.json");
                    var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("FolderId", out var fId))
                        FolderId = fId.GetString() ?? "";

                    if (doc.RootElement.TryGetProperty("ApiKey", out var aKey))
                        ApiKey = aKey.GetString() ?? "";

                    doc.Dispose();
                }
                else
                {
                    SaveSettings();
                    System.Diagnostics.Debug.WriteLine("📄 Создан файл yandex_config.json. Заполните его!");
                }
            }
            catch { }
        }
    }
}