using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ComandosRevit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace JanelaTubulacao
{
    /// <summary>
    /// Interação lógica para JanelaTubulacao.xam
    /// </summary>
    public partial class JanelaTubulacao : UserControl
    {
        UIDocument Doc;

        public static string NomeTagTuboSelecionada = "";
        public static string TipoTagTuboSelecionada = "";
        public static double ValorUsuarioTubo = 0;

        public JanelaTubulacao(UIDocument doc)
        {
            InitializeComponent();

            VerificarSistema();
        }

        public void VerificarSistema() 
        {
            ICollection<Element> tubulacoes =
                new FilteredElementCollector(Doc.Document, Doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_PipeCurves).ToElements();

            ICollection<Element> identificadores =
                 new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeTags).ToElements();

            IList<string> NomesTagsTubosAdicionados = new List<string>();
            IList<string> TiposTagsTubosAdicionados = new List<string>();

            foreach (Element i in tubulacoes)
            {
                // Aqui verifica os sistemas / disciplinas que o usuário tem na vista atual
                Parameter p = i.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);

                if (p != null && p.AsValueString() != null)
                {
                    if (!NomesTagsTubosAdicionados.Contains(p.AsValueString()))
                    {
                        ComboListaSistema.Items.Add(p.AsValueString());
                        NomesTagsTubosAdicionados.Add(p.AsValueString());
                    }
                }
            }
        }

        public void Selecionar_Sistema_Click(object sender, RoutedEventArgs e)
        {
            if (ComboListaSistema.SelectedIndex == -1)
            {
                TaskDialog.Show("Erro", "Selecione o sistema desejado!", TaskDialogCommonButtons.Ok);
                return;
            }
            ICollection<Element> tubulacoes =
               new FilteredElementCollector(Doc.Document, Doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_PipeCurves).ToElements();
            IList<ElementId> SistemaSelecionado = new List<ElementId>();

            foreach (Element t in tubulacoes)
            {
                // Verificar o sistema selecionado e selecionar apenas as tubulações correspondentes
                Parameter Sistemas = t.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
                if (Sistemas != null && Sistemas.AsValueString() != null)
                {
                    Parameter Comprimento = t.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);

                    if (Sistemas.AsValueString().Equals(ComboListaSistema.SelectedItem.ToString()))
                    {
                        if (Comprimento != null)
                        { // Converte a unidade de comprimento de pés (padrão do Revit) para metros
                            double ValorComprimento = UnitUtils.Convert(Comprimento.AsDouble(), DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_METERS);

                            if (ValorComprimento >= ValorUsuarioTubo)
                            {
                                SistemaSelecionado.Add(t.Id);
                            }
                        }
                    }
                }
            }
            Doc.Selection.SetElementIds(SistemaSelecionado);
        }

        private void InputComprimento_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Aqui o usuário pode escolher o comprimento específico de tubulações que ele deseja selecionar e/ou adicionar as tags
            if (InputComprimento.Text != "")
            {
                try
                {
                    ValorUsuarioTubo = Convert.ToDouble(InputComprimento.Text);
                }
                catch (Exception)
                {
                    ValorUsuarioTubo = 0;
                    InputComprimento.Text = "";
                }
            }
            else
            {
                ValorUsuarioTubo = 0;
            }
        }

        private void InputComprimento_PreviewTextInput(object sender, TextCompositionEventArgs e)
        { // Filtro de caracteres, para só aceitar números
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void AdicionarTags_Click(object sender, RoutedEventArgs e)
        {
            if (ComboListaSistema.SelectedIndex == -1)
            {
                TaskDialog.Show("Erro", "Selecione o sistema desejado!", TaskDialogCommonButtons.Ok);
                return;
            }
            TagsTubulacao.GetInstance.cTags.Raise();
        }

        private void ComboListaTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Aqui adiciona ao combobox os tipos de tags no projeto do usuário
            if (ComboListaTags.SelectedIndex == -1)
                return;

            NomeTagTuboSelecionada = ComboListaTags.SelectedItem.ToString();

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
            { // Adiciona as instâncias ( Direção e tamanho da tag )
                try
                {
                    dynamic elemento = h;
                    dynamic isFamilyInstance = elemento.Family;

                    if (isFamilyInstance != null)
                    {
                        FamilySymbol instancia = h as FamilySymbol;
                        string nomeFamilia = instancia.FamilyName;

                        if (instancia != null && NomeTagTuboSelecionada.Equals(nomeFamilia))
                        {
                            if (ComboListaInstancias.Items.Contains(instancia.Name))
                                continue;

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

            TipoTagTuboSelecionada = ComboListaInstancias.SelectedItem.ToString();
        }
    }
}
