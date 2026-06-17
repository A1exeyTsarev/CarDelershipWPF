using CarDelershipWPF.AppData;
using System;
using System.Linq;
using System.Windows;

namespace CarDelershipWPF.Pages.Directories
{
    public partial class EditModelDialog : Window
    {
        private Models _editingModel;
        private bool _isEditMode = false;

        // Конструктор для добавления новой модели
        public EditModelDialog()
        {
            InitializeComponent();
            Title = "Добавление модели";
            LoadComboBoxes();
        }

        // Конструктор для редактирования существующей модели
        public EditModelDialog(Models model)
        {
            InitializeComponent();
            Title = "Редактирование модели";
            _editingModel = model;
            _isEditMode = true;

            LoadComboBoxes();
            LoadModelData();
        }

        private void LoadComboBoxes()
        {
            try
            {
                // Производители
                var manufacturers = AppConnect.model01.Manufacturers
                    .Select(m => new { Manufacturer_Id = m.Manufacturer_Id, Name = m.Name })
                    .OrderBy(m => m.Name)
                    .ToList();
                cmbManufacturer.ItemsSource = manufacturers;
                cmbManufacturer.DisplayMemberPath = "Name";
                cmbManufacturer.SelectedValuePath = "Manufacturer_Id";

                // Типы кузова
                var bodyTypes = AppConnect.model01.BodyTypes
                    .Select(b => new { BodyType_Id = b.BodyType_Id, Name = b.Name })
                    .OrderBy(b => b.Name)
                    .ToList();
                cmbBodyType.ItemsSource = bodyTypes;
                cmbBodyType.DisplayMemberPath = "Name";
                cmbBodyType.SelectedValuePath = "BodyType_Id";

                // Типы двигателя
                var engineTypes = AppConnect.model01.EngineTypes
                    .Select(e => new { EngineType_Id = e.EngineType_Id, Name = e.Name })
                    .OrderBy(e => e.Name)
                    .ToList();
                cmbEngineType.ItemsSource = engineTypes;
                cmbEngineType.DisplayMemberPath = "Name";
                cmbEngineType.SelectedValuePath = "EngineType_Id";

                // КПП
                var transmissions = AppConnect.model01.Transmissions
                    .Select(t => new { Transmission_Id = t.Transmission_Id, Name = t.Name })
                    .OrderBy(t => t.Name)
                    .ToList();
                cmbTransmission.ItemsSource = transmissions;
                cmbTransmission.DisplayMemberPath = "Name";
                cmbTransmission.SelectedValuePath = "Transmission_Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void LoadModelData()
        {
            if (_editingModel != null)
            {
                txtName.Text = _editingModel.Name;
                txtPower.Text = _editingModel.Power.ToString();

                cmbManufacturer.SelectedValue = _editingModel.Manufacturer_Id;
                cmbBodyType.SelectedValue = _editingModel.BodyType_Id;
                cmbEngineType.SelectedValue = _editingModel.EngineType_Id;
                cmbTransmission.SelectedValue = _editingModel.Transmission_Id;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название модели", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtPower.Text, out int power) || power <= 0)
            {
                MessageBox.Show("Введите корректную мощность (л.с.)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbManufacturer.SelectedValue == null)
            {
                MessageBox.Show("Выберите производителя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode && _editingModel != null)
                {
                    // Редактирование
                    _editingModel.Name = txtName.Text.Trim();
                    _editingModel.Manufacturer_Id = (int)cmbManufacturer.SelectedValue;
                    _editingModel.BodyType_Id = (int)cmbBodyType.SelectedValue;
                    _editingModel.EngineType_Id = (int)cmbEngineType.SelectedValue;
                    _editingModel.Transmission_Id = (int)cmbTransmission.SelectedValue;
                    _editingModel.Power = power;
                }
                else
                {
                    // Добавление новой модели
                    var newModel = new Models
                    {
                        Name = txtName.Text.Trim(),
                        Manufacturer_Id = (int)cmbManufacturer.SelectedValue,
                        BodyType_Id = (int)cmbBodyType.SelectedValue,
                        EngineType_Id = (int)cmbEngineType.SelectedValue,
                        Transmission_Id = (int)cmbTransmission.SelectedValue,
                        Power = power
                    };
                    AppConnect.model01.Models.Add(newModel);
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

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}