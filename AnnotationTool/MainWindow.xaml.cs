using System;
using System.Windows;
using System.Deployment.Application;
using System.Reflection;

namespace AnnotationTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Version RunningVersion { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            RunningVersion = GetRunningVersion();
        }

        private Version GetRunningVersion()
        {
            try
            {
                return ApplicationDeployment.CurrentDeployment.CurrentVersion;
            }
            catch (Exception)
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }
    }
}
