using CarDelershipWPF.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CarDelershipWPF.Pages
{
    public partial class CheckoutPage : Page
    {
        private List<CartItemDisplay> cartItems;
        private decimal deliveryPrice = 0;

        public CheckoutPage(List<CartItemDisplay> items)
        {
            InitializeComponent();
            cartItems = items;

            txtUserInfo.Text = $"👤 {AppFrame.CurrentUserName}";

            // Заполняем данные пользователя
            var user = AppConnect.model01.Users.FirstOrDefault(u => u.User_Id == AppFrame.CurrentUserID);
            if (user != null)
            {
                txtFullName.Text = user.FullName;
                txtPhone.Text = user.Phone;
                txtEmail.Text = user.Email;
            }

            // Устанавливаем обработчик для изменения способа доставки
            cmbDelivery.SelectionChanged += CmbDelivery_SelectionChanged;

            UpdateTotals();
        }

        private void UpdateTotals()
        {
            int totalItems = cartItems?.Sum(c => c.Quantity) ?? 0;
            decimal subtotal = cartItems?.Sum(c => c.Price * c.Quantity) ?? 0;
            decimal total = subtotal + deliveryPrice;

            txtTotalItems.Text = totalItems.ToString();
            txtSubtotal.Text = subtotal.ToString("N0");
            txtDeliveryPrice.Text = deliveryPrice.ToString("N0");
            txtTotal.Text = total.ToString("N0");
        }

        private void CmbDelivery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDelivery.SelectedItem != null)
            {
                string selected = cmbDelivery.SelectedItem.ToString();

                if (selected.Contains("Самовывоз"))
                {
                    deliveryPrice = 0;
                    // При самовывозе адрес не нужен
                    txtAddress.IsEnabled = false;
                    txtAddress.Text = "";
                    txtAddressError.Visibility = Visibility.Collapsed;
                }
                else
                {
                    deliveryPrice = selected.Contains("по городу") ? 500 :
                                    selected.Contains("по области") ? 1500 : 5000;
                    txtAddress.IsEnabled = true;
                }

                txtDeliveryPrice.Text = deliveryPrice.ToString("N0");
                UpdateTotals();
            }
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMddHHmmss}";
        }

        private int GetOrderStatusId(string statusName)
        {
            var status = AppConnect.model01.OrderStatuses.FirstOrDefault(s => s.Name == statusName);
            return status?.OrderStatus_Id ?? 1;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка заполнения полей
                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    MessageBox.Show("Введите ФИО получателя", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtFullName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPhone.Text))
                {
                    MessageBox.Show("Введите номер телефона", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPhone.Focus();
                    return;
                }

                // Проверка адреса (обязательно только если не самовывоз)
                string deliveryMethod = cmbDelivery.SelectedItem?.ToString() ?? "";
                if (!deliveryMethod.Contains("Самовывоз"))
                {
                    if (string.IsNullOrWhiteSpace(txtAddress.Text))
                    {
                        txtAddressError.Text = "Введите адрес доставки";
                        txtAddressError.Visibility = Visibility.Visible;
                        txtAddress.Focus();
                        return;
                    }
                }
                txtAddressError.Visibility = Visibility.Collapsed;

                // Получаем ID статуса "Новый"
                int orderStatusId = GetOrderStatusId("Новый");

                // Создаем заказ
                var order = new Orders
                {
                    OrderNumber = GenerateOrderNumber(),
                    OrderStatus_Id = orderStatusId,
                    OrderMethod_Id = 2,  // 2 - Онлайн
                    DeliveryMethod_Id = cmbDelivery.SelectedIndex + 1,
                    CreatedDate = DateTime.Now,
                    CompleteDate = null,
                    TotalAmount = decimal.Parse(txtTotal.Text.Replace(" ", "")),
                    User_Id = AppFrame.CurrentUserID
                };

                AppConnect.model01.Orders.Add(order);
                AppConnect.model01.SaveChanges();

                // Добавляем товары в заказ
                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItems
                    {
                        Order_Id = order.Order_Id,
                        Car_Id = item.Car.Car_Id,
                        Quantity = item.Quantity,
                        PriceAtPurchase = item.Price
                    };
                    AppConnect.model01.OrderItems.Add(orderItem);

                    // Уменьшаем количество товара на складе
                    item.Car.Quantity -= item.Quantity;
                }

                // Очищаем корзину
                foreach (var item in cartItems)
                {
                    AppConnect.model01.ShoppingCarts.Remove(item.CartItem);
                }

                AppConnect.model01.SaveChanges();

                // Формируем сообщение о доставке
                string deliveryInfo = deliveryMethod.Contains("Самовывоз")
                    ? "Самовывоз"
                    : $"Доставка по адресу: {txtAddress.Text}";

                MessageBox.Show($"✅ Заказ №{order.OrderNumber} успешно оформлен!\n\n" +
                    $"Сумма заказа: {order.TotalAmount:N0} ₽\n" +
                    $"Способ доставки: {deliveryMethod}\n" +
                    $"Способ оплаты: {cmbPayment.SelectedItem}\n" +
                    $"📦 {deliveryInfo}\n\n" +
                    "Наш менеджер свяжется с вами для подтверждения заказа.",
                    "Заказ оформлен", MessageBoxButton.OK, MessageBoxImage.Information);

                // Возвращаемся в магазин
                AppFrame.FrameMain.Navigate(new MainShopPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new CartPage());
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
}