using QM.Inventory.TunnelsClient;
using System;
using System.Collections.Generic;
using System.Windows;
using Tunnels.Core.Models;
using User = Tunnels.Core.Models.User;

namespace QM.InventoryWMS.Controls {
    public partial class AddUserWindow : Window {
        private List<User> _listUsers;

        public List<User> ListUsers {
            get { return _listUsers; }
            set { _listUsers = value; }
        }
        public AddUserWindow() {
            InitializeComponent();
            LoadData();
            DataContext = this;
        }

        private async void LoadData() {
            ListUsers = await TunnelsClient.GetAllUsersAsync();
            dgUsers.ItemsSource = ListUsers;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private async void btnAddUser_Click(object sender, RoutedEventArgs e) {

            await TunnelsClient.CreateUser(new User {
                DateAdded = DateTime.Now,
                Username = tbUsername.Text,
                Name = tbName.Text,
                Password = tbPassword.Text,
                Role = (RolesEnum)cbRole.SelectedIndex
            });
        }

        public void DeleteSelectedProduct(object sender, RoutedEventArgs e) {
            var selectedUser = dgUsers.SelectedItem as User;
            if (selectedUser != null) {
                dgUsers.CancelEdit();
                ListUsers.Remove(selectedUser);
                dgUsers.Items.Refresh();
            }
        }
    }
}
