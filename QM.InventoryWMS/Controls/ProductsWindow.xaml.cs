using ExcelHelper;
using Microsoft.Win32;
using QM.Inventory.TunnelsClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private List<Product> Products { get; set; }
        public ProductsWindow(User currentUser) {
            this.currentUser = currentUser;
            InitializeComponent();
            LoadProductsDataGrid();
        }

        private void CalculateTotal()
        {
            if (dgProducts.Items.Count > 0)
            {
                lblSumValue.Content = dgProducts.ItemsSource.Cast<Product>().Sum(x => x.CurrentValue).ToString("0.##");

            }
            else
            {
                lblSumValue.Content = 0.ToString();
            }
        }

        private async void LoadProductsDataGrid() {
            var products = await TunnelsClient.GetAllProductsAsync(null);
            foreach (var item in products)
            {
                item.CurrentValue = Convert.ToDouble(item.CurrentValue.ToString("0.##"));
                item.CurrentQuantity = Convert.ToDouble(item.CurrentQuantity.ToString("0.##"));
            }
            Products = products;
            dgProducts.ItemsSource = products;

            CalculateTotal();
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

        private async void btnUpdateProducts(object sender, RoutedEventArgs e)
        {
            var productList = dgProducts.Items.Cast<Product>();
            await TunnelsClient.UpdateAllProductsAsync(productList);
        }

        private void cbStock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Products != null)
                switch (cbStock.SelectedIndex)
                {
                    case 0: dgProducts.ItemsSource = Products.FindAll(x => x.IsActive == true);
                        break;
                    case 1:
                        dgProducts.ItemsSource = Products.FindAll(x => x.IsActive == false);
                        break;
                }

            CalculateTotal();
        }

        private void ckBeditMode_Checked(object sender, RoutedEventArgs e)
        {
            if(ckBeditMode.IsChecked == true)
            {
                dgProducts.IsReadOnly = false;
                btnChangeActiveStatus.IsEnabled = false;
            }

        }

        private void ckBeditMode_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ckBeditMode.IsChecked == false)
            {
                dgProducts.IsReadOnly = true;
                btnChangeActiveStatus.IsEnabled = true;
            }
        }

        private async void btnChangeActiveStatus_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedIndex > -1)
            {
                if (dgProducts.SelectedItem is Product product)
                {
                    product.IsActive = !product.IsActive;
                    var productList = new List<Product>
                    {
                        product
                    };
                    await TunnelsClient.UpdateAllProductsAsync(productList);
                }
            }
            LoadProductsDataGrid();
        }

        private void btnSaveAsExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "Report " + DateTime.Now.ToString("yyyy-dd-MM HH-mm-ss");
            saveFileDialog.DefaultExt = ".xlsx";
            saveFileDialog.Filter = "Excel (*.xlsx)|*.xlsx";
            if (saveFileDialog.ShowDialog() == true)
            {
                CreateExcelFile.CreateExcelDocument(Products, saveFileDialog.FileName, false);
                Process.Start(new ProcessStartInfo { FileName = saveFileDialog.FileName, UseShellExecute = true });
            }
        }
    }
}
