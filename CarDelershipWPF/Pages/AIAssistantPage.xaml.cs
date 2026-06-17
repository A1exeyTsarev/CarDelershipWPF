using CarDelershipWPF.AppData;
using CarDelershipWPF.ApplicationData;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CarDelershipWPF.Pages
{
    public partial class AIAssistantPage : Page
    {
        public AIAssistantPage()
        {
            InitializeComponent();

            // Приветственное сообщение
            AddMessage("🤖 Здравствуйте! Я AI помощник автосалона CarDelership.\n\n" +
                      "Я могу помочь:\n" +
                      "• Подобрать автомобиль по вашим предпочтениям\n" +
                      "• Улучшить описание автомобиля\n" +
                      "• Ответить на вопросы о кредитовании\n" +
                      "• Рассказать о характеристиках машин\n\n" +
                      "Задайте мне вопрос! 🚗", false);
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }

        private async void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await SendMessageAsync();
            }
        }

        private async Task SendMessageAsync()
        {
            var message = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            AddMessage($"👤 {message}", true);
            txtInput.Clear();

            AddMessage("🤖 Печатает...", false, true);

            try
            {
                var response = await YandexGPT.SendMessageAsync(message);
                RemoveTypingIndicator();
                AddMessage($"🤖 {response}", false);
            }
            catch (Exception ex)
            {
                RemoveTypingIndicator();
                AddMessage($"⚠️ Ошибка: {ex.Message}\n\nПроверьте файл yandex_config.json", false);
            }
        }

        private void AddMessage(string text, bool isUser, bool isTyping = false)
        {
            Dispatcher.Invoke(() =>
            {
                // Используем полные имена System.Windows.Media.Color
                var border = new Border
                {
                    Background = isUser ?
                        new SolidColorBrush(System.Windows.Media.Color.FromRgb(51, 51, 51)) :
                        new SolidColorBrush(System.Windows.Media.Color.FromRgb(42, 42, 42)),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(12),
                    Margin = new Thickness(isUser ? 50 : 10, 5, isUser ? 10 : 50, 5),
                    HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                    Name = isTyping ? "TypingIndicator" : null
                };

                var textBlock = new TextBlock
                {
                    Text = text,
                    Foreground = isUser ?
                        new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)) :
                        new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 193, 7)),
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 450
                };

                border.Child = textBlock;
                ChatPanel.Children.Add(border);
                ChatScroller.ScrollToBottom();
            });
        }

        private void RemoveTypingIndicator()
        {
            Dispatcher.Invoke(() =>
            {
                for (int i = ChatPanel.Children.Count - 1; i >= 0; i--)
                {
                    if (ChatPanel.Children[i] is Border b && b.Name == "TypingIndicator")
                    {
                        ChatPanel.Children.RemoveAt(i);
                        break;
                    }
                }
            });
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new MainShopPage());
        }
    }
}