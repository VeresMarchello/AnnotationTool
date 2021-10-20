using System;
using System.Windows.Controls;
using System.Windows.Forms;

namespace AnnotationTool.View
{
    /// <summary>
    /// Interaction logic for View2D.xaml
    /// </summary>
    public partial class View2D : System.Windows.Controls.UserControl
    {
        public View2D()
        {
            InitializeComponent();
            Initialized += View2D_Initialized;
        }

        private void View2D_Initialized(object sender, EventArgs e)
        {
            var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.ExecutablePath);
            var path = config.AppSettings.Settings["ImagesPath"];

            if (path == null)
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    fbd.ShowNewFolderButton = false;
                    fbd.Description = "Válassza ki a képeket tartalmazó mappát!";
                    fbd.SelectedPath = $@"{AppDomain.CurrentDomain.BaseDirectory}Images\Left\Unpruned";
                    DialogResult result = fbd.ShowDialog();
                    if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        config.AppSettings.Settings.Add("ImagesPath", fbd.SelectedPath);
                        config.Save(System.Configuration.ConfigurationSaveMode.Modified);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
    }
}
