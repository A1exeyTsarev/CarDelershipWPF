using CarDelershipWPF.AppData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CarDelershipWPF.Pages
{
    public partial class CartPage : Page
    {
        private List<CartItemDisplay> cartItems;

        public CartPage()
        {
            InitializeComponent();
            txtUserInfo.Text = $"👤 {AppFrame.CurrentUserName}";
            LoadCart();
        }

        private string GetImagePath(Cars car)
        {
            try
            {
                var dbImage = AppConnect.model01.CarImages.FirstOrDefault(i => i.Car_Id == car.Car_Id);
                if (dbImage != null && !string.IsNullOrEmpty(dbImage.ImageName))
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string projectPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, @"..\..\.."));
                    string imageFolder = System.IO.Path.Combine(projectPath, "Images");
                    string fullPath = System.IO.Path.Combine(imageFolder, dbImage.ImageName);
                    if (System.IO.File.Exists(fullPath))
                        return fullPath;
                }
                return string.Empty;
            }
            catch { return string.Empty; }
        }

        private void LoadCart()
        {
            try
            {
                var cart = AppConnect.model01.ShoppingCarts
                    .Where(c => c.User_Id == AppFrame.CurrentUserID)
                    .ToList();

                cartItems = new List<CartItemDisplay>();

                foreach (var item in cart)
                {
                    var car = AppConnect.model01.Cars.FirstOrDefault(c => c.Car_Id == item.Car_Id);
                    if (car != null)
                    {
                        cartItems.Add(new CartItemDisplay
                        {
                            CartItem = item,
                            Car = car,
                            ImagePath = GetImagePath(car),
                            CarName = car.Name,
                            Price = car.DiscountPrice > 0 ? car.DiscountPrice : car.Price,
                            Quantity = item.Quantity
                        });
                    }
                }

                lvCart.ItemsSource = cartItems;
                UpdateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTotals()
        {
            int totalItems = cartItems?.Sum(c => c.Quantity) ?? 0;
            decimal totalPrice = cartItems?.Sum(c => c.Price * c.Quantity) ?? 0;

            txtTotalItems.Text = totalItems.ToString();
            txtTotalPrice.Text = totalPrice.ToString("N0");
        }

        private void BtnIncrease_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as CartItemDisplay;
            if (item != null)
            {
                if (item.Quantity + 1 > item.Car.Quantity)
                {
                    MessageBox.Show($"Недостаточно товара на складе! Доступно: {item.Car.Quantity} шт.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                item.CartItem.Quantity++;
                AppConnect.model01.SaveChanges();
                item.Quantity = item.CartItem.Quantity;
                lvCart.ItemsSource = null;
                lvCart.ItemsSource = cartItems;
                UpdateTotals();
            }
        }

        private void BtnDecrease_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as CartItemDisplay;
            if (item != null && item.CartItem.Quantity > 1)
            {
                item.CartItem.Quantity--;
                AppConnect.model01.SaveChanges();
                item.Quantity = item.CartItem.Quantity;
                lvCart.ItemsSource = null;
                lvCart.ItemsSource = cartItems;
                UpdateTotals();
            }
            else if (item != null && item.CartItem.Quantity == 1)
            {
                BtnRemove_Click(sender, e);
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as CartItemDisplay;
            if (item != null)
            {
                var result = MessageBox.Show($"Удалить товар \"{item.CarName}\" из корзины?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    AppConnect.model01.ShoppingCarts.Remove(item.CartItem);
                    AppConnect.model01.SaveChanges();
                    cartItems.Remove(item);
                    lvCart.ItemsSource = null;
                    lvCart.ItemsSource = cartItems;
                    UpdateTotals();
                }
            }
        }

        private void BtnClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (cartItems == null || cartItems.Count == 0)
            {
                MessageBox.Show("Корзина уже пуста!", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show("Очистить всю корзину?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var item in cartItems)
                {
                    AppConnect.model01.ShoppingCarts.Remove(item.CartItem);
                }
                AppConnect.model01.SaveChanges();
                cartItems.Clear();
                lvCart.ItemsSource = null;
                UpdateTotals();

                MessageBox.Show("Корзина очищена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (cartItems == null || cartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста! Добавьте товары для оформления заказа.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AppFrame.FrameMain.Navigate(new CheckoutPage(cartItems));
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new MainShopPage());
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
    }

    public class CartItemDisplay
    {
        public ShoppingCarts CartItem { get; set; }
        public Cars Car { get; set; }
        public string ImagePath { get; set; }
        public string CarName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}