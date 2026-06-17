using QRCoder;
using System;
using System.IO;

namespace CarDelershipWPF.Services
{
    public static class QRCodeHelper
    {
        public static string SaveQRCodeToFile(string text, string orderNumber, int pixelsPerModule = 20)
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string fileName = $"QR_{orderNumber}_{DateTime.Now:yyyyMMddHHmmss}.png";
                string filePath = Path.Combine(tempPath, fileName);

                using (var qrGenerator = new QRCodeGenerator())
                using (var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeBytes = qrCode.GetGraphic(pixelsPerModule);
                    File.WriteAllBytes(filePath, qrCodeBytes);
                }

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка генерации QR-кода: {ex.Message}");
            }
        }

        public static void DeleteQRCodeFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try { File.Delete(filePath); } catch { }
            }
        }
    }
}