using QM.Inventory.TunnelsClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Tunnels.Core.Models;

namespace QM.InventoryWMS.Controls {
    public partial class AddProductWindow : Window {
        private User currentUser { get; set; }
        public AddProductWindow(User currentUser) {
            InitializeComponent();
            this.currentUser = currentUser;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private async void btnAddProduct_Click(object sender, RoutedEventArgs e) {

            //Validate
            if (tbDistrCompany.Text == "" || tbProductName.Text == "" || tbQuantity.Text == "" || tbBuyPrice.Text == "") {
                MessageBox.Show("Completeaza toate campurile !");
                return;
            }

            List<ProductEntry> products = new List<ProductEntry>();
            products.Add(new ProductEntry {
                Product = new Product() {
                    IsActive = true,
                    DateAdded = DateTime.Now,
                    DistributionCompany = tbDistrCompany.Text,
                    Name = tbProductName.Text,
                    CurrentQuantity = Convert.ToDouble(tbQuantity.Text),
                    InitialQuantity = Convert.ToDouble(tbQuantity.Text),
                    CurrentValue = Convert.ToDouble(tbQuantity.Text) * Convert.ToDouble(tbBuyPrice.Text),
                    BuyPrice = Convert.ToDouble(tbBuyPrice.Text),
                    Type = cbProductType.Text
                },
                DateAdded = DateTime.Now,
                Quantity = Convert.ToDouble(tbQuantity.Text),
                Price = -Convert.ToDouble(tbBuyPrice.Text),
                Total = Convert.ToDouble(tbQuantity.Text) * -Convert.ToDouble(tbBuyPrice.Text),
                Type = cbProductType.Text,
            });
            await TunnelsClient.CreateOrderWithProductAsync(new Order() {
                UserId = currentUser.Id,
                DateAdded = DateTime.Now,
                OperationType = OperationTypeEnum.IN,
                Price = -Convert.ToDouble(tbBuyPrice.Text),
                Quantity = Convert.ToDouble(tbQuantity.Text),
                Total = Convert.ToDouble(tbQuantity.Text) * -Convert.ToDouble(tbBuyPrice.Text),
                ProductsEntries = products,
                IsActive = true
            });
            Close();
        }

        private void PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
            TextBox textBox = sender as TextBox;
            char ch = e.Text[0];
            if ((Char.IsDigit(ch) || ch == '.')) {
                if (ch == '.' && textBox.Text.Contains('.'))
                    e.Handled = true;
            }
            else
                e.Handled = true;
        }
    }
}
