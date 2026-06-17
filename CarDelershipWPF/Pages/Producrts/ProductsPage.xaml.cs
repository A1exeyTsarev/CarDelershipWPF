using CarDelershipWPF.AppData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CarDelershipWPF.Pages
{
    public partial class ProductsPage : Page
    {
        public ProductsPage()
        {
            InitializeComponent();
            LoadProducts();
        }

        // Валидация ввода для поиска
        private void TxtSearch_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только буквы (латиница и кириллица), цифры, пробел и дефис
            Regex regex = new Regex(@"^[a-zA-Zа-яА-Я0-9\s\-]+$");
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true; // Блокируем ввод
                ShowSearchError("Можно использовать только буквы, цифры, пробел и дефис");
            }
            else
            {
                HideSearchError();
            }
        }

        // Блокировка вставки недопустимых символов
        private void TxtSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
            {
                e.Handled = true;
                string text = Clipboard.GetText();
                if (!string.IsNullOrEmpty(text))
                {
                    // Очищаем текст от недопустимых символов
                    string cleanText = Regex.Replace(text, @"[^a-zA-Zа-яА-Я0-9\s\-]", "");
                    if (!string.IsNullOrEmpty(cleanText))
                    {
                        int caretIndex = txtSearch.CaretIndex;
                        txtSearch.Text = txtSearch.Text.Insert(caretIndex, cleanText);
                        txtSearch.CaretIndex = caretIndex + cleanText.Length;
                    }
                }
            }
        }

        private void ShowSearchError(string message)
        {
            txtSearchError.Text = message;
            txtSearchError.Visibility = Visibility.Visible;
            txtSearch.BorderBrush = Brushes.Red;
            txtSearch.BorderThickness = new Thickness(2);
        }

        private void HideSearchError()
        {
            txtSearchError.Visibility = Visibility.Collapsed;
            txtSearch.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFC107");
            txtSearch.BorderThickness = new Thickness(1);
        }

        // Метод для получения пути к папке с изображениями
        private string GetImageFolderPath()
        {
            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string projectPath = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\.."));
                string imageFolder = Path.Combine(projectPath, "Images");

                if (!Directory.Exists(imageFolder))
                {
                    imageFolder = Path.Combine(baseDirectory, "Images");
                }

                return imageFolder;
            }
            catch
            {
                return string.Empty;
            }
        }

        // Метод для получения пути к изображению
        private string GetImagePath(Cars car, string imageFolder)
        {
            try
            {
                var dbImage = AppConnect.model01.CarImages.FirstOrDefault(i => i.Car_Id == car.Car_Id);

                if (dbImage != null && !string.IsNullOrEmpty(dbImage.ImageName))
                {
                    string fullPath = Path.Combine(imageFolder, dbImage.ImageName);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }

                string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp" };
                foreach (string ext in extensions)
                {
                    string pathById = Path.Combine(imageFolder, $"{car.Car_Id}{ext}");
                    if (File.Exists(pathById))
                    {
                        return pathById;
                    }
                }

                string defaultPath = Path.Combine(imageFolder, "no_image.jpg");
                if (File.Exists(defaultPath))
                {
                    return defaultPath;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // КЛАСС ДЛЯ ОТОБРАЖЕНИЯ ТОВАРОВ С КАРТИНКАМИ
        public class ProductDisplay
        {
            public Cars Car { get; set; }
            public string ImagePath { get; set; }

            public int Car_Id => Car?.Car_Id ?? 0;
            public string Name => Car?.Name ?? "";
            public decimal Price => Car?.Price ?? 0;
            public int Quantity => Car?.Quantity ?? 0;
            public string AvailabilityStatus => Car?.AvailabilityStatus ?? "";
        }

        private void LoadProducts()
        {
            try
            {
                var products = AppConnect.model01.Cars.ToList();
                string imageFolder = GetImageFolderPath();
                List<ProductDisplay> displayList = new List<ProductDisplay>();

                foreach (var product in products)
                {
                    string imagePath = GetImagePath(product, imageFolder);
                    displayList.Add(new ProductDisplay
                    {
                        Car = product,
                        ImagePath = imagePath
                    });
                }

                dgProducts.ItemsSource = displayList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void UpdateProductsList()
        {
            try
            {
                var products = AppConnect.model01.Cars.ToList();

                // Проверка длины поискового запроса
                string searchText = txtSearch?.Text ?? "";

                if (searchText.Length > 50)
                {
                    ShowSearchError("Поисковый запрос не может превышать 50 символов");
                    txtSearch.Text = searchText.Substring(0, 50);
                    txtSearch.CaretIndex = 50;
                    return;
                }

                // Показываем счетчик символов
                if (searchText.Length > 0)
                {
                    txtSearchCounter.Text = $"{searchText.Length}/50";
                    txtSearchCounter.Visibility = Visibility.Visible;
                }
                else
                {
                    txtSearchCounter.Visibility = Visibility.Collapsed;
                }

                // Поиск по названию
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string searchTextLower = searchText.ToLower();
                    products = products.Where(x => x.Name.ToLower().Contains(searchTextLower)).ToList();

                    if (products.Count == 0)
                    {
                        ShowSearchError("Товары не найдены");
                    }
                    else
                    {
                        HideSearchError();
                    }
                }
                else
                {
                    HideSearchError();
                }

                string imageFolder = GetImageFolderPath();
                List<ProductDisplay> displayList = new List<ProductDisplay>();

                foreach (var product in products)
                {
                    string imagePath = GetImagePath(product, imageFolder);
                    displayList.Add(new ProductDisplay
                    {
                        Car = product,
                        ImagePath = imagePath
                    });
                }

                dgProducts.ItemsSource = displayList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProductsList();
        }
        // Кнопка корзины
        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new CartPage());
        }

        // Кнопка добавления в корзину
        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.Tag as ProductDisplay;
            if (product != null)
            {
                try
                {
                    // Проверяем, есть ли товар в корзине
                    var existingCart = AppConnect.model01.ShoppingCarts
                        .FirstOrDefault(c => c.User_Id == AppFrame.CurrentUserID && c.Car_Id == product.Car.Car_Id);

                    if (existingCart != null)
                    {
                        existingCart.Quantity++;
                        MessageBox.Show($"Количество товара \"{product.Car.Name}\" увеличено на 1",
                            "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var newCart = new ShoppingCarts
                        {
                            User_Id = AppFrame.CurrentUserID,
                            Car_Id = product.Car.Car_Id,
                            Quantity = 1,
                            AddedAt = DateTime.Now
                        };
                        AppConnect.model01.ShoppingCarts.Add(newCart);
                        MessageBox.Show($"Товар \"{product.Car.Name}\" добавлен в корзину",
                            "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    AppConnect.model01.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении в корзину: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void DgProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgProducts.SelectedItem is ProductDisplay selectedDisplay)
            {
                var dialog = new ProductEditWindow(selectedDisplay.Car);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                    LoadProducts();
            }
            else
            {
                MessageBox.Show("Выберите товар для редактирования!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProductEditWindow();
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
                LoadProducts();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is ProductDisplay selectedDisplay)
            {
                var dialog = new ProductEditWindow(selectedDisplay.Car);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                    LoadProducts();
            }
            else
            {
                MessageBox.Show("Выберите товар для редактирования!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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