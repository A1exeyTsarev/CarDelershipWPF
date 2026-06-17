using System;
using System.Text;
using System.IO;
using Word = Microsoft.Office.Interop.Word;

namespace CarDelershipWPF.Services
{
    public class ReceiptGenerator
    {
        public class ReceiptData
        {
            public string OrderNumber { get; set; }
            public string OrderDate { get; set; }
            public string Status { get; set; }
            public string CustomerName { get; set; }
            public string CustomerPhone { get; set; }
            public string CustomerEmail { get; set; }
            public string CustomerAddress { get; set; }
            public int TotalItems { get; set; }
            public decimal Subtotal { get; set; }
            public decimal DeliveryPrice { get; set; }
            public decimal TotalAmount { get; set; }
            public System.Collections.Generic.List<OrderItemData> Items { get; set; }
        }

        public class OrderItemData
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }

        public static void GenerateReceipt(ReceiptData data, string outputPath)
        {
            Word.Application wordApp = null;
            Word.Document newDocument = null;
            string qrImagePath = null;

            try
            {
                // Генерируем QR-код
                string qrText = $"Заказ №{data.OrderNumber} | {data.Status} | {data.TotalAmount:N0} ₽";
                qrImagePath = QRCodeHelper.SaveQRCodeToFile(qrText, data.OrderNumber, 20);

                wordApp = new Word.Application();
                wordApp.Visible = false;
                newDocument = wordApp.Documents.Add();

                // Настройка документа
                var range = newDocument.Content;
                range.Font.Name = "Courier New";
                range.Font.Size = 9;

                newDocument.PageSetup.LeftMargin = 20;
                newDocument.PageSetup.RightMargin = 20;
                newDocument.PageSetup.TopMargin = 20;
                newDocument.PageSetup.BottomMargin = 20;

                // Верхняя часть чека
                range.Text += "\n";
                range.Text += "╔════════════════════════════════════════════════╗\n";
                range.Text += "║                🚗 CarDelership 🚗                ║\n";
                range.Text += "╠════════════════════════════════════════════════╣\n";
                range.Text += $"║               ЧЕК ЗАКАЗА №{data.OrderNumber,-15}║\n";
                range.Text += "╠════════════════════════════════════════════════╣\n";
                range.Text += $"║ {data.CustomerName,-46}║\n";
                range.Text += $"║ Тел: {(data.CustomerPhone ?? "Не указан"),-41}║\n";
                range.Text += $"║ {(data.CustomerAddress ?? "Самовывоз"),-46}║\n";
                range.Text += $"║ {data.OrderDate,-46}║\n";
                range.Text += "╠════════════════════════════════════════════════╣\n";
                range.Text += "║                   ТОВАРЫ                      ║\n";
                range.Text += "╠════════════════════════════════════════════════╣\n";

                // Товары
                foreach (var item in data.Items)
                {
                    string name = item.ProductName.Length > 30 ? item.ProductName.Substring(0, 27) + "..." : item.ProductName;
                    range.Text += $"║ {name,-30} ║\n";
                    range.Text += $"║   {item.Quantity} шт x {item.Price:N0} ₽ = {(item.Quantity * item.Price):N0} ₽║\n";
                    range.Text += "╠════════════════════════════════════════════════╣\n";
                }

                // Итог
                range.Text += $"║ Товаров: {data.TotalItems,2} шт{new string(' ', 30)}║\n";
                range.Text += $"║ Сумма: {data.Subtotal,10:N0} ₽{new string(' ', 24)}║\n";
                if (data.DeliveryPrice > 0)
                {
                    range.Text += $"║ Доставка: {data.DeliveryPrice,10:N0} ₽{new string(' ', 24)}║\n";
                }
                range.Text += "╠════════════════════════════════════════════════╣\n";
                range.Text += $"║ ИТОГО: {data.TotalAmount,12:N0} ₽{new string(' ', 22)}║\n";
                range.Text += "╚════════════════════════════════════════════════╝\n\n";

                // Добавляем QR-код отдельно
                range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                // Добавляем текст перед QR-кодом
                range.Text += "Отсканируйте QR-код для отслеживания заказа:\n\n";

                // Вставляем QR-код
                var shapeRange = range.InlineShapes.AddPicture(qrImagePath);
                shapeRange.Width = 120;
                shapeRange.Height = 120;

                // Перемещаем курсор после картинки
                range.Collapse(Word.WdCollapseDirection.wdCollapseEnd);

                // Добавляем подпись
                range.Text += "\n\n";
                range.Text += "Спасибо за покупку!\n";
                range.Text += "Ждем вас снова в CarDelership\n\n";
                range.Text += "Чек действителен при предъявлении\n\n";
                range.Text += "В случае вопросов звоните:\n";
                range.Text += "+7 (XXX) XXX-XX-XX";

                // Сохранение в PDF
                newDocument.SaveAs2(outputPath, Word.WdSaveFormat.wdFormatPDF);
            }
            finally
            {
                if (!string.IsNullOrEmpty(qrImagePath))
                {
                    QRCodeHelper.DeleteQRCodeFile(qrImagePath);
                }

                if (newDocument != null)
                {
                    newDocument.Close(false);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(newDocument);
                }
                if (wordApp != null)
                {
                    wordApp.Quit(false);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
                }
            }
        }
    }
}