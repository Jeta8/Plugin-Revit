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
            ExternalEvent exComandoTags = ExternalEvent.Create(ComandoTags.GetInstance);
            ComandoTags.GetInstance.cTags = exComandoTags;

            UIApplication uiapp = commandData.Application;
            UserControl2 janela = new UserControl2(uiapp.ActiveUIDocument);
            janela.Show();



            return Result.Succeeded;

        }
    }
}