
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
    }
}



//public Result insereFamilia()
//{
//    iGlobals = Globals.GetInstance;

//```
//        Parameter reservatorio = null;

//    using (var trans = new Transaction(ActiveDoc.Doc, "Inserindo o Reservatório"))
//    {
//        trans.Start();

//        double rPosicionamento = 0;

//        // Posicionamento dos reservatórios
//        if (iGlobals.iReservatorios.reservatorioSuperior)
//        {
//            // Valor do reservatório adotado pelo usuário
//            if (iGlobals.vreservaSuperior > 10000) // Reservatóro máximo
//            {
//                UnMEP.MainWindow.window.addLog("O volume do reservatório superior excedeu 10.000L! será considerado o maior reservatório disponível.", 2);
//            }

//            rPosicionamento = iGlobals.iAguaFria.selecionaReservatorio(iGlobals.vreservaSuperior);
//        }
//        else
//        {
//            if (iGlobals.vreservaInferior > 10000) // Reservatóro máximo
//            {
//                UnMEP.MainWindow.window.addLog("O volume do reservatório inferior excedeu 10.000L! será considerado o maior reservatório disponível.", 2);
//            }

//            // Pega o reservatório ideal
//            rPosicionamento = iGlobals.iAguaFria.selecionaReservatorio(iGlobals.vreservaInferior);
//        }

//        var format = String.Format("{0:N0}", rPosicionamento);

//        List<BuiltInCategory> listaFiltro = new List<BuiltInCategory>();
//        listaFiltro.Add(BuiltInCategory.OST_PlumbingFixtures);

//        ElementMulticategoryFilter filtroCompatibilitar = new ElementMulticategoryFilter(listaFiltro);
//        FilteredElementCollector docCollector = new FilteredElementCollector(ActiveDoc.Doc).WherePasses(filtroCompatibilitar);

//        // Percorre todas as famílias da categoria carregadas no projeto
//        foreach (var pecasHidrossanatiras in docCollector.ToElements())
//        {
//            Parameter p = pecasHidrossanatiras.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM);

//            if (p != null)
//            {
//                // Nome da instância da família
//                if (rPosicionamento > 750)
//                {
//                    if (pecasHidrossanatiras.Name.Equals("UnMEP - " + (rPosicionamento > 5000 ? "C" : (rPosicionamento > 750 ? "B" : "A")) + " - " + format + " L"))
//                    {
//                        // Encontrou a instância da família, salva o parâmetro na variável
//                        reservatorio = p;
//                        break;
//                    }
//                }
//                else
//                {
//                    if (pecasHidrossanatiras.Name.Equals("UnMEP - " + (rPosicionamento > 5000 ? "C" : (rPosicionamento > 750 ? "B" : "A")) + " - " + rPosicionamento + " L"))
//                    {
//                        reservatorio = p;
//                        break;
//                    }
//                }
//            }
//        }

//        trans.Commit();
//    }

//    if (reservatorio != null)
//    {
//        // Acessa a família responsável pela instância
//        ElementId familyId = reservatorio.Element.Id;
//        dynamic family = ActiveDoc.Doc.GetElement(familyId);

//        if (family != null)
//        {
//            // Acessa o símbolo da família aqui
//            dynamic fmanager = family.Family;
//        }
//    }
//}
//```‌