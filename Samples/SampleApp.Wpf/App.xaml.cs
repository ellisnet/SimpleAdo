using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SampleApp.Services;

namespace SampleApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region ADDED TO SAMPLE TO DEMONSTRATE SimpleAdo.Sqlite

        public Services.IDatabasePath DatabasePath { get; set; } = new DatabasePath();
        public SimpleAdo.IObjectCryptEngine CryptEngine { get; set; }

        private readonly string _appDataKey = "Thi$T3stPa$$w0rd";

        public App()
        {
            CryptEngine = new SampleAesCryptEngine(_appDataKey);
        }

        #endregion
    }
}
