using Autodesk.Revit.UI;
using ClassLibrary1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClassLibrary1
{
    internal class TagsDisponiveis
    {
        // TagsDisponiveis JanelaRevit = new TagsDisponiveis();
        public void JanelaRevit(object sender, EventArgs e)
        {
            MessageBox.Show("Testando");
            return;
        }
    }
}


public class ComandoTags : IExternalEventHandler
{
    public ExternalEvent cTags;

    // Constructor
    private ComandoTags()
    {
    }

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