using CarDelershipWPF.AppData;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CarDelershipWPF.Pages
{
    public partial class ProductEditWindow : Window
    {
        private Cars _editingCar;
        private string _selectedImagePath = null;
        private string _imagesFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Images"));

        public ProductEditWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
            LoadStatuses();

            if (!Directory.Exists(_imagesFolder))
                Directory.CreateDirectory(_imagesFolder);

            btnDelete.Visibility = Visibility.Collapsed;
        }

        public ProductEditWindow(Cars car) : this()
        {
            _editingCar = car;
            LoadCarData();
            btnDelete.Visibility = Visibility.Visible;
        }

        private void LoadComboBoxes()
        {
            cmbModel.ItemsSource = AppConnect.model01.Models.ToList();
            cmbModel.DisplayMemberPath = "Name";
            cmbModel.SelectedValuePath = "Model_Id";

            cmbColor.ItemsSource = AppConnect.model01.Colors.ToList();
            cmbColor.DisplayMemberPath = "Name";
            cmbColor.SelectedValuePath = "Color_Id";
        }

        private void LoadStatuses()
        {
            cmbStatus.ItemsSource = new[] { "В наличии", "Под заказ", "Нет в наличии", "Скоро в продаже" };
            cmbStatus.SelectedIndex = 0;
        }

        private void LoadCarData()
        {
            txtName.Text = _editingCar.Name;
            txtPrice.Text = _editingCar.Price.ToString();
            txtDiscountPrice.Text = _editingCar.DiscountPrice.ToString();
            txtQuantity.Text = _editingCar.Quantity.ToString();
            txtYear.Text = _editingCar.Year.ToString();

            cmbModel.SelectedValue = _editingCar.model_Id;
            cmbColor.SelectedValue = _editingCar.Color_Id;
            cmbStatus.SelectedItem = _editingCar.AvailabilityStatus;

            var image = AppConnect.model01.CarImages.FirstOrDefault(i => i.Car_Id == _editingCar.Car_Id);
            if (image != null && !string.IsNullOrEmpty(image.ImageName))
            {
                string fullPath = Path.Combine(_imagesFolder, image.ImageName);
                if (File.Exists(fullPath))
                    imgPreview.Source = new BitmapImage(new Uri(fullPath));
            }
        }

        // ==================== БЛОКИРОВКА ВВОДА ====================

        // Только цифры и точка для цен
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[0-9.,]+$"))
                e.Handled = true;
        }

        // Только цифры для года и количества
        private void IntegerOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[0-9]+$"))
                e.Handled = true;
        }

        private void Number_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        // ==================== ВЫБОР ИЗОБРАЖЕНИЯ ====================

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";
            dialog.Title = "Выберите фото для товара";

            if (dialog.ShowDialog() == true)
            {
                _selectedImagePath = dialog.FileName;
                imgPreview.Source = new BitmapImage(new Uri(dialog.FileName));
            }
        }

        // ==================== СОХРАНЕНИЕ ====================

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверка названия
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название товара", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            // Проверка цены
            if (string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Введите цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return;
            }

            string priceText = txtPrice.Text.Replace(',', '.');
            if (!decimal.TryParse(priceText, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену (положительное число)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return;
            }

            // Проверка цены со скидкой
            decimal discountPrice = 0;
            if (!string.IsNullOrWhiteSpace(txtDiscountPrice.Text))
            {
                string discountText = txtDiscountPrice.Text.Replace(',', '.');
                if (!decimal.TryParse(discountText, out discountPrice) || discountPrice < 0)
                {
                    MessageBox.Show("Введите корректную цену со скидкой (положительное число или 0)", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtDiscountPrice.Focus();
                    return;
                }

                // ПРОВЕРКА: цена со скидкой не может быть больше обычной цены
                if (discountPrice > price)
                {
                    MessageBox.Show("Цена со скидкой не может быть больше обычной цены!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtDiscountPrice.Focus();
                    return;
                }
            }

            // Проверка количества
            if (string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                MessageBox.Show("Введите количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantity.Focus();
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество (целое положительное число)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantity.Focus();
                return;
            }

            // Проверка года
            int year = DateTime.Now.Year;
            if (!string.IsNullOrWhiteSpace(txtYear.Text))
            {
                if (!int.TryParse(txtYear.Text, out year) || year < 1900 || year > DateTime.Now.Year + 1)
                {
                    MessageBox.Show($"Введите корректный год (от 1900 до {DateTime.Now.Year + 1})", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtYear.Focus();
                    return;
                }
            }

            if (cmbModel.SelectedValue == null)
            {
                MessageBox.Show("Выберите модель", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbModel.Focus();
                return;
            }

            if (cmbColor.SelectedValue == null)
            {
                MessageBox.Show("Выберите цвет", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbColor.Focus();
                return;
            }

            try
            {
                if (_editingCar == null)
                {
                    // ДОБАВЛЕНИЕ НОВОГО ТОВАРА
                    Cars newCar = new Cars
                    {
                        Name = txtName.Text.Trim(),
                        Price = price,
                        DiscountPrice = discountPrice,
                        Quantity = quantity,
                        Year = year,
                        model_Id = (int)cmbModel.SelectedValue,
                        Color_Id = (int)cmbColor.SelectedValue,
                        AvailabilityStatus = cmbStatus.SelectedItem?.ToString(),
                        CreatedAt = DateTime.Now,
                        Mileage = 0
                    };
                    AppConnect.model01.Cars.Add(newCar);
                    AppConnect.model01.SaveChanges();

                    if (_selectedImagePath != null)
                    {
                        string extension = Path.GetExtension(_selectedImagePath);
                        string newFileName = $"{newCar.Car_Id}_{DateTime.Now.Ticks}{extension}";
                        string destPath = Path.Combine(_imagesFolder, newFileName);
                        File.Copy(_selectedImagePath, destPath, true);

                        CarImages carImage = new CarImages
                        {
                            Car_Id = newCar.Car_Id,
                            ImageName = newFileName
                        };
                        AppConnect.model01.CarImages.Add(carImage);
                        AppConnect.model01.SaveChanges();
                    }

                    MessageBox.Show("Товар успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // РЕДАКТИРОВАНИЕ ТОВАРА
                    _editingCar.Name = txtName.Text.Trim();
                    _editingCar.Price = price;
                    _editingCar.DiscountPrice = discountPrice;
                    _editingCar.Quantity = quantity;
                    _editingCar.Year = year;
                    _editingCar.model_Id = (int)cmbModel.SelectedValue;
                    _editingCar.Color_Id = (int)cmbColor.SelectedValue;
                    _editingCar.AvailabilityStatus = cmbStatus.SelectedItem?.ToString();

                    if (_selectedImagePath != null)
                    {
                        var oldImage = AppConnect.model01.CarImages.FirstOrDefault(i => i.Car_Id == _editingCar.Car_Id);
                        if (oldImage != null)
                            AppConnect.model01.CarImages.Remove(oldImage);

                        string extension = Path.GetExtension(_selectedImagePath);
                        string newFileName = $"{_editingCar.Car_Id}_{DateTime.Now.Ticks}{extension}";
                        string destPath = Path.Combine(_imagesFolder, newFileName);
                        File.Copy(_selectedImagePath, destPath, true);

                        CarImages newImage = new CarImages
                        {
                            Car_Id = _editingCar.Car_Id,
                            ImageName = newFileName
                        };
                        AppConnect.model01.CarImages.Add(newImage);
                    }

                    AppConnect.model01.SaveChanges();
                    MessageBox.Show("Товар успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== УДАЛЕНИЕ ТОВАРА ====================

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_editingCar == null)
            {
                MessageBox.Show("Нет товара для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить товар \"{_editingCar.Name}\"?\n\nЭто действие невозможно отменить!",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    imgPreview.Source = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    var images = AppConnect.model01.CarImages.Where(i => i.Car_Id == _editingCar.Car_Id).ToList();
                    foreach (var image in images)
                    {
                        AppConnect.model01.CarImages.Remove(image);
                    }

                    AppConnect.model01.Cars.Remove(_editingCar);
                    AppConnect.model01.SaveChanges();

                    MessageBox.Show("Товар успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}