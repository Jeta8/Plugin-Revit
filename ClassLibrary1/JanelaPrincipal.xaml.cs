
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using ComandosRevit;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Janelas
{
    /// <summary>
    /// Interação lógica para JanelaPrincipal.xaml
    /// </summary>
    public partial class JanelaPrincipal : Window
    {
        UIDocument Doc;


       
        public static string NomeTagAcessorioSelecionado = "";
        public static string NomeTagPecaSelecionada = "";
        public static string DirecaoTagSelecionada = "";

     
        public static string TipoTagAcessorioSelecionado = "";
        public static string TipoTagPecaSelecionado = "";
        public static string FamiliaLuvaSelecionada = "";
        public static string SistemaAlvo = "";

        public static double TamanhoLinhaTag = 1.5;

        public JanelaPrincipal()
        {
        }

        public JanelaPrincipal(UIDocument doc)
        {
            InitializeComponent();
            Doc = doc;

            VerificarSistemas();
        }

        public void VerificarSistemas()
        {
            

            //Conexões

            ICollection<Element> tagsconexoes =
                 new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeFittingTags).ToElements();

            // Acessorios

            ICollection<Element> tagsacessorios =
                 new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeAccessoryTags).ToElements();

            // Peças Hidro Sanitárias

            ICollection<Element> tagspecas =
                 new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PlumbingFixtureTags).ToElements();

            // Luvas

            ICollection<Element> luvasT =
                new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeFitting).ToElements();

          
            IList<string> TagsConexoesAdicionados = new List<string>();
            IList<string> TagsAcessoriosAdicionados = new List<string>();
            IList<string> TagsPecasAdicionados = new List<string>();
            IList<string> LuvasAdicionadas = new List<string>();

       
            foreach (Element j in tagsconexoes)
            {
                // Aqui verifica os tipos de tags que o usuário tem em todo o projeto dele
                Parameter k = j.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                if (k != null && k.AsString() != null)
                {
                    if (!TagsConexoesAdicionados.Contains(k.AsString()))
                    {
                        if (ComboListaTagsConexoes.Items.Contains(k.AsString()))
                        {
                            continue;
                        }

                        else
                        {
                            ComboListaTagsConexoes.Items.Add(k.AsString());
                            TagsConexoesAdicionados.Add(k.AsString());
                        }
                    }
                }
            }


            foreach (Element v in tagsacessorios)
            {
                // Aqui verifica os tipos de tags que o usuário tem em todo o projeto dele
                Parameter d = v.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                if (d != null && d.AsString() != null)
                {
                    if (!TagsAcessoriosAdicionados.Contains(d.AsString()))
                    {
                        ComboListaTagsAcessorios.Items.Add(d.AsString());
                        TagsAcessoriosAdicionados.Add(d.AsString());
                    }
                }
            }

            foreach (Element n in tagspecas)
            {
                // Aqui verifica os tipos de tags que o usuário tem em todo o projeto dele
                Parameter d = n.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                if (d != null && d.AsString() != null)
                {
                    if (!TagsPecasAdicionados.Contains(d.AsString()))
                    {
                        ComboListaTagsPecas.Items.Add(d.AsString());
                        TagsPecasAdicionados.Add(d.AsString());
                    }
                }
            }

            foreach (Element cn in luvasT)
            {
                try
                {
                    if (cn is FamilySymbol)
                    {
                        dynamic simboloFamilia = cn;
                        Family Familia = simboloFamilia.Family;

                        if (Familia != null)
                        {
                            Parameter tipoParte = Familia.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE);

                            if (tipoParte != null && tipoParte.AsValueString() != null)
                            {
                                if ((tipoParte.AsValueString().Contains("União") || tipoParte.AsValueString().Contains("Union")) && !LuvasAdicionadas.Contains(tipoParte.AsValueString()))
                                {
                                    ComboListaLuvasMaterial.Items.Add(simboloFamilia.FamilyName);
                                    LuvasAdicionadas.Add(simboloFamilia.FamilyName);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
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


        public static double ValorUsuarioLuva = 0;

     

        // Limpar Tags
        private void LimparTags_Click(object sender, RoutedEventArgs e)
        {
            ComandoLimpeza.GetInstance.LimpezaTags.Raise();
        }

        // Tags em Conexões
        private void AdicionarTagsConexoes_Click(object sender, RoutedEventArgs e)
        {
            if (ComboListaSistema.SelectedIndex == -1)
            {
                TaskDialog.Show("Erro", "Selecione o sistema desejado!", TaskDialogCommonButtons.Ok);
                return;
            }
            TagsConexoes.GetInstance.TagsConex.Raise();
        }

        private void ComboListaTagsConexoes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Aqui adiciona ao combobox os tipos de tags no projeto do usuário
            if (ComboListaTagsConexoes.SelectedIndex == -1)
                return;

            NomeTagConexaoSelecionada = ComboListaTagsConexoes.SelectedItem.ToString();


            ICollection<Element> tagsconexoes =
             new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeFittingTags).ToElements();


            // Conexões
            foreach (Element h in tagsconexoes)
            {
                try
                {
                    dynamic conexao = h;
                    dynamic isFamilyInstanceConex = conexao.Family;

                    if (isFamilyInstanceConex == null)
                    {
                        tagsconexoes.Remove(h);
                    }
                }
                catch (Exception)
                {
                }
            }
            ComboListaInstanciasConexoes.Items.Clear();
            foreach (Element h in tagsconexoes)
            { // Adiciona as instâncias ( Direção e tamanho da tag )
                try
                {
                    dynamic conexao = h;
                    dynamic isFamilyInstanceConex = conexao.Family;

                    if (isFamilyInstanceConex != null)
                    {
                        FamilySymbol instanciaconexao = h as FamilySymbol;
                        string nomeFamilia = instanciaconexao.FamilyName;

                        if (instanciaconexao != null && NomeTagConexaoSelecionada.Equals(nomeFamilia))
                        {
                            if (ComboListaInstanciasConexoes.Items.Contains(instanciaconexao.Name))
                                continue;

                            if (!ComboListaInstanciasConexoes.Items.Contains(instanciaconexao.Name))
                            {
                                ComboListaInstanciasConexoes.Items.Add(instanciaconexao.Name);
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


        private void ComboListaInstanciasConexoes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboListaInstanciasConexoes.SelectedIndex == -1)
                return;

            TipoTagConexaoSelecionada = ComboListaInstanciasConexoes.SelectedItem.ToString();

        }

        // Acessórios
        private void AdicionarTagsAcessorios_Click(object sender, RoutedEventArgs e)
        {
            if (ComboListaSistema.SelectedIndex == -1)
            {
                TaskDialog.Show("Erro", "Selecione o sistema desejado!", TaskDialogCommonButtons.Ok);
                return;
            }
            TagsAcessorios.GetInstance.TagsAcess.Raise();
        }

        private void ComboListaAcessorios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            NomeTagAcessorioSelecionado = ComboListaTagsAcessorios.SelectedItem.ToString();

            ICollection<Element> tagsacessorios =
           new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeAccessoryTags).ToElements();


            foreach (Element m in tagsacessorios)
            {
                try
                {
                    dynamic acessorio = m;
                    dynamic isFamilyInstanceAcess = acessorio.Family;

                    if (isFamilyInstanceAcess == null)
                    {
                        tagsacessorios.Remove(m);
                    }
                }
                catch (Exception)
                {
                }
            }

            ComboListaInstanciasAcessorios.Items.Clear();
            foreach (Element m in tagsacessorios)
            { // Adiciona as instâncias ( Direção e tamanho da tag )
                try
                {
                    dynamic acessorio = m;
                    dynamic isFamilyInstanceAcess = acessorio.Family;

                    if (isFamilyInstanceAcess != null)
                    {
                        FamilySymbol instanciaacessorio = m as FamilySymbol;
                        string nomeFamilia = instanciaacessorio.FamilyName;

                        if (instanciaacessorio != null && NomeTagAcessorioSelecionado.Equals(nomeFamilia))
                        {
                            if (ComboListaInstanciasAcessorios.Items.Contains(instanciaacessorio.Name))
                                continue;
                            if (!ComboListaInstanciasAcessorios.Items.Contains(instanciaacessorio.Name))
                            {
                                ComboListaInstanciasAcessorios.Items.Add(instanciaacessorio.Name);
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

        private void ComboListaInstanciasAcessorios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboListaInstanciasAcessorios.SelectedIndex == -1)
                return;

            TipoTagAcessorioSelecionado = ComboListaInstanciasAcessorios.SelectedItem.ToString();
        }

        // Peças
        private void ComboListaTagsPecas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            NomeTagPecaSelecionada = ComboListaTagsPecas.SelectedItem.ToString();

            ICollection<Element> tagspecas =
              new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PlumbingFixtureTags).ToElements();

            foreach (Element m in tagspecas)
            {
                try
                {
                    dynamic peca = m;
                    dynamic isFamilyInstanceAcess = peca.Family;

                    if (isFamilyInstanceAcess == null)
                    {
                        tagspecas.Remove(m);
                    }
                }
                catch (Exception)
                {
                }
            }

            ComboListaInstanciasPecas.Items.Clear();
            foreach (Element m in tagspecas)
            { // Adiciona as instâncias ( Direção e tamanho da tag )
                try
                {
                    dynamic peca = m;
                    dynamic isFamilyInstancePecas = peca.Family;

                    if (isFamilyInstancePecas != null)
                    {
                        FamilySymbol instanciapeca = m as FamilySymbol;
                        string nomeFamilia = instanciapeca.FamilyName;

                        if (instanciapeca != null && NomeTagPecaSelecionada.Equals(nomeFamilia))
                        {
                            if (ComboListaInstanciasPecas.Items.Contains(instanciapeca.Name))
                                continue;
                            if (!ComboListaInstanciasPecas.Items.Contains(instanciapeca.Name))
                            {
                                ComboListaInstanciasPecas.Items.Add(instanciapeca.Name);
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

        private void ComboListaInstanciasPecas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboListaInstanciasPecas.SelectedIndex == -1)
                return;

            TipoTagPecaSelecionado = ComboListaInstanciasPecas.SelectedItem.ToString();
        }

        private void AdicionarTagsPecas_Click(object sender, RoutedEventArgs e)
        {
            if (ComboListaSistema.SelectedIndex == -1)
            {
                TaskDialog.Show("Erro", "Selecione o sistema desejado!", TaskDialogCommonButtons.Ok);
                return;
            }

            TagsPecasHidro.GetInstance.TagsPecas.Raise();
        }

        public enum Direcoes
        {
            Cima = 0,
            Direita = 1,
            Baixo = 2,
            Esquerda = 3
        }
        public static Direcoes direcoesNomes = Direcoes.Esquerda;
        public void ComboListaDirecaoTag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboListaDirecaoTag.SelectedIndex == -1)
            {
                return;
            }
            direcoesNomes = (Direcoes)ComboListaDirecaoTag.SelectedIndex;
        }

        private void ComboListaLuvasMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FamiliaLuvaSelecionada = ComboListaLuvasMaterial.SelectedItem.ToString();
        }

        private void InputComprimentoLuva_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Aqui o usuário pode escolher o comprimento específico de tubulações que ele deseja selecionar e/ou adicionar as tags
            if (InputComprimentoLuva.Text != "")
            {
                try
                {
                    ValorUsuarioLuva = Convert.ToDouble(InputComprimentoLuva.Text);
                }
                catch (Exception)
                {
                    ValorUsuarioLuva = 0;
                    InputComprimentoLuva.Text = "";
                }
            }
            else
            {
                ValorUsuarioLuva = 6;
            }
        }

        private void InputComprimentoLuva_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Filtro de caracteres, para só aceitar números
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void AdicionarLuvas_Click(object sender, RoutedEventArgs e)
        {
            if (ComboListaSistema.SelectedIndex == -1)
            {
                TaskDialog.Show("Erro", "Selecione o sistema desejado!", TaskDialogCommonButtons.Ok);
                return;
            }
            AdicLuvas.GetInstance.AdicionarLuvas.Raise();
        }


        public void ComboListaSistema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboSistema != -1)
            {
                SistemaAlvo = ComboListaSistema.SelectedItem.ToString();
            }
           
            comboSistema = ComboListaSistema.SelectedIndex;
        }
        public static int comboSistema = -1;
    }
}