using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ComandosRevit;
using Janelas;
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

namespace JanelaConexao
{
    /// <summary>
    /// Interação lógica para JanelaProdutividade.xam
    /// </summary>
    public partial class JanelaDasConexoes : UserControl
    {
        UIDocument Doc;
        public JanelaDasConexoes(UIDocument doc)
        {
            InitializeComponent();
        }

        public void VerificarSistemas()
        {
            //Conexões

            ICollection<Element> tagsconexoes =
                 new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeFittingTags).ToElements();

            IList<string> TagsConexoesAdicionados = new List<string>();

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
        }

        public static string NomeTagConexaoSelecionada = "";
        public static string TipoTagConexaoSelecionada = "";

        // Tags em Conexões
        private void AdicionarTagsConexoes_Click(object sender, RoutedEventArgs e)
        {
            if (JanelaPrincipal.comboSistema == -1)
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
    }
}
