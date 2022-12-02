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
using Autodesk.Revit.DB.Electrical;
using System.Diagnostics;

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

                    if (Comprimento != null)
                    {
                        double ValorComprimento = UnitUtils.Convert(Comprimento.AsDouble(), DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_METERS);                     

                        if (ValorComprimento >= UserControl2.ValorUsuario)
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
            var AcessTag = TagsAcessorios.TagsAcessoriosDoUnmep;
            var PecasTag = TagsPecasHidro.TagsPecasDoUnmep;

            if (uTag != null || ConTag != null || AcessTag != null || PecasTag != null)
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

                            IndependentTag tagConexao = IndependentTag.Create(
                            Doc.Document, TagAcessorioSelecionado.Id, Doc.ActiveView.Id, refe,
                            true, TagOrientation.Horizontal, PosicaoFinal);

                            //var callLine = tagConexao.get_Parameter(BuiltInParameter.LEADER_LINE);
                            //callLine.Set(1);
                            tagConexao.LeaderElbow = PosicaoFinal;
                            // LeaderElbow para mudar posição 
                            // https://forums.autodesk.com/t5/revit-api-forum/default-leader-length/td-p/9813768

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
                            if (fcmanager.Name.Equals(UserControl2.TipoTagPecaSelecionado))
                            {
                                TagPecaSelecionada = d;
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

                        // Determina a posição da Tag (XYZ)
                        var posicaoTagPecas = itempecas.get_BoundingBox(Doc.ActiveView).Max;
                        var posicaominimaPecas = itempecas.get_BoundingBox(Doc.ActiveView).Min;

                        // Checagem de posicionamento da tubulação (Horizontal varia em X, Vertical varia em Z)

                        var DifPosX = (posicaoTagPecas.X - posicaominimaPecas.X);
                        var DifPosY = (posicaoTagPecas.Y - posicaominimaPecas.Y);
                        var DifPosZ = (posicaoTagPecas.Z - posicaominimaPecas.Z);


                        XYZ PosicaoFinal = new XYZ(posicaoTagPecas.X - (DifPosX / 2), posicaoTagPecas.Y - (DifPosY / 2), posicaoTagPecas.Z - (DifPosZ / 2));

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

                                var posicConec = k.Origin;
                               
                                IndependentTag tagPeca = IndependentTag.Create(
                                 Doc.Document, TagPecaSelecionada.Id, Doc.ActiveView.Id, refe,
                                 true, TagOrientation.Vertical, posicConec);

                                tagPeca.LeaderEndCondition = LeaderEndCondition.Free;
                                tagPeca.LeaderEnd = posicConec;
                               

                                if (UserControl2.direcoesNomes == UserControl2.Direcoes.Cima)
                                {                        
                                    tagPeca.TagHeadPosition = new XYZ(posicConec.X, posicConec.Y, (posicConec.Z + UserControl2.TamanhoLinhaTag));
                                }
                                else if (UserControl2.direcoesNomes == UserControl2.Direcoes.Direita)
                                {
                                    tagPeca.TagHeadPosition = new XYZ(posicConec.X + UserControl2.TamanhoLinhaTag, posicConec.Y, posicConec.Z);
                                }
                                else if (UserControl2.direcoesNomes == UserControl2.Direcoes.Baixo)
                                {
                                    tagPeca.TagHeadPosition = new XYZ(posicConec.X, posicConec.Y, posicConec.Z - UserControl2.TamanhoLinhaTag);
                                }
                                else if (UserControl2.direcoesNomes == UserControl2.Direcoes.Esquerda)
                                {
                                    tagPeca.TagHeadPosition = new XYZ(posicConec.X - UserControl2.TamanhoLinhaTag, posicConec.Y, posicConec.Z );
                                }

                                t.Commit();

                                if (tagPeca != null)
                                {
                                    TagsPecasDoUnmep.Add(tagPeca.Id);
                                }
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
            return "Comando Tags Peças";
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