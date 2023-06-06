using ExcelHelper;
using Microsoft.Win32;
using QM.Inventory.TunnelsClient;
using QM.InventoryWMS.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Tunnels.Core.Models;
using Tunnels.Core.Views;

namespace QM.InventoryWMS {
    public partial class MainWindow : Window {
        public User? User;
        private OrdersWithProductsFilterRequest Filter { get; set; } = new OrdersWithProductsFilterRequest();
        private List<OrdersWithProductsView> OrdersWithProducts { get; set; } = new List<OrdersWithProductsView>();
        public Product SelectedProduct { get; set; }
        public List<Product> Products { get; set; }
        public int SelectedProductId { get; set; }

        public MainWindow() {
            DataContext = this;
            InitializeComponent();
            cbOperationType.ItemsSource = Enum.GetValues(typeof(OperationTypeEnum)).Cast<OperationTypeEnum>();
            if (cbIsActive.IsChecked != null) {
                Filter.IsActive = !cbIsActive.IsChecked;
            }
            else {
                Filter.IsActive = cbIsActive.IsChecked;
            }

            if (cbOrderIsActive.IsChecked != null) {
                Filter.IsOrderActive = !cbOrderIsActive.IsChecked;
            }
            else {
                Filter.IsOrderActive = cbOrderIsActive.IsChecked;
            }
        }
        private async void LoadOrdersDataGrid() {
            OrdersWithProducts = await TunnelsClient.GetAllOrdersWithProductsByFilterAsync(Filter);
            dgOrders.ItemsSource = OrdersWithProducts;
            CalculateTotal();
        }
        private void ReloadUI() {
            if (User?.Role == RolesEnum.User || User?.Role == RolesEnum.Administrator) {
                btnAddProduct.Visibility = Visibility.Visible;
                btnManageUsers.Visibility = Visibility.Visible;
                btnSaveAsExcel.Visibility = Visibility.Visible;
                btnDeleteOrder.Visibility = Visibility.Visible;
                dgOrders.Visibility = Visibility.Visible;
                btnFilter.Visibility = Visibility.Visible;
                lblStartDate.Visibility = Visibility.Visible;
                lblEndDate.Visibility = Visibility.Visible;
                lblProduct.Visibility = Visibility.Visible;
                lblOrder.Visibility = Visibility.Visible;
                rbDate.Visibility = Visibility.Visible;
                rbNoFilter.Visibility = Visibility.Visible;
                rbProduct.Visibility = Visibility.Visible;
                rbOrder.Visibility = Visibility.Visible;
                cbIsActive.Visibility = Visibility.Visible;
                dpStartDate.Visibility = Visibility.Visible;
                dpEndDate.Visibility = Visibility.Visible;
                cbProduct.Visibility = Visibility.Visible;
                tbOrderId.Visibility = Visibility.Visible;
                rbDateAndProduct.Visibility = Visibility.Visible;
                lblOperationType.Visibility = Visibility.Visible;
                cbOperationType.Visibility = Visibility.Visible;
                cbOrderIsActive.Visibility = Visibility.Visible;
            }
            else {
                btnAddProduct.Visibility = Visibility.Hidden;
                btnManageUsers.Visibility = Visibility.Hidden;
                btnSaveAsExcel.Visibility = Visibility.Hidden;
                btnDeleteOrder.Visibility = Visibility.Hidden;
                dgOrders.Visibility = Visibility.Hidden;
                btnFilter.Visibility = Visibility.Hidden;
                lblStartDate.Visibility = Visibility.Hidden;
                lblEndDate.Visibility = Visibility.Hidden;
                lblProduct.Visibility = Visibility.Hidden;
                lblOrder.Visibility = Visibility.Hidden;
                rbDate.Visibility = Visibility.Hidden;
                rbNoFilter.Visibility = Visibility.Hidden;
                rbProduct.Visibility = Visibility.Hidden;
                rbOrder.Visibility = Visibility.Hidden;
                cbIsActive.Visibility = Visibility.Hidden;
                dpStartDate.Visibility = Visibility.Hidden;
                dpEndDate.Visibility = Visibility.Hidden;
                cbProduct.Visibility = Visibility.Hidden;
                tbOrderId.Visibility = Visibility.Hidden;
                rbDateAndProduct.Visibility = Visibility.Hidden;
                lblOperationType.Visibility = Visibility.Hidden;
                cbOperationType.Visibility = Visibility.Hidden;
                cbOrderIsActive.Visibility = Visibility.Hidden;
            }
        }

        #region Events
        private async void btnAddProduct_Click(object sender, RoutedEventArgs e) {
            ProductsWindow products = new(User);
            products.ShowDialog();
            cbProduct.ItemsSource = await TunnelsClient.GetAllProductsAsync(null);
            //LoadOrdersDataGrid();
        }


        private void btnLogin_Click(object sender, RoutedEventArgs e) {
            this.Hide();

            LogInWindow logInWindow = new LogInWindow(User);
            logInWindow.ShowDialog();
            if (logInWindow.User != null) {
                User = logInWindow.User;
                lblConnectedUser.Content = "Welcome: " + logInWindow.User.Name;
                this.Show();
            }
            else {
                lblConnectedUser.Content = "Please LogIn !";
                this.Show();
            }
            ReloadUI();
        }
        private void btnLogout_Click(object sender, RoutedEventArgs e) {
            User = null;
            lblConnectedUser.Content = "Please LogIn !";
            ReloadUI();
        }

        #endregion

        private void btnManageUsers_Click(object sender, RoutedEventArgs e) {
            AddUserWindow addUserWindow = new AddUserWindow();
            addUserWindow.ShowDialog();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            ReloadUI();
            cbProduct.ItemsSource = await TunnelsClient.GetAllProductsAsync((bool?)cbIsActive.IsChecked);
            //LoadOrdersDataGrid();
        }

        private async void btnFilter_Click(object sender, RoutedEventArgs e) {
            // Validation TODO
            if (Filter.FilterType == null) {
                MessageBox.Show("Select filter option !");
                return;
            }
            switch (Filter.FilterType.Value) {
                case FilterTypeEnum.ByOrderId:
                    if (Filter.OrderId == null) {
                        MessageBox.Show("Enter the ByOrderId !");
                        return;
                    }
                    break;
                case FilterTypeEnum.ByProductId:
                    if (Filter.ProductId == null) {
                        MessageBox.Show("Select the Product !");
                        return;
                    }
                    break;
                case FilterTypeEnum.ByDate:
                    if (Filter.StartDate > Filter.EndDate) {
                        MessageBox.Show("StartDate must be greater than EndDate or equal !");
                        return;
                    }
                    if (Filter.StartDate == null || Filter.EndDate == null) {
                        MessageBox.Show("Select StartDate and EndDate !");
                        return;
                    }
                    break;
                case FilterTypeEnum.ByDateAndProductId:
                    if (Filter.ProductId == null) {
                        MessageBox.Show("Select the Product !");
                        return;
                    }
                    if (Filter.StartDate > Filter.EndDate) {
                        MessageBox.Show("StartDate must be greater than EndDate or equal !");
                        return;
                    }
                    if (Filter.StartDate == null || Filter.EndDate == null) {
                        MessageBox.Show("Select StartDate and EndDate !");
                        return;
                    }
                    break;
            }
            LoadOrdersDataGrid();
            
        }

        private void CalculateTotal() {
            if (OrdersWithProducts.Any()) {
                if (OrdersWithProducts.Where(x => x.OperationType == OperationTypeEnum.IN).Any())
                    lblTotalIn.Content = OrdersWithProducts.Where(x => x.OperationType == OperationTypeEnum.IN).Sum(x => x.TotalProduct);
                if (OrdersWithProducts.Where(x => x.OperationType == OperationTypeEnum.OUT).Any())
                    lblTotalOut.Content = OrdersWithProducts.Where(x => x.OperationType == OperationTypeEnum.OUT).Sum(x => x.TotalProduct);
            }
            else {
                lblTotalIn.Content = 0.ToString();
                lblTotalOut.Content = 0.ToString();
            }
        }

        private void rb_Checked(object sender, RoutedEventArgs e) {
            RadioButton? rb = sender as RadioButton;
            switch (rb.Tag.ToString()) {
                case "NoFilter":
                    Filter.FilterType = FilterTypeEnum.NoFilter;
                    break;
                case "ByDate":
                    Filter.FilterType = FilterTypeEnum.ByDate;
                    break;
                case "ByProductId":
                    Filter.FilterType = FilterTypeEnum.ByProductId;
                    break;
                case "ByOrderId":
                    Filter.FilterType = FilterTypeEnum.ByOrderId;
                    break;
                case "ByDateAndProductId":
                    Filter.FilterType = FilterTypeEnum.ByDateAndProductId;
                    break;
                default:
                    Filter.FilterType = FilterTypeEnum.NoFilter;
                    break;
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
            rbDate.IsChecked = true;
            Filter.StartDate = dpStartDate.SelectedDate;
            Filter.EndDate = dpEndDate.SelectedDate;
        }

        private void cbProduct_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (cbProduct.Items.Count != 0) {
                rbProduct.IsChecked = true;
                if (((Product)cbProduct.SelectedValue) != null)
                    Filter.ProductId = ((Product)cbProduct.SelectedValue).Id;
            }
        }

        private void tbOrderId_TextChanged(object sender, TextChangedEventArgs e) {
            rbOrder.IsChecked = true;
            if (tbOrderId.Text != "")
                Filter.OrderId = Convert.ToDouble(tbOrderId.Text);
        }

        private void btnSaveAsExcel_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "Report " + DateTime.Now.ToString("yyyy-dd-MM HH-mm-ss");
            saveFileDialog.DefaultExt = ".xlsx";
            saveFileDialog.Filter = "Excel (*.xlsx)|*.xlsx";
            if (saveFileDialog.ShowDialog() == true) {
                CreateExcelFile.CreateExcelDocument(OrdersWithProducts, saveFileDialog.FileName, true);
                Process.Start(new ProcessStartInfo { FileName = saveFileDialog.FileName, UseShellExecute = true });
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            Application.Current.Shutdown();
        }

        private void cbIsActive_Click(object sender, RoutedEventArgs e) {
            if (cbIsActive.IsChecked != null) {
                Filter.IsActive = !cbIsActive.IsChecked;
            }
            else {
                Filter.IsActive = cbIsActive.IsChecked;
            }
        }

        private void tbOrderId_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private async void cbProduct_Loaded(object sender, RoutedEventArgs e) {
            cbProduct.ItemsSource = await TunnelsClient.GetAllProductsAsync(null);
        }

        private void cbOperationType_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Filter.OperationType = (OperationTypeEnum)(sender as ComboBox).SelectedItem;
        }

        private void cbOrderIsActive_Click(object sender, RoutedEventArgs e) {
            if (cbOrderIsActive.IsChecked != null) {
                Filter.IsOrderActive = !cbOrderIsActive.IsChecked;
            }
            else {
                Filter.IsOrderActive = cbOrderIsActive.IsChecked;
            }
        }

        private async void btnDeleteOrder_Click(object sender, RoutedEventArgs e) {
            if (tbOrderId.Text != "") {
                var id = Convert.ToInt32(tbOrderId.Text);
                if (MessageBox.Show($"Esti sigur ca vrei sa invalidezi bonul: {id} ?", "Invalidare bon", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) {
                    return;
                }
                else {
                    await TunnelsClient.InactivateOrder(Convert.ToInt32(tbOrderId.Text));
                }
            }
            else {
                MessageBox.Show("Introdu numar bon");
            }
        }
    }
}