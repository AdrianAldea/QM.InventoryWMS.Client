﻿using System.Globalization;
using System.Windows;
using WPFLocalizeExtension.Engine;

namespace QM.InventoryWMS {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.CurrentCulture;
        }
    }
}
