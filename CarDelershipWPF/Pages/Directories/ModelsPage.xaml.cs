using CarDelershipWPF.AppData;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CarDelershipWPF.Pages.Directories
{
    public partial class ModelsPage : Page
    {
        public ModelsPage()
        {
            InitializeComponent();
            LoadData();

            // Двойной клик для редактирования
            dgItems.MouseDoubleClick += DgItems_MouseDoubleClick;
        }

        private void LoadData()
        {
            try
            {
                var models = AppConnect.model01.Models.ToList();
                var manufacturers = AppConnect.model01.Manufacturers.ToDictionary(m => m.Manufacturer_Id, m => m.Name);
                var bodyTypes = AppConnect.model01.BodyTypes.ToDictionary(b => b.BodyType_Id, b => b.Name);
                var engineTypes = AppConnect.model01.EngineTypes.ToDictionary(e => e.EngineType_Id, e => e.Name);
                var transmissions = AppConnect.model01.Transmissions.ToDictionary(t => t.Transmission_Id, t => t.Name);

                var result = models.Select(m => new
                {
                    m.Model_Id,
                    m.Name,
                    m.Power,
                    Производитель = manufacturers.ContainsKey(m.Manufacturer_Id) ? manufacturers[m.Manufacturer_Id] : "Не указан",
                    ТипКузова = bodyTypes.ContainsKey(m.BodyType_Id) ? bodyTypes[m.BodyType_Id] : "Не указан",
                    Двигатель = engineTypes.ContainsKey(m.EngineType_Id) ? engineTypes[m.EngineType_Id] : "Не указан",
                    КПП = transmissions.ContainsKey(m.Transmission_Id) ? transmissions[m.Transmission_Id] : "Не указан"
                }).OrderBy(m => m.Name).ToList();

                dgItems.ItemsSource = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EditModelDialog();
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void DgItems_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgItems.SelectedItem != null)
            {
                var selected = dgItems.SelectedItem;
                var id = (int)selected.GetType().GetProperty("Model_Id")?.GetValue(selected);

                var model = AppConnect.model01.Models.FirstOrDefault(m => m.Model_Id == id);
                if (model != null)
                {
                    var dialog = new EditModelDialog(model);
                    dialog.Owner = Window.GetWindow(this);
                    if (dialog.ShowDialog() == true)
                    {
                        LoadData();
                    }
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (AppFrame.FrameMain.CanGoBack)
                AppFrame.FrameMain.GoBack();
            else
                AppFrame.FrameMain.Navigate(new DirectoriesMenuPage());
        }
    }
}