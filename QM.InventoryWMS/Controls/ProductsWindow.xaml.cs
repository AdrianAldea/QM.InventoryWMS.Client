using QM.Inventory.TunnelsClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Tunnels.Core.Models;

namespace QM.InventoryWMS.Controls {
    /// <summary>
    /// Interaction logic for ProductsWindow.xaml
    /// </summary>
    public partial class ProductsWindow : Window {
        private User currentUser { get; set; }
        public ProductsWindow(User currentUser) {
            this.currentUser = currentUser;
            InitializeComponent();
            LoadProductsDataGrid();
        }

        private async void LoadProductsDataGrid() {
            var products = await TunnelsClient.GetAllProductsAsync(null);
            foreach (var item in products)
            {
                item.CurrentValue = Convert.ToDouble(item.CurrentValue.ToString("0.##"));
                item.CurrentQuantity = Convert.ToDouble(item.CurrentQuantity.ToString("0.##"));
            }
            dgProducts.ItemsSource = products;
        }

        private async void btnDeleteProduct_Click(object sender, RoutedEventArgs e) {
            if (dgProducts.SelectedItem != null) {
                await TunnelsClient.DeleteProductById(((Product)dgProducts.SelectedItem).Id);
                LoadProductsDataGrid();
            }
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e) {
            AddProductWindow addProductWindow = new AddProductWindow(currentUser);
            addProductWindow.ShowDialog();
            LoadProductsDataGrid();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void dgProducts_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            var element = (e.EditingElement as TextBox);

            if (dataGrid.SelectedIndex > -1)
            {
                var selectedItem = dataGrid.SelectedItem as Product;

            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var productList = dgProducts.Items.Cast<Product>();
            await TunnelsClient.UpdateAllProductsAsync(productList);
        }
    }
}
