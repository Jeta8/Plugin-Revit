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
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace SegundaBiblioteca
{
    public class ComandoTags : IExternalEventHandler
    {
        public ExternalEvent cTags;


        public static ICollection<ElementId> TagsDoUnmep = new Collection<ElementId>();

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
        { // Comando que adiciona as tags ao Revit
            UIDocument Doc = app.ActiveUIDocument;


            // Coleções que armazenam as tubulações e tags do projeto do usuário
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
                    Parameter DiametroTub = item.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);

                    if (Comprimento != null)
                    {
                        double ValorComprimento = UnitUtils.Convert(Comprimento.AsDouble(), DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_METERS);
                        double ValorDiametro = UnitUtils.Convert(DiametroTub.AsDouble(), DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_MILLIMETERS);

                        if (ValorComprimento >= UserControl2.ValorUsuario)
                        {
                            var refe = new Reference(item);

                            XYZ PosicaoFinalTag = null;
                            // Determina a posição da Tag (XYZ)
                            var posicaoTag = item.get_BoundingBox(Doc.ActiveView).Max;
                            var posicaominima = item.get_BoundingBox(Doc.ActiveView).Min;

                            // Checagem de posicionamento da tubulação (Horizontal varia em X, Vertical varia em Z)

                            var DifPosX = (posicaoTag.X - posicaominima.X);
                            var DifPosY = (posicaoTag.Y - posicaominima.Y);
                            var DifPosZ = (posicaoTag.Z - posicaominima.Z);
                            var DifDiam = (((ValorDiametro / 2) * 0.01) - 0.04);

                            XYZ PosicaoFinal = new XYZ((posicaoTag.X - (DifPosX / 2)), posicaoTag.Y - (DifPosY / 2), posicaoTag.Z - (DifPosZ / 2));


                            //if (DifPosX > DifPosZ && DifPosX > DifPosY)
                            //    variacao = 1;
                            //else if (DifPosY > DifPosX && DifPosX > DifPosZ)
                            //    variacao = 2;
                            //else
                            //    variacao = 3;

                            //PosicaoFinalTag = posicaoTag;

                            //// Switch com as possibilidades de tubulações (Aqui também é onde especifica que a tag ficará no centro da tubulação)

                            //switch (variacao)
                            //{
                            //    case 1:
                            //        DifPosX = posicaoTag.X - ((posicaoTag.X - posicaominima.X) / 2);
                            //        PosicaoFinalTag = new XYZ(DifPosX, (posicaoTag.Y - DifDiam), posicaoTag.Z);
                            //        break;
                            //    case 2:
                            //        DifPosY = posicaoTag.Y - ((posicaoTag.Y - posicaominima.Y) / 2);
                            //        PosicaoFinalTag = new XYZ(posicaoTag.X - 10, (DifPosY - DifDiam), posicaoTag.Z  );
                            //        break;
                            //    case 3:
                            //        DifPosZ = posicaoTag.Z - ((posicaoTag.Z - posicaominima.Z) / 2);
                            //        PosicaoFinalTag = new XYZ(posicaoTag.X, (posicaoTag.Y - DifDiam), DifPosZ);
                            //        break;
                            //}
                            // 10358270
                            try
                            {
                                // Comando que diz qual a vista, qual tag, tubulação de referência e qual posição o Revit usará pra colocar a tag
                                Transaction t = new Transaction(Doc.Document, "Adicionar Tag");
                                t.Start();

                                IndependentTag tag = IndependentTag.Create(
                                Doc.Document, TagSelecionada.Id, Doc.ActiveView.Id, refe,
                                false, TagOrientation.Horizontal, PosicaoFinal);

                                t.Commit();

                                if (tag != null)
                                {
                                    TagsDoUnmep.Add(tag.Id);
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

    public class ComandoLimpeza : IExternalEventHandler
    {
        public ExternalEvent LimpezaTags;

        // Constructor
        private ComandoLimpeza()
        {
        }

        private static readonly ComandoLimpeza _instanceLimpeza = new ComandoLimpeza();
        public static ComandoLimpeza GetInstance
        {
            get
            {
                return _instanceLimpeza;
            }
        }

        // Comando ao Revit para apagar as tags
        public void Execute(UIApplication app)
        {
            var uTag = ComandoTags.TagsDoUnmep;
            if (uTag != null)
            {
                Transaction p = new Transaction(app.ActiveUIDocument.Document, "Limpar Tag");
                p.Start();
                try
                {
                    foreach (ElementId i in uTag)
                    {
                        app.ActiveUIDocument.Document.Delete(i);
                    }

                    p.Commit();
                    uTag.Clear();
                }
                catch (Exception e) { }
            }
        }

        public string GetName()
        {
            return "Comando Limpeza";
        }
    }
    internal class TagsDisponiveis
    {
        public void JanelaRevit(object sender, EventArgs e)
        {
            return;
        }
    }
}