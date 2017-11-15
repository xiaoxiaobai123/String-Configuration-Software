using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Burner_AdvanTech
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            //  MainWindow.Device.acCanClose();

            Burner_AdvanTech.MainWindow.Device.acCanClose();
            Environment.Exit(0);
            System.Windows.Application.Current.Shutdown();
        }
    }
}
