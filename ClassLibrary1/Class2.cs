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

            ICollection<Element> identificadores =
                new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeTags).ToElements();

            Element TagSelecionada = null;

            foreach (Element g in identificadores)
            {
                try
                {
                    dynamic elemento = g;
                    dynamic isFamilyInstance = elemento.Family;

                    if (isFamilyInstance != null)
                    {
                        // Acessa o símbolo da família aqui
                        FamilySymbol fmanager = g as FamilySymbol;

                        if (fmanager != null)
                        {
                            if (fmanager.Name.Equals(UserControl2.TipoTagSelecionada))
                            {
                                TagSelecionada = g;
                                break;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (TagSelecionada != null)
            {

                foreach (Element item in tubulacoes)
                {
                    Parameter Comprimento = item.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);


                    if (Comprimento != null)
                    {
                        double ValorComprimento = UnitUtils.Convert(Comprimento.AsDouble(), DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_METERS);

                        if (ValorComprimento >= UserControl2.ValorUsuario)
                        {
                            var refe = new Reference(item);
                            int indexVariacao = 0;
                            XYZ PosicaoFinalTag = null;
                            // Determina a posição da Tag (XYZ)
                            var posicaoTag = item.get_BoundingBox(Doc.ActiveView).Max;
                            var posicaominima = item.get_BoundingBox(Doc.ActiveView).Min;

                            var DifPosX = (posicaoTag.X - posicaominima.X);
                            var DifPosY = (posicaoTag.Y - posicaominima.Y);
                            var DifPosZ = (posicaoTag.Z - posicaominima.Z);

                            double variacao = 0;

                            if (DifPosX > DifPosZ && DifPosX > DifPosY)
                                variacao = 1;
                            else if (DifPosY > DifPosX && DifPosX > DifPosZ)
                                variacao = 2;
                            else
                                variacao = 3;

                            PosicaoFinalTag = posicaoTag;


                            switch (variacao)
                            {
                                case 1:
                                    DifPosX = posicaoTag.X - ((posicaoTag.X - posicaominima.X) / 2);
                                    PosicaoFinalTag = new XYZ(DifPosX, posicaoTag.Y, posicaoTag.Z);
                                    break;
                                case 2:
                                    DifPosY = posicaoTag.Y - ((posicaoTag.Y - posicaominima.Y) / 2);
                                    PosicaoFinalTag = new XYZ(posicaoTag.X, DifPosY, posicaoTag.Z);
                                    break;
                                case 3:
                                    DifPosZ = posicaoTag.Z - ((posicaoTag.Z - posicaominima.Z) / 2);
                                    PosicaoFinalTag = new XYZ(posicaoTag.X, posicaoTag.Y, DifPosZ);
                                    break;
                            }

                            try
                            {
                                Transaction t = new Transaction(Doc.Document, "Adicionar Tag");
                                t.Start();

                                IndependentTag tag = IndependentTag.Create(
                                Doc.Document, TagSelecionada.Id, Doc.ActiveView.Id, refe,
                                false,
                                 TagOrientation.Horizontal, PosicaoFinalTag);

                                t.Commit();

                                if (tag != null)
                                {

                                }
                            }
                            catch (Exception er)
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
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