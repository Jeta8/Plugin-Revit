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
using System.Windows.Ink;

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
            var ConTag = TagsConexoes.TagsConexoesDoUnmep;
            if (uTag != null || ConTag != null)
            {
                Transaction p = new Transaction(app.ActiveUIDocument.Document, "Limpar Tag");
                p.Start();
                try
                {
                    foreach (ElementId i in uTag)
                    {
                        app.ActiveUIDocument.Document.Delete(i);
                    }
                    foreach (ElementId o in ConTag)
                    {
                        app.ActiveUIDocument.Document.Delete(o);
                    }

                    p.Commit();
                    uTag.Clear();
                    ConTag.Clear();
                }
                catch (Exception e) { }
            }
        }

        public string GetName()
        {
            return "Comando Limpeza";
        }
    }

    public class TagsConexoes : IExternalEventHandler
    {
        public ExternalEvent TagsConex;

        public static ICollection<ElementId> TagsConexoesDoUnmep = new Collection<ElementId>();
        public static ICollection<ElementId> TagsAcessoriosDoUnmep = new Collection<ElementId>();

        // Constructor
        private TagsConexoes()
        {
        }

        private static readonly TagsConexoes _instancia = new TagsConexoes();
        public static TagsConexoes GetInstance
        {
            get
            {
                return _instancia;
            }
        }

        public void Execute(UIApplication app)
        {
            UIDocument Doc = app.ActiveUIDocument;

            ICollection<Element> conexoes =
                new FilteredElementCollector(Doc.Document, Doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_PipeFitting).ToElements();

            ICollection<Element> tagsconexoes =
                new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeFittingTags).ToElements();

            ICollection<Element> acessorios = 
                new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeAccessory).ToElements();

            ICollection<Element> tagsacessorios =
                 new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeAccessoryTags).ToElements();

            Element TagConexSelecionada = null;
            Element TagAcessorioSelecionado = null;


            foreach (Element g in tagsconexoes)
            {
                try
                {

                    dynamic elemento = g;
                    dynamic isFamilyInstance = elemento.Family;

                    if (isFamilyInstance != null)
                    {
                        // Acessa o símbolo da família aqui
                        FamilySymbol fcmanager = g as FamilySymbol;

                        if (fcmanager != null)
                        {
                            if (fcmanager.Name.Equals(UserControl2.TipoTagConexaoSelecionada))
                            {
                                TagConexSelecionada = g;
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
            if (TagConexSelecionada != null)
            {
                foreach (Element itemconex in conexoes)
                {
                    try
                    {
                        FamilyInstance VerificarSuperComp = itemconex as FamilyInstance;
                        if (VerificarSuperComp.SuperComponent != null)
                        {
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                    }
                    if (itemconex != null)
                    {
                        var refe = new Reference(itemconex);
                        // Determina a posição da Tag (XYZ)
                        var posicaoTagConex = itemconex.get_BoundingBox(Doc.ActiveView).Max;
                        var posicaominimaConex = itemconex.get_BoundingBox(Doc.ActiveView).Min;

                        // Checagem de posicionamento da tubulação (Horizontal varia em X, Vertical varia em Z)

                        var DifPosX = (posicaoTagConex.X - posicaominimaConex.X);
                        var DifPosY = (posicaoTagConex.Y - posicaominimaConex.Y);
                        var DifPosZ = (posicaoTagConex.Z - posicaominimaConex.Z);


                        XYZ PosicaoFinal = new XYZ(posicaoTagConex.X - (DifPosX / 2), posicaoTagConex.Y - (DifPosY / 2), posicaoTagConex.Z - (DifPosZ / 2));

                        try
                        {
                            // Comando que diz qual a vista, qual tag, tubulação de referência e qual posição o Revit usará pra colocar a tag
                            Transaction t = new Transaction(Doc.Document, "Adicionar Tag na Conexão");
                            t.Start();

                            IndependentTag tagConexao = IndependentTag.Create(
                            Doc.Document, TagConexSelecionada.Id, Doc.ActiveView.Id, refe,
                            true, TagOrientation.Horizontal, PosicaoFinal);

                            t.Commit();

                            if (tagConexao != null)
                            {
                                TagsConexoesDoUnmep.Add(tagConexao.Id);
                            }
                        }
                        catch (Exception er)
                        {
                            continue;
                        }

                    }
                    else
                    {
                        continue;
                    }
                }
            }

            // Acessorios

            foreach (Element d in tagsacessorios)
            {


                try
                {

                    dynamic elemento = d;
                    dynamic isFamilyInstance = elemento.Family;

                    if (isFamilyInstance != null)
                    {
                        // Acessa o símbolo da família aqui
                        FamilySymbol fcmanager = d as FamilySymbol;

                        if (fcmanager != null)
                        {
                            if (fcmanager.Name.Equals(UserControl2.TipoTagAcessorioSelecionado))
                            {
                                TagAcessorioSelecionado = d;
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
            if (TagAcessorioSelecionado != null)
            {

                foreach (Element itemacess in acessorios)
                {
                    try
                    {
                        FamilyInstance VerificarSuperComp = itemacess as FamilyInstance;
                        if (VerificarSuperComp != null && VerificarSuperComp.SuperComponent != null)
                        {
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                    }
                    if (itemacess != null)
                    {
                        try
                        {
                            if (itemacess.LevelId.IntegerValue == -1)
                            {
                                continue;
                            } ;
                        }
                        catch(Exception e) { }


                        var refe = new Reference(itemacess);
                        // Determina a posição da Tag (XYZ)
                        var posicaoTagAcess = itemacess.get_BoundingBox(Doc.ActiveView).Max;
                        var posicaominimaAcess = itemacess.get_BoundingBox(Doc.ActiveView).Min;

                        // Checagem de posicionamento da tubulação (Horizontal varia em X, Vertical varia em Z)

                        var DifPosX = (posicaoTagAcess.X - posicaominimaAcess.X);
                        var DifPosY = (posicaoTagAcess.Y - posicaominimaAcess.Y);
                        var DifPosZ = (posicaoTagAcess.Z - posicaominimaAcess.Z);


                        XYZ PosicaoFinal = new XYZ(posicaoTagAcess.X - (DifPosX / 2), posicaoTagAcess.Y - (DifPosY / 2), posicaoTagAcess.Z - (DifPosZ / 2));

                        try
                        {
                            // Comando que diz qual a vista, qual tag, tubulação de referência e qual posição o Revit usará pra colocar a tag
                            Transaction t = new Transaction(Doc.Document, "Adicionar Tag na Conexão");
                            t.Start();

                            IndependentTag tagConexao = IndependentTag.Create(
                            Doc.Document, TagConexSelecionada.Id, Doc.ActiveView.Id, refe,
                            true, TagOrientation.Horizontal, PosicaoFinal);

                            t.Commit();

                            if (tagConexao != null)
                            {
                                TagsAcessoriosDoUnmep.Add(tagConexao.Id);
                            }
                        }
                        catch (Exception er)
                        {
                            continue;
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
            return "Comando Tags Acessorios";
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