using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using KinectV2EmguCV.View;
using KinectV2EmguCV.ViewModel;

namespace KinectV2EmguCV
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Uncomment the following after testing to see that NBug is working as configured
            NBug.Settings.ReleaseMode = true;

            // NBug configuration (you can also choose to create xml configuration file)
            NBug.Settings.StoragePath = NBug.Enums.StoragePath.CurrentDirectory;
            NBug.Settings.UIMode = NBug.Enums.UIMode.Full;

            // Hook-up to all possible unhandled exception sources for WPF app, after NBug is configured
            AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
            Application.Current.DispatcherUnhandledException += NBug.Handler.DispatcherUnhandledException;
        }

        private void AppStart(object sender, StartupEventArgs e)
        {
            TopDownTrackerViewModel vm = new TopDownTrackerViewModel();
            TopDownTrackerView view = new TopDownTrackerView();
            view.DataContext = vm;
            view.Show();
        }
    }
}
