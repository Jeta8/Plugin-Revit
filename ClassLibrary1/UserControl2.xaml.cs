
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
using System.Xml.Linq;
using Line = Autodesk.Revit.DB.Line;

namespace ClassLibrary1
{
    /// <summary>
    /// Interação lógica para UserControl2.xaml
    /// </summary>
    public partial class UserControl2 : Window
    {
        UIDocument Doc;
        public static string NomeTagSelecionada = "";
        public static string TipoTagSelecionada = "";
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
                            ComboListaSistema.Items.Add(p.AsValueString());
                            NomesAdicionados.Add(p.AsValueString());
                        }

                    }
                }
            }
            
            foreach (Element g in identificadores)
            {
               
                Parameter t = g.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                if (t != null && t.AsString() != null)
                {
                    if (!TagsAdicionados.Contains(t.AsString()))
                    {
                        ComboListaTags.Items.Add(t.AsString());
                        TagsAdicionados.Add(t.AsString());
                    }           
                }
              
            }
        }


            public void Selecionar_Sistema_Click(object sender, RoutedEventArgs e)
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

                        if (Sistemas.AsValueString().Equals(ComboListaSistema.SelectedItem.ToString()))
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

    public static double ValorUsuario = 0;
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
            else
            {
                ValorUsuario = 0;
            }
        }

        private void InputComprimento_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void AdicionarTags_Click(object sender, RoutedEventArgs e)
        {
            ComandoTags.GetInstance.cTags.Raise();
        }

        private void ComboListaTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboListaTags.SelectedIndex == -1)
                return;

            NomeTagSelecionada = ComboListaTags.SelectedItem.ToString();

            ICollection<Element> identificadores =
             new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeTags).ToElements();


            foreach (Element h in identificadores)
            {
                try
                {
                    dynamic elemento = h;
                    dynamic isFamilyInstance = elemento.Family;

                    if (isFamilyInstance == null)
                    {
                        identificadores.Remove(h);
                    }
                }
                catch (Exception)
                {
                }
            }
            ComboListaInstancias.Items.Clear();
            foreach (Element h in identificadores)
            {
                try
                {
                    dynamic elemento = h;
                    dynamic isFamilyInstance = elemento.Family;

                    if (isFamilyInstance != null)
                    {
                        FamilySymbol instancia = h as FamilySymbol;
                        string nomeFamilia = instancia.FamilyName;

                        if (instancia != null && NomeTagSelecionada.Equals(nomeFamilia))
                        {
                            if (!ComboListaInstancias.Items.Contains(instancia.Name))
                            {
                                ComboListaInstancias.Items.Add(instancia.Name);
                            }
                        }
                    }
                }

                catch (Exception)
                {
                    continue;
                }
            }
        }

        private void ComboListaInstancias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboListaInstancias.SelectedIndex == -1)
                return;

            TipoTagSelecionada = ComboListaInstancias.SelectedItem.ToString();
        }

        private void LimparTags_Click(object sender, RoutedEventArgs e)
        {
            ComandoLimpeza.GetInstance.LimpezaTags.Raise();                     
        }
    }
}