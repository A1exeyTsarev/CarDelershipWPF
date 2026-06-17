using CarDelershipWPF.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CarDelershipWPF.Pages
{
    public partial class AddOrderWindow : Window
    {
        private List<OrderItem> _items = new List<OrderItem>();

        public class OrderItem
        {
            public int Car_Id { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Total => Quantity * Price;
        }

        public AddOrderWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            try
            {
                // Пользователи
                cmbUser.ItemsSource = AppConnect.model01.Users.ToList();
                if (cmbUser.Items.Count > 0) cmbUser.SelectedIndex = 0;

                // Статусы заказов
                cmbStatus.ItemsSource = AppConnect.model01.OrderStatuses.ToList();
                if (cmbStatus.Items.Count > 0) cmbStatus.SelectedIndex = 0;

                // Способы доставки
                cmbDelivery.ItemsSource = AppConnect.model01.DeliveryMethods.ToList();
                if (cmbDelivery.Items.Count > 0) cmbDelivery.SelectedIndex = 0;

                // Товары
                cmbProduct.ItemsSource = AppConnect.model01.Cars.ToList();
                if (cmbProduct.Items.Count > 0) cmbProduct.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void UpdateTotal()
        {
            decimal total = _items.Sum(i => i.Total);
            lblTotal.Text = $"{total:N0} ₽";
            dgItems.ItemsSource = null;
            dgItems.ItemsSource = _items;
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            var product = cmbProduct.SelectedItem as Cars;
            if (product == null)
            {
                MessageBox.Show("Выберите товар", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Введите количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existing = _items.FirstOrDefault(i => i.Car_Id == product.Car_Id);
            if (existing != null)
            {
                existing.Quantity += qty;
            }
            else
            {
                _items.Add(new OrderItem
                {
                    Car_Id = product.Car_Id,
                    ProductName = product.Name,
                    Quantity = qty,
                    Price = product.Price
                });
            }

            txtQuantity.Text = "";
            UpdateTotal();
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as OrderItem;
            if (item != null)
            {
                _items.Remove(item);
                UpdateTotal();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("Добавьте товары в заказ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Генерируем номер заказа
                string orderNumber = $"ORD-{DateTime.Now:yyyyMMddHHmmss}";
                decimal total = _items.Sum(i => i.Total);

                // Создаем заказ
                var order = new Orders
                {
                    OrderNumber = orderNumber,
                    User_Id = (int)cmbUser.SelectedValue,
                    OrderStatus_Id = (int)cmbStatus.SelectedValue,
                    DeliveryMethod_Id = (int)cmbDelivery.SelectedValue,
                    CreatedDate = DateTime.Now,
                    TotalAmount = total,
                    OrderMethod_Id = 1, // 1 - через сайт
                    CompleteDate = null
                };

                AppConnect.model01.Orders.Add(order);
                AppConnect.model01.SaveChanges();

                // Добавляем товары в заказ
                foreach (var item in _items)
                {
                    var orderItem = new OrderItems
                    {
                        Order_Id = order.Order_Id,
                        Car_Id = item.Car_Id,
                        Quantity = item.Quantity,
                        PriceAtPurchase = item.Price
                    };
                    AppConnect.model01.OrderItems.Add(orderItem);
                }

                AppConnect.model01.SaveChanges();

                MessageBox.Show($"Заказ #{orderNumber} успешно создан!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}