﻿using System;
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
using System.Windows.Controls;
using Autodesk.Revit.DB.Plumbing;

namespace SegundaBiblioteca
{
    public class ComandoTags : IExternalEventHandler
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
            UIDocument Doc = app.ActiveUIDocument;


            ICollection<Element> tubulacoes =
            new FilteredElementCollector(Doc.Document, Doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_PipeCurves).ToElements();

            var localzi = tubulacoes.First().Location as LocationCurve;
            var simbolos = tubulacoes.First().get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM);
            Parameter IDsimbolo = tubulacoes.First().get_Parameter(BuiltInParameter.SYMBOL_ID_PARAM);


            ICollection<Element> identificadores =
                new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeTags).ToElements();

            Element TagSelecionada = null;

            foreach (Element g in identificadores)
            {
                Parameter t = g.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM);

                if (t != null && t.AsValueString() != null)
                {
                    if (t.AsValueString().Equals(UserControl2.NomeTagSelecionada))
                    {
                        TagSelecionada = g;
                        break;
                    }
                }
            }

            if (TagSelecionada != null)
            {
                var refe = new Reference(tubulacoes.First());

                Parameter SymbolTag = TagSelecionada.get_Parameter(BuiltInParameter.SYMBOL_ID_PARAM);

                try
                {
                    Transaction t = new Transaction(Doc.Document, "Adicionar Tag");
                    t.Start();


                    IndependentTag tag = IndependentTag.Create(
                    Doc.Document, SymbolTag.AsElementId(), Doc.ActiveView.Id, refe,
                    false,
                     TagOrientation.Horizontal, XYZ.BasisZ);

                    t.Commit();

                    if (tag != null)
                    {

                    }
                }
                catch (Exception er)
                {

                }


            }

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