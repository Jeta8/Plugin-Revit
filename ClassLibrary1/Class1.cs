using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClassLibrary1;
using SegundaBiblioteca;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace ClassLibrary1
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Evento para abrir a janela do plugin
            UIApplication uiapp = commandData.Application;
            UserControl2 janela = new UserControl2(uiapp.ActiveUIDocument);
            janela.Show();

            // Evento para adicionar as tags nas tubulações
            ExternalEvent exComandoTags = ExternalEvent.Create(ComandoTags.GetInstance);
            ComandoTags.GetInstance.cTags = exComandoTags;

            // Evento para adicionar as tags nas conexoes
            ExternalEvent ComandoTagsConexoes = ExternalEvent.Create(TagsConexoes.GetInstance);
            TagsConexoes.GetInstance.TagsConex = ComandoTagsConexoes;

            // Evento para adicionar as tags nos acessorios
            ExternalEvent ComandoTagsAcessorios = ExternalEvent.Create(TagsAcessorios.GetInstance);
            TagsAcessorios.GetInstance.TagsAcess = ComandoTagsAcessorios;

            // Evento para adicionar as tags nas peças
            ExternalEvent ComandoTagsPecas = ExternalEvent.Create(TagsPecasHidro.GetInstance);
            TagsPecasHidro.GetInstance.TagsPecas = ComandoTagsPecas;

            // Evento para adicionar as luvas nas tubulações
            ExternalEvent ComandoLuvas = ExternalEvent.Create(AdicLuvas.GetInstance);
            AdicLuvas.GetInstance.AdicionarLuvas = ComandoLuvas;

            // Evento para apagar as tags
            ExternalEvent comandoLimpeza = ExternalEvent.Create(ComandoLimpeza.GetInstance);
            ComandoLimpeza.GetInstance.LimpezaTags = comandoLimpeza;
            

            

            return Result.Succeeded;

        }
    }
}