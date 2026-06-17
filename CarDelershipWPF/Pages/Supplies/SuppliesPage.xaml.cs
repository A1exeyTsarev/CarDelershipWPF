using CarDelershipWPF.AppData;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CarDelershipWPF.Pages
{
    public partial class SuppliesPage : Page
    {
        public SuppliesPage()
        {
            InitializeComponent();
            LoadStatusFilter();
            LoadSupplies();
        }

        // Загрузка фильтра статусов
        private void LoadStatusFilter()
        {
            var statuses = AppConnect.model01.StatusSupplies.ToList();
            // Используем правильное имя класса StatusSupplies
            statuses.Insert(0, new StatusSupplies { StatusSupply_Id = 0, StatusName = "Все статусы" });
            cmbStatus.ItemsSource = statuses;
            cmbStatus.DisplayMemberPath = "StatusName";
            cmbStatus.SelectedValuePath = "StatusSupply_Id";
            cmbStatus.SelectedIndex = 0;
        }

        // Загрузка поставок
        private void LoadSupplies()
        {
            try
            {
                var supplies = AppConnect.model01.Supplies.ToList();
                var suppliers = AppConnect.model01.Suppliers.ToDictionary(s => s.Supplier_Id, s => s.Name);
                var statuses = AppConnect.model01.StatusSupplies.ToDictionary(s => s.StatusSupply_Id, s => s.StatusName);

                var list = supplies.Select(s => new
                {
                    s.Supply_Id,
                    s.Supplier_Id,
                    SupplierName = suppliers.ContainsKey(s.Supplier_Id) ? suppliers[s.Supplier_Id] : "Неизвестно",
                    CreatedDate = s.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                    CompletedDate = s.CompletedAt.HasValue ? s.CompletedAt.Value.ToString("dd.MM.yyyy HH:mm") : "—",
                    s.Status_Id,
                    StatusName = statuses.ContainsKey(s.Status_Id) ? statuses[s.Status_Id] : "Неизвестно"
                }).OrderByDescending(s => s.CreatedDate).ToList();

                // Фильтр по статусу
                if (cmbStatus.SelectedValue != null && (int)cmbStatus.SelectedValue > 0)
                {
                    int statusId = (int)cmbStatus.SelectedValue;
                    list = list.Where(s => s.Status_Id == statusId).ToList();
                }

                dgSupplies.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Фильтр по статусу
        private void cmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSupplies();
        }

        // Создать поставку
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddSupplyWindow();
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
                LoadSupplies();
        }

        // Редактировать поставку
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            dynamic item = (sender as Button)?.Tag;
            if (item == null) return;

            int supplyId = item.Supply_Id;
            var supply = AppConnect.model01.Supplies.FirstOrDefault(s => s.Supply_Id == supplyId);
            if (supply != null)
            {
                var dialog = new AddSupplyWindow(supply);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                    LoadSupplies();
            }
        }

        // Изменение статуса
        private void cmbSupplyStatus_Changed(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null) return;

            dynamic item = combo.Tag;
            if (item == null || combo.SelectedValue == null) return;

            int supplyId = item.Supply_Id;
            int newStatusId = (int)combo.SelectedValue;

            var supply = AppConnect.model01.Supplies.FirstOrDefault(s => s.Supply_Id == supplyId);
            if (supply != null && supply.Status_Id != newStatusId)
            {
                supply.Status_Id = newStatusId;
                // Если статус "Завершено" (ID=3), ставим дату завершения
                if (newStatusId == 3)
                    supply.CompletedAt = DateTime.Now;

                AppConnect.model01.SaveChanges();
                LoadSupplies();

                MessageBox.Show("Статус поставки изменен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Назад
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (AppFrame.FrameMain.CanGoBack)
                AppFrame.FrameMain.GoBack();
            else
                AppFrame.FrameMain.Navigate(new DashboardPage());
        }
    }
}