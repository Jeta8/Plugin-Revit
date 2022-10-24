using Autodesk.Revit.UI;
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
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;


namespace ClassLibrary1
{
    /// <summary>
    /// Interação lógica para UserControl2.xam
    /// </summary>
    public partial class UserControl2 : UserControl
    {
        public sealed class ComandoTags : IExternalEventHandler
        {
            public ExternalEvent cTags;

            // Constructor
            private ComandoTags()
            {
            }

            
            private static readonly ComandoTags _instance = new ComandoTags();
            public static ComandoTags GetInstance
            {
                get
                {
                    return _instance;
                }
            }

            public void Execute(UIApplication app)
            {
                // Aqui dentro é executado a lógica do programa
                ExternalEvent exComandoTags = ExternalEvent.Create(ComandoTags.GetInstance);
                ComandoTags.GetInstance.cTags = exComandoTags;

             //   Form1 JanelaRevit = new Form1();
             //   JanelaRevit.ShowDialog();

            }

            public string GetName()
            {
                return "Comando Tags";
            }
        }
        public UserControl2()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Exibir.Visibility = Visibility.Hidden;
            Conteudo.Visibility = Visibility.Visible;
            
            
        }
    }
}
