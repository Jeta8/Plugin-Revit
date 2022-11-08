using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using ClassLibrary1;
using System.Windows;

namespace SegundaBiblioteca
{
    public class ComandoTags : IExternalEventHandler
    {
        public ExternalEvent cTags;


        // Constructor
        private ComandoTags()
        {
        }

        public static object GetInstance { get; internal set; }

        public void Execute(UIApplication app)
        {

            // Aqui dentro é executado a lógica do programa
            UserControl2 JanelaRevit = new UserControl2();
            JanelaRevit.Show();

        }

        public string GetName()
        {
            return "Comando Tags";
        }
    }

    internal class TagsDisponiveis
    {
        // TagsDisponiveis JanelaRevit = new TagsDisponiveis();
        public void JanelaRevit(object sender, EventArgs e)
        {
            
            return;
        }
    }

}