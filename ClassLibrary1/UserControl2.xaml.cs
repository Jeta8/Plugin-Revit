
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
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
        public static string NomeTagConexaoSelecionada = "";
        public static string NomeTagAcessorioSelecionado = "";
        public static string NomeTagPecaSelecionada = "";
        public static string DirecaoTagSelecionada = "";

        public static string MaterialLuvaSelecionado = "";


        public static string TipoTagSelecionada = "";
        public static string TipoTagConexaoSelecionada = "";
        public static string TipoTagAcessorioSelecionado = "";
        public static string TipoTagPecaSelecionado = "";

        public static string FamiliaLuvaSelecionada = "";


        public static double TamanhoLinhaTag = 1.5;

        public static string SistemaAlvo = ""; 



        public UserControl2()
        {
        }

        public UserControl2(UIDocument doc)
        {
            InitializeComponent();
            Doc = doc;
            VerificarSistemas();

            ProjectLocationSet pa = new ProjectLocationSet();

        }

        public void VerificarSistemas()
        {
            // Coleções que armazenam as tubulações e tags do projeto do usuário

            //Tubulações
            ICollection<Element> tubulacoes =
                 new FilteredElementCollector(Doc.Document, Doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_PipeCurves).ToElements();
            ICollection<Element> identificadores =
                 new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeTags).ToElements();

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



            IList<string> NomesAdicionados = new List<string>();

            IList<string> TagsAdicionados = new List<string>();


            IList<string> TagsConexoesAdicionados = new List<string>();


            IList<string> TagsAcessoriosAdicionados = new List<string>();

            IList<string> TagsPecasAdicionados = new List<string>();

            IList<string> LuvasAdicionadas = new List<string>();



            foreach (Element i in tubulacoes)
            {
                // Aqui verifica os sistemas / disciplinas que o usuário tem na vista atual
                Parameter p = i.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);

                if (p != null && p.AsValueString() != null)
                {
                    if (!NomesAdicionados.Contains(p.AsValueString()))
                    {
                        ComboListaSistema.Items.Add(p.AsValueString());
                        NomesAdicionados.Add(p.AsValueString());
                    }
                }
            }

            foreach (Element g in identificadores)
            {
                // Aqui verifica os tipos de tags que o usuário tem em todo o projeto dele
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

            foreach (Element j in tagsconexoes)
            {
                // Aqui verifica os tipos de tags que o usuário tem em todo o projeto dele
                Parameter k = j.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                if (k != null && k.AsString() != null)
                {
                    if (!TagsAdicionados.Contains(k.AsString()))
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

            foreach (Element t in luvasT)
            {
                
                //var getCon = TagsPecasHidro.GetConnectors(t);
                try
                {
                    if (t.GetType().ToString().Equals("Autodesk.Revit.DB.FamilyInstance"))
                    {
                        FamilyInstance p = t as FamilyInstance;
                        MechanicalFitting y = p.MEPModel as MechanicalFitting;

                        if (y.PartType == PartType.Union)
                        {
                            Parameter f = t.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM);
                            if (!LuvasAdicionadas.Contains(f.AsValueString()))
                            {
                                ComboListaLuvasMaterial.Items.Add(f.AsValueString());
                                LuvasAdicionadas.Add(f.AsValueString());
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                }  
            }
        }

        public void Selecionar_Sistema_Click(object sender, RoutedEventArgs e)
        {
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

        public static double ValorUsuarioTubo = 0;
        public static double ValorUsuarioLuva = 0;

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
                    ValorUsuarioTubo = 6;
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

        // Tags nas tubulações
        private void AdicionarTags_Click(object sender, RoutedEventArgs e)
        {
            ComandoTags.GetInstance.cTags.Raise();
        }

        private void ComboListaTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Aqui adiciona ao combobox os tipos de tags no projeto do usuário
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
            { // Adiciona as instâncias ( Direção e tamanho da tag )
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

            TipoTagSelecionada = ComboListaInstancias.SelectedItem.ToString();
        }

        // Limpar Tags
        private void LimparTags_Click(object sender, RoutedEventArgs e)
        {
            ComandoLimpeza.GetInstance.LimpezaTags.Raise();
        }

        // Tags em Conexões
        private void AdicionarTagsConexoes_Click(object sender, RoutedEventArgs e)
        {
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
            MaterialLuvaSelecionado = ComboListaLuvasMaterial.SelectedItem.ToString();

            ICollection<Element> luvas =
                new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeFitting).ToElements();

            foreach (Element b in luvas)
            {
                try
                {
                    dynamic luva = b;
                    dynamic isFamilyInstanceLuvas = luva.Family;

                    if (isFamilyInstanceLuvas == null)
                    {
                        luvas.Remove(b);
                    }
                }
                catch (Exception)
                {
                }
            }
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
                ValorUsuarioLuva = 0;
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
            AdicLuvas.GetInstance.AdicionarLuvas.Raise();
        }

        private void ComboListaSistema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SistemaAlvo = ComboListaSistema.SelectedItem.ToString();
        }
    }
}