using CarDelershipWPF.AppData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CarDelershipWPF.Pages
{
    public partial class MainShopPage : Page
    {
        private List<ProductDisplay> allProducts;

        public MainShopPage()
        {
            InitializeComponent();

            txtUserInfo.Text = $"👤 {AppFrame.CurrentUserName}";
            txtUserRole.Text = $"🎫 {AppFrame.CurrentUserRole}";

            // Инициализация ComboBox сортировки
            cmbSort.Items.Add("По умолчанию");
            cmbSort.Items.Add("Цена: по возрастанию");
            cmbSort.Items.Add("Цена: по убыванию");
            cmbSort.Items.Add("Название: А-Я");
            cmbSort.Items.Add("Название: Я-А");
            cmbSort.Items.Add("Год: новые сначала");
            cmbSort.Items.Add("Год: старые сначала");
            cmbSort.SelectedIndex = 0;

            LoadProducts();
            LoadManufacturers();
            CheckUserRole();
        }

        // Метод для получения пути к папке с изображениями (рабочий)
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

        // Метод для получения пути к изображению (рабочий)
        private string GetImagePath(Cars car, string imageFolder)
        {
            try
            {
                // 1. Ищем по имени из таблицы CarImages
                var dbImage = AppConnect.model01.CarImages.FirstOrDefault(i => i.Car_Id == car.Car_Id);

                if (dbImage != null && !string.IsNullOrEmpty(dbImage.ImageName))
                {
                    string fullPath = Path.Combine(imageFolder, dbImage.ImageName);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }

                // 2. Ищем по ID автомобиля с разными расширениями
                string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp" };
                foreach (string ext in extensions)
                {
                    string pathById = Path.Combine(imageFolder, $"{car.Car_Id}{ext}");
                    if (File.Exists(pathById))
                    {
                        return pathById;
                    }
                }

                // 3. Заглушка
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

                allProducts = displayList;
                lvCars.ItemsSource = displayList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadManufacturers()
        {
            try
            {
                var manufacturers = AppConnect.model01.Manufacturers.Select(m => m.Name).Distinct().OrderBy(m => m).ToList();
                manufacturers.Insert(0, "Все производители");
                cmbManufacturer.ItemsSource = manufacturers;
                cmbManufacturer.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки производителей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                if (allProducts == null || allProducts.Count == 0) return;

                var query = allProducts.AsEnumerable();

                // Поиск
                string searchText = txtSearch.Text?.ToLower() ?? "";
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(p => p.Car.Name.ToLower().Contains(searchText));
                }

                // Фильтр по производителю
                if (cmbManufacturer.SelectedItem != null && cmbManufacturer.SelectedItem.ToString() != "Все производители")
                {
                    string selectedManufacturer = cmbManufacturer.SelectedItem.ToString();
                    query = query.Where(p => p.Car.Models.Manufacturers.Name == selectedManufacturer);
                }

                // Сортировка
                if (cmbSort.SelectedItem != null)
                {
                    string sortOption = cmbSort.SelectedItem.ToString();
                    switch (sortOption)
                    {
                        case "Цена: по возрастанию":
                            query = query.OrderBy(p => p.Price);
                            break;
                        case "Цена: по убыванию":
                            query = query.OrderByDescending(p => p.Price);
                            break;
                        case "Название: А-Я":
                            query = query.OrderBy(p => p.Car.Name);
                            break;
                        case "Название: Я-А":
                            query = query.OrderByDescending(p => p.Car.Name);
                            break;
                        case "Год: новые сначала":
                            query = query.OrderByDescending(p => p.Car.Year);
                            break;
                        case "Год: старые сначала":
                            query = query.OrderBy(p => p.Car.Year);
                            break;
                    }
                }

                lvCars.ItemsSource = query.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckUserRole()
        {
            if (AppFrame.CurrentUserRole == "Администратор" || AppFrame.CurrentUserRole == "Менеджер")
            {
                btnDashboard.Visibility = Visibility.Visible;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void CmbManufacturer_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            cmbManufacturer.SelectedIndex = 0;
            cmbSort.SelectedIndex = 0;
            ApplyFilters();
        }

        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.Tag as ProductDisplay;
            if (product != null)
            {
                string discountInfo = "";
                if (product.Car.DiscountPrice > 0)
                {
                    discountInfo = $"\n💰 Цена со скидкой: {product.Car.DiscountPrice:N0} ₽\n🏷️ Было: {product.Car.Price:N0} ₽";
                }

                MessageBox.Show($"🚗 {product.Car.Name}\n\n" +
                    $"📅 Год: {product.Car.Year}\n" +
                    $"💰 Цена: {product.Price:N0} ₽{discountInfo}\n" +
                    $"🎨 Цвет: {product.Car.Colors?.Name ?? "Неизвестно"}\n" +
                    $"🔧 Двигатель: {product.Car.Models?.EngineTypes?.Name ?? "Неизвестно"} ({product.Car.Models?.Power ?? 0} л.с.)\n" +
                    $"⚙️ Коробка: {product.Car.Models?.Transmissions?.Name ?? "Неизвестно"}\n" +
                    $"🚘 Кузов: {product.Car.Models?.BodyTypes?.Name ?? "Неизвестно"}\n" +
                    $"📦 В наличии: {product.Car.Quantity} шт.\n" +
                    $"📝 Статус: {product.Car.AvailabilityStatus}",
                    "Информация об автомобиле",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new DashboardPage());
        }
        private void BtnAI_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new AIAssistantPage());
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AppFrame.CurrentUser = null;
                AppFrame.CurrentUserID = 0;
                AppFrame.CurrentUserName = "";
                AppFrame.CurrentUserRole = "";
                AppFrame.FrameMain.Navigate(new PageLogin());
            }
        }

        // ========== ДОБАВЛЕННЫЕ МЕТОДЫ ДЛЯ КОРЗИНЫ ==========

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
                    // Проверяем количество на складе
                    if (product.Car.Quantity <= 0)
                    {
                        MessageBox.Show("Товара нет в наличии!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Проверяем, есть ли товар в корзине
                    var existingCart = AppConnect.model01.ShoppingCarts
                        .FirstOrDefault(c => c.User_Id == AppFrame.CurrentUserID && c.Car_Id == product.Car.Car_Id);

                    if (existingCart != null)
                    {
                        // Проверяем, не превышает ли количество то, что есть на складе
                        if (existingCart.Quantity + 1 > product.Car.Quantity)
                        {
                            MessageBox.Show($"Недостаточно товара на складе! Доступно: {product.Car.Quantity} шт.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        existingCart.Quantity++;
                        MessageBox.Show($"Количество товара \"{product.Car.Name}\" увеличено до {existingCart.Quantity}",
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
}