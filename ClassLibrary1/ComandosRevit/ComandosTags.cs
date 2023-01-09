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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Windows.Ink;
using Autodesk.Revit.DB.Electrical;
using System.Diagnostics;
using System.Windows.Forms;

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
                            if (fmanager.FamilyName.Equals(UserControl2.NomeTagSelecionada))
                            {
                                if (fmanager.Name.Equals(UserControl2.TipoTagSelecionada))
                                {
                                    TagSelecionada = g;
                                    break;
                                }
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

                        if (ValorComprimento >= UserControl2.ValorUsuarioTubo)
                        {
                            var refe = new Reference(item);


                            // Determina a posição da Tag (XYZ)
                            var posicaoTag = item.get_BoundingBox(Doc.ActiveView).Max;
                            var posicaominima = item.get_BoundingBox(Doc.ActiveView).Min;

                            // Checagem de posicionamento da tubulação (Horizontal varia em X, Vertical varia em Z)

                            var DifPosX = (posicaoTag.X - posicaominima.X);
                            var DifPosY = (posicaoTag.Y - posicaominima.Y);
                            var DifPosZ = (posicaoTag.Z - posicaominima.Z);

                            XYZ PosicaoFinal = new XYZ((posicaoTag.X - (DifPosX / 2)), posicaoTag.Y - (DifPosY / 2), posicaoTag.Z - (DifPosZ / 2));

                            try
                            {
                                // Comando que diz qual a vista, qual tag, tubulação de referência e qual posição o Revit usará pra colocar a tag
                                Transaction t = new Transaction(Doc.Document, "Adicionar Tag");
                                t.Start();


                                Parameter Sistemas = item.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
                                if (Sistemas != null && Sistemas.AsValueString() != null)
                                {
                                    if (Sistemas.AsValueString().Equals(UserControl2.SistemaAlvo))
                                    {
                                        IndependentTag tag = IndependentTag.Create(
                                        Doc.Document, TagSelecionada.Id, Doc.ActiveView.Id, refe,
                                        false, TagOrientation.Horizontal, PosicaoFinal);

                                        if (tag != null)
                                        {
                                            TagsDoUnmep.Add(tag.Id);
                                        }
                                    }
                                }

                                t.Commit();

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
            var AcessTag = TagsAcessorios.TagsAcessoriosDoUnmep;
            var PecasTag = TagsPecasHidro.TagsPecasDoUnmep;

            if (uTag != null || ConTag != null || AcessTag != null || PecasTag != null)
            {

                try
                {
                    Transaction p = new Transaction(app.ActiveUIDocument.Document, "Limpar Tag");
                    p.Start();
                    foreach (ElementId i in uTag)
                    {
                        app.ActiveUIDocument.Document.Delete(i);
                    }
                    foreach (ElementId o in ConTag)
                    {
                        app.ActiveUIDocument.Document.Delete(o);
                    }
                    foreach (ElementId u in AcessTag)
                    {
                        app.ActiveUIDocument.Document.Delete(u);
                    }
                    foreach (ElementId r in PecasTag)
                    {
                        app.ActiveUIDocument.Document.Delete(r);
                    }

                    p.Commit();
                    uTag.Clear();
                    ConTag.Clear();
                    AcessTag.Clear();
                    PecasTag.Clear();
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


            Element TagConexSelecionada = null;

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
                            if (fcmanager.FamilyName.Equals(UserControl2.NomeTagConexaoSelecionada))
                            {
                                if (fcmanager.Name.Equals(UserControl2.TipoTagConexaoSelecionada))
                                {
                                    TagConexSelecionada = g;
                                    break;
                                }
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

                            Parameter Sistemas = itemconex.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
                            if (Sistemas != null && Sistemas.AsValueString() != null)
                            {
                                if (Sistemas.AsValueString().Equals(UserControl2.SistemaAlvo))
                                {
                                    IndependentTag tagConexao = IndependentTag.Create(
                                    Doc.Document, TagConexSelecionada.Id, Doc.ActiveView.Id, refe,
                                    true, TagOrientation.Horizontal, PosicaoFinal);

                                    if (tagConexao != null)
                                    {
                                        TagsConexoesDoUnmep.Add(tagConexao.Id);
                                    }
                                }
                            }

                            t.Commit();

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

    public class TagsAcessorios : IExternalEventHandler
    {
        public ExternalEvent TagsAcess;

        public static ICollection<ElementId> TagsAcessoriosDoUnmep = new Collection<ElementId>();
        // Constructor
        private TagsAcessorios()
        {
        }
        private static readonly TagsAcessorios _instancia = new TagsAcessorios();
        public static TagsAcessorios GetInstance
        {
            get
            {
                return _instancia;
            }
        }
        public void Execute(UIApplication app)
        {
            UIDocument Doc = app.ActiveUIDocument;

            ICollection<Element> acessorios =
              new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeAccessory).ToElements();

            ICollection<Element> tagsacessorios =
                 new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeAccessoryTags).ToElements();

            Element TagAcessorioSelecionado = null;

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
                            if (fcmanager.FamilyName.Equals(UserControl2.NomeTagAcessorioSelecionado))
                            {
                                if (fcmanager.Name.Equals(UserControl2.TipoTagAcessorioSelecionado))
                                {
                                    TagAcessorioSelecionado = fcmanager;
                                    break;
                                }
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
                            };
                        }
                        catch (Exception e) { }


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
                            Transaction t = new Transaction(Doc.Document, "Adicionar Tag no Acessório");
                            t.Start();


                            Parameter Sistemas = itemacess.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
                            if (Sistemas != null && Sistemas.AsValueString() != null)
                            {
                                if (Sistemas.AsValueString().Equals(UserControl2.SistemaAlvo))
                                {
                                    IndependentTag tagAcessorio = IndependentTag.Create(
                                    Doc.Document, TagAcessorioSelecionado.Id, Doc.ActiveView.Id, refe,
                                    true, TagOrientation.Horizontal, PosicaoFinal);

                                    tagAcessorio.LeaderEndCondition = LeaderEndCondition.Free;
                                    tagAcessorio.LeaderEnd = PosicaoFinal;
                                    if (tagAcessorio != null)
                                    {
                                        TagsAcessoriosDoUnmep.Add(tagAcessorio.Id);
                                    }
                                }
                            }



                            t.Commit();


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

    public class TagsPecasHidro : IExternalEventHandler
    {
        public ExternalEvent TagsPecas;

        public static ICollection<ElementId> TagsPecasDoUnmep = new Collection<ElementId>();
        public static ICollection<ElementId> Conectores = new Collection<ElementId>();
        // Constructor
        private TagsPecasHidro()
        {
        }
        private static readonly TagsPecasHidro _instancia = new TagsPecasHidro();
        public static TagsPecasHidro GetInstance
        {
            get
            {
                return _instancia;
            }
        }
        public static ConnectorSet GetConnectors(Element e)
        {
            ConnectorSet connectors = null;

            if (e is FamilyInstance)
            {
                MEPModel m = ((FamilyInstance)e).MEPModel;

                if (null != m
                  && null != m.ConnectorManager)
                {
                    connectors = m.ConnectorManager.Connectors;

                }
            }
            else
            {
                if (e is MEPCurve)
                {
                    connectors = ((MEPCurve)e)
                      .ConnectorManager.Connectors;
                }
            }

            return connectors;
        }


        public void Execute(UIApplication app)
        {
            UIDocument Doc = app.ActiveUIDocument;
            View3D activeView3D = app.ActiveUIDocument.ActiveView as View3D;

            ICollection<Element> pecas =
              new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PlumbingFixtures).ToElements();

            ICollection<Element> tagspecas =
                 new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PlumbingFixtureTags).ToElements();

            Element TagPecaSelecionada = null;


            foreach (Element d in tagspecas)
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
                            if (fcmanager.FamilyName.Equals(UserControl2.NomeTagPecaSelecionada))
                            {
                                if (fcmanager.Name.Equals(UserControl2.TipoTagPecaSelecionado))
                                {
                                    TagPecaSelecionada = d;
                                    break;
                                }
                            }

                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            if (TagPecaSelecionada != null)
            {
                foreach (Element itempecas in pecas)
                {
                    try
                    {
                        FamilyInstance VerificarSuperComp = itempecas as FamilyInstance;
                        if (VerificarSuperComp != null && VerificarSuperComp.SuperComponent != null)
                        {
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                    }
                    if (itempecas != null)
                    {
                        try
                        {
                            if (itempecas.LevelId.IntegerValue == -1)
                            {
                                continue;
                            };
                        }
                        catch (Exception e) { }

                        var refe = new Reference(itempecas);


                        try
                        {
                            // Comando que diz qual a vista, qual tag, tubulação de referência e qual posição o Revit usará pra colocar a tag
                            Transaction t = new Transaction(Doc.Document, "Adicionar Tag na Peça");
                            t.Start();

                            var sd = GetConnectors(itempecas);

                            foreach (Connector k in sd)
                            {
                                if (k.Direction == FlowDirectionType.Out)
                                    continue;

                                // eyePosition = X, upDirection = Y, forwardDirection = Z

                                var Viewtest = activeView3D.GetOrientation().EyePosition;
                                var posicConec = k.Origin;

                                Parameter Sistemas = itempecas.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
                                if (Sistemas != null && Sistemas.AsValueString() != null)
                                {
                                    if (Sistemas.AsValueString().Equals(UserControl2.SistemaAlvo))
                                    {
                                        IndependentTag tagPeca = IndependentTag.Create(
                                            Doc.Document, TagPecaSelecionada.Id, Doc.ActiveGraphicalView.Id, refe,
                                            true, TagOrientation.Vertical, posicConec);

                                        tagPeca.LeaderEndCondition = LeaderEndCondition.Free;
                                        tagPeca.LeaderEnd = posicConec;



                                        if (UserControl2.direcoesNomes == UserControl2.Direcoes.Cima)
                                        {
                                            tagPeca.TagHeadPosition = new XYZ(posicConec.X, posicConec.Y, (posicConec.Z + UserControl2.TamanhoLinhaTag));
                                        }
                                        else if (UserControl2.direcoesNomes == UserControl2.Direcoes.Direita)
                                        {
                                            tagPeca.TagHeadPosition = new XYZ(posicConec.X + UserControl2.TamanhoLinhaTag, posicConec.Y + UserControl2.TamanhoLinhaTag, posicConec.Z);
                                        }
                                        else if (UserControl2.direcoesNomes == UserControl2.Direcoes.Baixo)
                                        {
                                            tagPeca.TagHeadPosition = new XYZ(posicConec.X, posicConec.Y, posicConec.Z - UserControl2.TamanhoLinhaTag);
                                        }
                                        else if (UserControl2.direcoesNomes == UserControl2.Direcoes.Esquerda)
                                        {
                                            tagPeca.TagHeadPosition = new XYZ(posicConec.X - UserControl2.TamanhoLinhaTag, posicConec.Y - UserControl2.TamanhoLinhaTag, posicConec.Z);
                                        }

                                        if (tagPeca != null)
                                        {
                                            TagsPecasDoUnmep.Add(tagPeca.Id);
                                        }

                                    }
                                }


                            }
                            t.Commit();
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
            return "Comando Tags Peças";
        }
    }

    public class AdicLuvas : IExternalEventHandler
    {
        public ExternalEvent AdicionarLuvas;

        public static ICollection<ElementId> LuvasDoUnmep = new Collection<ElementId>();

        // Constructor
        private AdicLuvas()
        {
        }

        private static readonly AdicLuvas _instancia = new AdicLuvas();
        public static AdicLuvas GetInstance
        {
            get
            {
                return _instancia;
            }
        }


        public static List<Connector> GetClosestConnector(Element e1, Element e2)
        {
            ConnectorSet connectors1 = null;
            ConnectorSet connectors2 = null;


        List<Connector> Conexoes = new List<Connector> { };

            if (e1 is FamilyInstance)
            {
                MEPModel m = ((FamilyInstance)e1).MEPModel;

                if (null != m
                  && null != m.ConnectorManager)
                {
                    connectors1 = m.ConnectorManager.Connectors;
                }
            }
            else
            {
                if (e1 is MEPCurve)
                {
                    connectors1 = ((MEPCurve)e1)
                      .ConnectorManager.Connectors;
                }
            }

            if (e2 is FamilyInstance)
            {
                MEPModel m = ((FamilyInstance)e2).MEPModel;

                if (null != m
                  && null != m.ConnectorManager)
                {
                    connectors2 = m.ConnectorManager.Connectors;
                }
            }
            else
            {
                if (e2 is MEPCurve)
                {
                    connectors2 = ((MEPCurve)e2)
                      .ConnectorManager.Connectors;
                }
            }

            Connector pFinal1 = null;
            Connector pFinal2 = null;

            foreach (Connector cn1 in connectors1)
            {
                XYZ p1 = cn1.Origin;

                foreach (Connector cn2 in connectors2)
                {
                    if (p1.IsAlmostEqualTo(cn2.Origin))
                    {
                        pFinal1 = cn1;
                        pFinal2 = cn2;
                    }
                }
            }

            Conexoes.Add(pFinal2);
            Conexoes.Add(pFinal1);

            return Conexoes;
        }



‌
        public void Execute(UIApplication app)
        {
            UIDocument Doc = app.ActiveUIDocument;

            ICollection<Element> tubulacoes =
                new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeCurves).ToElements();

            ICollection<Element> luvas =
                new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeFitting).ToElements();

            Element LuvaSelecionada = null;

            foreach (Element z in luvas)
            {
                try
                {
                    dynamic elemento = z;
                    dynamic isFamilyInstance = elemento.Family;

                    if (isFamilyInstance != null)
                    {
                        FamilySymbol fcmanager = z as FamilySymbol;

                        if (fcmanager != null)
                        {

                            if (fcmanager.FamilyName.Equals(UserControl2.FamiliaLuvaSelecionada))
                            {
                                LuvaSelecionada = z;
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
            if (LuvaSelecionada != null)
            {
                Transaction j = new Transaction(Doc.Document, "Adicionar Luva na tubulação");
                j.Start();

                foreach (Element tb in tubulacoes)
                {

                    Parameter Comprimento = tb.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);

                    if (Comprimento != null)
                    {
                        double ValorComprimento = UnitUtils.Convert(Comprimento.AsDouble(), DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_METERS);

                        if (ValorComprimento >= UserControl2.ValorUsuarioLuva)
                        {
                           
                                LocationCurve c1 = (tb.Location as LocationCurve);


                                try
                                {
                                 

                                    //  var cf = GetConnectors(tb);

                                    Parameter Sistemas = tb.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
                                    if (Sistemas != null && Sistemas.AsValueString() != null)
                                    {
                                        if (Sistemas.AsValueString().Equals(UserControl2.SistemaAlvo))
                                        {

                                            var tubo = c1.Curve;
                                            var startPoint = tubo.GetEndPoint(0);
                                            var endPoint = tubo.GetEndPoint(1);

                                            XYZ splitpoint = endPoint.Subtract(startPoint).Normalize();

                                            try
                                            {
                                                for (double userValue = UserControl2.ValorUsuarioLuva; userValue < ValorComprimento; userValue *= 2)
                                                {

                                                    XYZ Split = startPoint.Add(splitpoint.Multiply(userValue / 3.281));
                                                    ElementId luvaColocada = PlumbingUtils.BreakCurve(Doc.Document, tb.Id, Split);
                                                    var cLv = GetClosestConnector(tb, tb);
                                                    Doc.Document.Create.NewUnionFitting();
                                                    
                                                    if (luvaColocada != null)
                                                    {
                                                        LuvasDoUnmep.Add(luvaColocada);
                                                    }
                                                }
                                            }
                                            catch (Exception er) { continue; }

                                            //PlumbingUtils luvaTubos = PlumbingUtils.ConnectPipePlaceholdersAtTee(Doc.Document,);

                                        }
                                    }



                                   
                                }
                                catch (Exception er)
                                {
                                    continue;
                                }
                            
                        }
                    }
                }
                j.Commit();
            }
        }

        private static Connector FindConnector(Pipe pipe, XYZ conXYZ)
        {
            ConnectorSet conns = pipe.ConnectorManager.Connectors;
            foreach (Connector conn in conns)
            {
                if (conn.Origin.IsAlmostEqualTo(conXYZ))
                {
                    return conn;
                }
            }
            return null;
        }
        private static Connector FindConnectedTo(Pipe pipe, XYZ conXYZ)
        {
            Connector connItself = FindConnector(pipe, conXYZ);
            ConnectorSet connSet = connItself.AllRefs;
            foreach (Connector conn in connSet)
            {
                if (conn.Owner.Id.IntegerValue != pipe.Id.IntegerValue &&
                    conn.ConnectorType == ConnectorType.End)
                {
                    return conn;
                }
            }
            return null;
        }


        public string GetName()
        {
            return "Comando Adicionar Luvas";
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
