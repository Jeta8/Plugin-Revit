
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SegundaBiblioteca;
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


namespace ClassLibrary1
{
    /// <summary>
    /// Interação lógica para UserControl2.xaml
    /// </summary>
    public partial class UserControl2 : Window
    {
        UIDocument Doc;

        public UserControl2()
        {
        }

        public UserControl2(UIDocument doc)
        {
            InitializeComponent();
            Doc = doc;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ICollection<ElementId> tubulacoes =
                new FilteredElementCollector(Doc.Document, Doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_PipeCurves).ToElementIds();
            TaskDialog.Show("Quantidade de tubulações: ", tubulacoes.Count.ToString());

            ICollection<ElementId> sistemas =
                new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_SwitchSystem).ToElementIds();
            TaskDialog.Show("Sistema", sistemas.Count.ToString());
            Doc.Selection.SetElementIds(tubulacoes);
            
        }
    }
}
