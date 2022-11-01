
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using SegundaBiblioteca;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            VerificarSistemas();
        }

        public void VerificarSistemas()
        {
            ICollection<Element> tubulacoes =
                 new FilteredElementCollector(Doc.Document, Doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_PipeCurves).ToElements();




            IList<string> NomesAdicionados = new List<string>();

            foreach (Element i in tubulacoes)
            {
                Parameter p = i.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
                if (p != null && p.AsValueString() != null)
                {

                    {
                        if (!NomesAdicionados.Contains(p.AsValueString()))
                        {
                            ComboLista.Items.Add(p.AsValueString());
                            NomesAdicionados.Add(p.AsValueString());
                        }

                    }
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ICollection<ElementId> tubulacoes =
               new FilteredElementCollector(Doc.Document, Doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_PipeCurves).ToElementIds();
            if() // Verificar o sistema selecionado e selecionar apenas as tubulações correspondentes
            Doc.Selection.SetElementIds(tubulacoes);
        }

        //private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (ComboLista.SelectedItem != null)

        //    try
        //    {
        //        IList<string> ListaTipos = new List<string>();

        //        foreach (Element cSistemas in TiposSistemas)
        //        {
        //            ListaTipos.Add(cSistemas.Name);
        //        }
        //        ComboLista.DataContext = ListaTipos;
        //    }
        //    catch (Exception ex)
        //    {
        //        TaskDialog.Show("Error", ex.ToString());
        //    }

        //}
    }
}