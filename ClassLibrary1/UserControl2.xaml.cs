
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using SegundaBiblioteca;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Line = Autodesk.Revit.DB.Line;

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

            ICollection<Element> identificadores =
                new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeTags).ToElements();

            IList<string> NomesAdicionados = new List<string>();

            IList<string> TagsAdicionados = new List<string>();

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
            foreach (Element g in identificadores)
            {
                Parameter t = g.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM);

                if (t != null && t.AsValueString() != null)
                {
                    if (!TagsAdicionados.Contains(t.AsValueString()))
                    {
                        ComboListaTags.Items.Add(t.AsValueString());
                        TagsAdicionados.Add(t.AsValueString());
                    }
                }

            }
        }


        private void SelecaoT_Click(object sender, RoutedEventArgs e)
        {
            ICollection<Element> tubulacoes =
               new FilteredElementCollector(Doc.Document, Doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_PipeCurves).ToElements();
            // Verificar o sistema selecionado e selecionar apenas as tubulações correspondentes
            IList<ElementId> SistemaSelecionado = new List<ElementId>();

            foreach (Element t in tubulacoes)
            {
                Parameter Sistemas = t.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
                if (Sistemas != null && Sistemas.AsValueString() != null)
                {
                    Parameter Comprimento = t.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);

                    if (Sistemas.AsValueString().Equals(ComboLista.SelectedItem.ToString()))
                    {
                        if (Comprimento != null)
                        {
                            double ValorComprimento = UnitUtils.Convert(Comprimento.AsDouble(), DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_METERS);

                            if (ValorComprimento >= ValorUsuario)
                            {
                                SistemaSelecionado.Add(t.Id);
                            }
                        }
                    }
                }
            }
            Doc.Selection.SetElementIds(SistemaSelecionado);
        }

        double ValorUsuario = 0;
        private void InputComprimento_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (InputComprimento.Text != "")
            {
                try
                {
                    ValorUsuario = Convert.ToDouble(InputComprimento.Text);
                }
                catch (Exception)
                {
                    ValorUsuario = 0;
                    InputComprimento.Text = "";
                }
            }
        }

        private void InputComprimento_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void AdicionarTags_Click(object sender, RoutedEventArgs e)
        {

           ICollection<Element> tubulacoes =
               new FilteredElementCollector(Doc.Document, Doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_PipeCurves).ToElements();
           
            var localzi = tubulacoes.First().Location as LocationCurve;
            FamilySymbol jarro = null;
            Line linha = localzi.Curve as Line;

            if (localzi.Curve != null)
            {

            }

           // FamilyInstance refer = Doc.Document.FamilyCreate.NewFamilyInstance(linha, jarro, Doc.Document.ActiveView);

            //Reference reference;
            ICollection <Element> Tags =
                 new FilteredElementCollector(Doc.Document, Doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_PipeTags).ToElements();
            IList<Element> ListaTag = new List<Element>();

           // IndependentTag tag = IndependentTag.Create(
           //Doc, Doc.Document, new Reference(reference),
           // false, TagMode.TM_ADDBY_CATEGORY,
           //TagOrientation.Horizontal);
        }
       // LEADER_LINE


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