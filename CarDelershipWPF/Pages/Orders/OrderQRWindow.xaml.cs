using CarDelershipWPF.AppData;
using QRCoder;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CarDelershipWPF.Pages
{
    public partial class OrderQRWindow : Window
    {
        private Orders _order;

        public OrderQRWindow(Orders order)
        {
            InitializeComponent();
            _order = order;
            LoadOrderData();
            LoadQR();
        }

        private void LoadOrderData()
        {
            txtOrderNumber.Text = _order.OrderNumber;

            var status = AppConnect.model01.OrderStatuses.FirstOrDefault(s => s.OrderStatus_Id == _order.OrderStatus_Id);
            txtStatus.Text = status?.Name ?? "Неизвестно";

            var user = AppConnect.model01.Users.FirstOrDefault(u => u.User_Id == _order.User_Id);
            txtCustomer.Text = user?.FullName ?? "Неизвестно";

            txtTotalAmount.Text = $"{_order.TotalAmount:N0} ₽";
        }

        private void LoadQR()
        {
            try
            {
                // Получаем товары в заказе
                var orderItems = AppConnect.model01.OrderItems.Where(oi => oi.Order_Id == _order.Order_Id).ToList();

                // Упрощённая информация для QR-кода (чтобы не превысить лимит)
                var sb = new StringBuilder();
                sb.AppendLine($"Заказ №{_order.OrderNumber}");
                sb.AppendLine($"Статус: {txtStatus.Text}");
                sb.AppendLine($"Сумма: {_order.TotalAmount:N0} ₽");
                sb.AppendLine($"Дата: {_order.CreatedDate:dd.MM.yyyy HH:mm}");
                sb.AppendLine($"Клиент: {txtCustomer.Text}");
                sb.AppendLine("");
                sb.AppendLine("Товары:");

                foreach (var item in orderItems)
                {
                    var car = AppConnect.model01.Cars.FirstOrDefault(c => c.Car_Id == item.Car_Id);
                    if (car != null)
                    {
                        var model = AppConnect.model01.Models.FirstOrDefault(m => m.Model_Id == car.model_Id);
                        var manufacturer = AppConnect.model01.Manufacturers.FirstOrDefault(m => m.Manufacturer_Id == model.Manufacturer_Id);

                        string carName = $"{manufacturer?.Name ?? "Неизвестно"} {model?.Name ?? "Неизвестно"}";

                        sb.AppendLine($"  {carName} - {item.Quantity}шт x {item.PriceAtPurchase:N0} ₽");
                    }
                }

                sb.AppendLine("");
                sb.AppendLine($"Итого: {_order.TotalAmount:N0} ₽");
                sb.AppendLine("Спасибо за покупку!");

                string qrData = sb.ToString();

                // Генерация QR-кода с меньшим уровнем коррекции (L вместо H)
                using (var qrGenerator = new QRCodeGenerator())
                using (var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.L))
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeBytes = qrCode.GetGraphic(20);

                    var bitmapImage = new BitmapImage();
                    using (var stream = new MemoryStream(qrCodeBytes))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = stream;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                    }

                    imgQR.Source = bitmapImage;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации QR-кода: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}