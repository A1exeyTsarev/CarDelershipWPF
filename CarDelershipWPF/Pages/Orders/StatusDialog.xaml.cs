using CarDelershipWPF.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CarDelershipWPF
{
    public partial class StatusDialog : Window
    {
        private Orders _order;

        public StatusDialog(Orders order, List<OrderStatuses> statuses)
        {
            InitializeComponent();
            _order = order;
            cmbStatus.ItemsSource = statuses;
            cmbStatus.SelectedValue = order.OrderStatus_Id;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (cmbStatus.SelectedValue != null)
            {
                int newStatus = (int)cmbStatus.SelectedValue;
                if (_order.OrderStatus_Id != newStatus)
                {
                    _order.OrderStatus_Id = newStatus;
                    AppConnect.model01.SaveChanges();
                }
            }
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}