using AnnotationTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnnotationTool.View
{
    /// <summary>
    /// Interaction logic for DataGrid.xaml
    /// </summary>
    public partial class DataGrid : UserControl
    {
        public DataGrid()
        {
            InitializeComponent();
            DataContext = this;
        }


        public List<_2DLine> _2DLineList
        {
            get { return (List<_2DLine>)GetValue(_2DLineListProperty); }
            set { SetValue(_2DLineListProperty, value); }
        }
        public _2DLine Selected2dLine
        {
            get { return (_2DLine)GetValue(Selected2dLineProperty); }
            set { SetValue(Selected2dLineProperty, value); }
        }


        public static readonly DependencyProperty Selected2dLineProperty =
            DependencyProperty.Register("Selected2dLine", typeof(_2DLine), typeof(DataGrid),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty _2DLineListProperty =
            DependencyProperty.Register("_2DLineList", typeof(List<_2DLine>), typeof(DataGrid));

    }
}
