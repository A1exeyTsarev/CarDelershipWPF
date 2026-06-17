using CarDelershipWPF.AppData;
using CarDelershipWPF.Services;
using Microsoft.Win32;
using System;
using CarDelershipWPF.Pages;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CarDelershipWPF.Pages
{
    public partial class OrdersPage : Page
    {
        // Класс для отображения заказа
        public class OrderInfo
        {
            public int Order_Id { get; set; }
            public string OrderNumber { get; set; }
            public string CreatedDate { get; set; }
            public decimal TotalAmount { get; set; }
            public int OrderStatus_Id { get; set; }
            public string StatusName { get; set; }
            public string UserName { get; set; }
        }

        // Класс для сортировки
        public class SortOption
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public OrdersPage()
        {
            InitializeComponent();
            LoadStatusFilter();
            LoadOrders();
        }

        // Загрузка статусов в фильтр
        private void LoadStatusFilter()
        {
            var statuses = AppConnect.model01.OrderStatuses.ToList();
            statuses.Insert(0, new OrderStatuses { OrderStatus_Id = 0, Name = "Все статусы" });
            cmbStatus.ItemsSource = statuses;
            cmbStatus.DisplayMemberPath = "Name";
            cmbStatus.SelectedValuePath = "OrderStatus_Id";
            cmbStatus.SelectedIndex = 0;

            // Варианты сортировки - используем класс вместо анонимного объекта
            var sortOptions = new System.Collections.Generic.List<SortOption>
            {
                new SortOption { Id = 0, Name = "Без сортировки" },
                new SortOption { Id = 1, Name = "По дате (новые)" },
                new SortOption { Id = 2, Name = "По дате (старые)" },
                new SortOption { Id = 3, Name = "По сумме (возрастание)" },
                new SortOption { Id = 4, Name = "По сумме (убывание)" }
            };

            cmbSort.ItemsSource = sortOptions;
            cmbSort.DisplayMemberPath = "Name";
            cmbSort.SelectedValuePath = "Id";
            cmbSort.SelectedIndex = 0;
        }

        // Загрузка заказов
        private void LoadOrders()
        {
            try
            {
                var orders = AppConnect.model01.Orders.ToList();
                var users = AppConnect.model01.Users.ToDictionary(u => u.User_Id, u => u.FullName);
                var statuses = AppConnect.model01.OrderStatuses.ToDictionary(s => s.OrderStatus_Id, s => s.Name);

                var list = orders.Select(o => new OrderInfo
                {
                    Order_Id = o.Order_Id,
                    OrderNumber = o.OrderNumber,
                    CreatedDate = o.CreatedDate.ToString("dd.MM.yyyy HH:mm"),
                    TotalAmount = o.TotalAmount,
                    OrderStatus_Id = o.OrderStatus_Id,
                    StatusName = statuses.ContainsKey(o.OrderStatus_Id) ? statuses[o.OrderStatus_Id] : "Неизвестно",
                    UserName = users.ContainsKey(o.User_Id) ? users[o.User_Id] : "Неизвестно"
                }).ToList();

                // Поиск
                string search = txtSearch.Text?.Trim();
                if (!string.IsNullOrEmpty(search))
                {
                    list = list.Where(o => o.OrderNumber.Contains(search)).ToList();
                }

                // Фильтр по статусу
                if (cmbStatus.SelectedValue != null && (int)cmbStatus.SelectedValue > 0)
                {
                    int statusId = (int)cmbStatus.SelectedValue;
                    list = list.Where(o => o.OrderStatus_Id == statusId).ToList();
                }

                // Сортировка
                if (cmbSort.SelectedValue != null)
                {
                    int sortId = (int)cmbSort.SelectedValue;
                    switch (sortId)
                    {
                        case 1: list = list.OrderByDescending(o => o.CreatedDate).ToList(); break;
                        case 2: list = list.OrderBy(o => o.CreatedDate).ToList(); break;
                        case 3: list = list.OrderBy(o => o.TotalAmount).ToList(); break;
                        case 4: list = list.OrderByDescending(o => o.TotalAmount).ToList(); break;
                    }
                }

                lvOrders.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }


        }



        // Поиск
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadOrders();

        // Фильтр по статусу
        private void cmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadOrders();

        // Сортировка
        private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadOrders();

        // Сброс фильтров
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            cmbStatus.SelectedIndex = 0;
            cmbSort.SelectedIndex = 0;
            LoadOrders();
        }

        // Создать заказ
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddOrderWindow();
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true) LoadOrders();
        }

        // Изменить статус
        private void btnEditStatus_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.Tag as OrderInfo;
            if (item == null) return;

            var order = AppConnect.model01.Orders.FirstOrDefault(o => o.Order_Id == item.Order_Id);
            if (order != null)
            {
                var statuses = AppConnect.model01.OrderStatuses.ToList();
                var dialog = new StatusDialog(order, statuses);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true) LoadOrders();
            }
        }

        // Кнопка назад
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new DashboardPage());
        }
        // QR-код - показать окно с QR-кодом
        private void btnShowQR_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var orderInfo = button?.Tag as OrderInfo;

            if (orderInfo == null)
            {
                MessageBox.Show("Выберите заказ", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var order = AppConnect.model01.Orders.FirstOrDefault(o => o.Order_Id == orderInfo.Order_Id);
            if (order == null) return;

            var dialog = new OrderQRWindow(order);
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }
        // ========== НОВЫЙ МЕТОД ДЛЯ ПЕЧАТИ ЧЕКА ==========

        // Печать чека
        private void btnPrintReceipt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var orderInfo = button?.Tag as OrderInfo;

                if (orderInfo == null)
                {
                    MessageBox.Show("Выберите заказ для печати чека", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Загружаем полные данные заказа
                var order = AppConnect.model01.Orders.FirstOrDefault(o => o.Order_Id == orderInfo.Order_Id);
                if (order == null) return;

                var user = AppConnect.model01.Users.FirstOrDefault(u => u.User_Id == order.User_Id);
                var orderItems = AppConnect.model01.OrderItems.Where(oi => oi.Order_Id == order.Order_Id).ToList();

                // Собираем данные для чека
                var receiptData = new ReceiptGenerator.ReceiptData
                {
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.CreatedDate.ToString("dd.MM.yyyy HH:mm:ss"),
                    Status = orderInfo.StatusName,
                    CustomerName = user?.FullName ?? "Неизвестно",
                    CustomerPhone = user?.Phone,
                    CustomerEmail = user?.Email,
                    CustomerAddress = "",
                    Items = new System.Collections.Generic.List<ReceiptGenerator.OrderItemData>(),
                    Subtotal = 0,
                    DeliveryPrice = 0,
                    TotalAmount = order.TotalAmount,
                    TotalItems = 0
                };

                // Получаем стоимость доставки
                var delivery = AppConnect.model01.DeliveryMethods.FirstOrDefault(d => d.DeliveryMethods_Id == order.DeliveryMethod_Id);
                receiptData.DeliveryPrice = delivery?.Price ?? 0;
                receiptData.Subtotal = order.TotalAmount - receiptData.DeliveryPrice;

                // Добавляем товары
                foreach (var item in orderItems)
                {
                    var car = AppConnect.model01.Cars.FirstOrDefault(c => c.Car_Id == item.Car_Id);
                    receiptData.Items.Add(new ReceiptGenerator.OrderItemData
                    {
                        ProductName = car?.Name ?? "Неизвестно",
                        Quantity = item.Quantity,
                        Price = item.PriceAtPurchase
                    });
                    receiptData.TotalItems += item.Quantity;
                }

                // Диалог сохранения файла
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
                saveFileDialog.FileName = $"Чек_заказа_{order.OrderNumber}.pdf";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Генерируем чек
                    ReceiptGenerator.GenerateReceipt(receiptData, saveFileDialog.FileName);

                    MessageBox.Show($"✅ Чек заказа №{order.OrderNumber} успешно создан!\n\n" +
                        $"Сохранен: {saveFileDialog.FileName}\n\n" +
                        $"Сумма заказа: {order.TotalAmount:N0} ₽",
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Открываем PDF
                    System.Diagnostics.Process.Start(saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка печати чека: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}