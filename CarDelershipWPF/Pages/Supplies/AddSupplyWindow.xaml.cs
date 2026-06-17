using CarDelershipWPF.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CarDelershipWPF.Pages
{
    public partial class AddSupplyWindow : Window
    {
        private Supplies _editingSupply;
        private List<SupplyItemDisplay> _items = new List<SupplyItemDisplay>();

        public class SupplyItemDisplay
        {
            public int Car_Id { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
        }

        public AddSupplyWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
        }

        public AddSupplyWindow(Supplies supply)
        {
            InitializeComponent();
            _editingSupply = supply;
            LoadComboBoxes();
            LoadSupplyData();
        }

        private void LoadComboBoxes()
        {
            // Поставщики
            cmbSupplier.ItemsSource = AppConnect.model01.Suppliers.ToList();
            if (cmbSupplier.Items.Count > 0) cmbSupplier.SelectedIndex = 0;

            // Статусы
            cmbStatus.ItemsSource = AppConnect.model01.StatusSupplies.ToList();
            if (cmbStatus.Items.Count > 0) cmbStatus.SelectedIndex = 0;

            // Товары
            cmbProduct.ItemsSource = AppConnect.model01.Cars.ToList();
            if (cmbProduct.Items.Count > 0) cmbProduct.SelectedIndex = 0;
        }

        private void LoadSupplyData()
        {
            if (_editingSupply != null)
            {
                cmbSupplier.SelectedValue = _editingSupply.Supplier_Id;
                cmbStatus.SelectedValue = _editingSupply.Status_Id;

                // Загружаем товары из поставки
                var items = AppConnect.model01.SupplyItems
                    .Where(i => i.Supply_Id == _editingSupply.Supply_Id)
                    .ToList();

                foreach (var item in items)
                {
                    var product = AppConnect.model01.Cars.FirstOrDefault(c => c.Car_Id == item.Car_Id);
                    _items.Add(new SupplyItemDisplay
                    {
                        Car_Id = item.Car_Id,
                        ProductName = product?.Name ?? "Неизвестно",
                        Quantity = item.Quantity
                    });
                }
                UpdateItemsGrid();
            }
        }

        private void UpdateItemsGrid()
        {
            dgItems.ItemsSource = null;
            dgItems.ItemsSource = _items;
        }

        // Валидация для числового ввода (только цифры)
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        // Блокировка пробела
        private void Number_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
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
                _items.Add(new SupplyItemDisplay
                {
                    Car_Id = product.Car_Id,
                    ProductName = product.Name,
                    Quantity = qty
                });
            }

            txtQuantity.Text = "";
            UpdateItemsGrid();
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.Tag as SupplyItemDisplay;
            if (item != null)
            {
                _items.Remove(item);
                UpdateItemsGrid();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("Добавьте товары в поставку", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_editingSupply == null)
                {
                    // НОВАЯ ПОСТАВКА
                    var supply = new Supplies
                    {
                        Supplier_Id = (int)cmbSupplier.SelectedValue,
                        Status_Id = (int)cmbStatus.SelectedValue,
                        CreatedAt = DateTime.Now
                    };
                    AppConnect.model01.Supplies.Add(supply);
                    AppConnect.model01.SaveChanges();

                    // Добавляем товары
                    foreach (var item in _items)
                    {
                        var supplyItem = new SupplyItems
                        {
                            Supply_Id = supply.Supply_Id,
                            Car_Id = item.Car_Id,
                            Quantity = item.Quantity
                        };
                        AppConnect.model01.SupplyItems.Add(supplyItem);
                    }
                }
                else
                {
                    // РЕДАКТИРОВАНИЕ ПОСТАВКИ
                    _editingSupply.Supplier_Id = (int)cmbSupplier.SelectedValue;
                    _editingSupply.Status_Id = (int)cmbStatus.SelectedValue;

                    // Если статус "Завершено", ставим дату завершения
                    if (_editingSupply.Status_Id == 3 && _editingSupply.CompletedAt == null)
                        _editingSupply.CompletedAt = DateTime.Now;

                    // Удаляем старые товары
                    var oldItems = AppConnect.model01.SupplyItems
                        .Where(i => i.Supply_Id == _editingSupply.Supply_Id)
                        .ToList();
                    foreach (var item in oldItems)
                        AppConnect.model01.SupplyItems.Remove(item);

                    // Добавляем новые товары
                    foreach (var item in _items)
                    {
                        var supplyItem = new SupplyItems
                        {
                            Supply_Id = _editingSupply.Supply_Id,
                            Car_Id = item.Car_Id,
                            Quantity = item.Quantity
                        };
                        AppConnect.model01.SupplyItems.Add(supplyItem);
                    }
                }

                AppConnect.model01.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}