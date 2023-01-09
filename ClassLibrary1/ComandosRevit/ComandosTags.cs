using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using System.Collections.ObjectModel;
using Janelas;
using System.Linq;
using System.Windows.Controls;

namespace ComandosRevit
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
        {
            // Comando que adiciona as tags ao Revit
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
                            if (fmanager.Name.Equals(JanelaPrincipal.TipoTagSelecionada))
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

                        if (ValorComprimento >= JanelaPrincipal.ValorUsuarioTubo)
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
                                    if (Sistemas.AsValueString().Equals(JanelaPrincipal.SistemaAlvo))
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
                            catch (Exception)
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
                            if (fcmanager.Name.Equals(JanelaPrincipal.TipoTagConexaoSelecionada))
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

                            Parameter Sistemas = itemconex.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
                            if (Sistemas != null && Sistemas.AsValueString() != null)
                            {
                                if (Sistemas.AsValueString().Equals(JanelaPrincipal.SistemaAlvo))
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
                        catch (Exception)
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
                            if (fcmanager.Name.Equals(JanelaPrincipal.TipoTagAcessorioSelecionado))
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
                        catch (Exception) { }


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
                                if (Sistemas.AsValueString().Equals(JanelaPrincipal.SistemaAlvo))
                                {
                                    IndependentTag tagConexao = IndependentTag.Create(
                                    Doc.Document, TagAcessorioSelecionado.Id, Doc.ActiveView.Id, refe,
                                    true, TagOrientation.Horizontal, PosicaoFinal);

                                    tagConexao.LeaderEndCondition = LeaderEndCondition.Free;
                                    tagConexao.LeaderEnd = PosicaoFinal;
                                    if (tagConexao != null)
                                    {
                                        TagsAcessoriosDoUnmep.Add(tagConexao.Id);
                                    }
                                }
                            }



                            t.Commit();


                        }
                        catch (Exception)
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
                            if (fcmanager.Name.Equals(JanelaPrincipal.TipoTagPecaSelecionado))
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
                        catch (Exception) { }

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
                                    if (Sistemas.AsValueString().Equals(JanelaPrincipal.SistemaAlvo))
                                    {
                                        IndependentTag tagPeca = IndependentTag.Create(
                                            Doc.Document, TagPecaSelecionada.Id, Doc.ActiveGraphicalView.Id, refe,
                                            true, TagOrientation.Vertical, posicConec);

                                        tagPeca.LeaderEndCondition = LeaderEndCondition.Free;
                                        tagPeca.LeaderEnd = posicConec;



                                        if (JanelaPrincipal.direcoesNomes == JanelaPrincipal.Direcoes.Cima)
                                        {
                                            tagPeca.TagHeadPosition = new XYZ(posicConec.X, posicConec.Y, (posicConec.Z + JanelaPrincipal.TamanhoLinhaTag));
                                        }
                                        else if (JanelaPrincipal.direcoesNomes == JanelaPrincipal.Direcoes.Direita)
                                        {
                                            tagPeca.TagHeadPosition = new XYZ(posicConec.X + JanelaPrincipal.TamanhoLinhaTag, posicConec.Y + JanelaPrincipal.TamanhoLinhaTag, posicConec.Z);
                                        }
                                        else if (JanelaPrincipal.direcoesNomes == JanelaPrincipal.Direcoes.Baixo)
                                        {
                                            tagPeca.TagHeadPosition = new XYZ(posicConec.X, posicConec.Y, posicConec.Z - JanelaPrincipal.TamanhoLinhaTag);
                                        }
                                        else if (JanelaPrincipal.direcoesNomes == JanelaPrincipal.Direcoes.Esquerda)
                                        {
                                            tagPeca.TagHeadPosition = new XYZ(posicConec.X - JanelaPrincipal.TamanhoLinhaTag, posicConec.Y - JanelaPrincipal.TamanhoLinhaTag, posicConec.Z);
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
                        catch (Exception)
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

        public List<Connector> GetClosestConnector(Element e1, Element e2)
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

        public void Execute(UIApplication app)
        {
            int x = 0;

            UIDocument Doc = app.ActiveUIDocument;

            // Pega as tubulações e luvas conforme categoria em todo o projeto       
            ICollection<Element> tubulações =
                new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeCurves).ToElements();

            ICollection<Element> luvas =
                new FilteredElementCollector(Doc.Document).OfCategory(BuiltInCategory.OST_PipeFitting).ToElements();

            FamilySymbol LuvaSelecionada = null;

            // Verifica se a luva selecionada existe no projeto
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
                            if (fcmanager.FamilyName.Equals(JanelaPrincipal.FamiliaLuvaSelecionada))
                            {
                                // Luva encontrada, guarda o simbolo da familia
                                LuvaSelecionada = (FamilySymbol)z;
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

            // Se o simbolo for válido, inicia o processo de colocar as luvas nas tubulações
            if (LuvaSelecionada != null)
            {
                // Inicia as alterações no documento
                Transaction t = new Transaction(Doc.Document, "Adicionar Luva na tubulação");
                t.Start();

                IList<RoutingPreferenceRule> RegrasAnteriores = new List<RoutingPreferenceRule>();

                // Percorre todas as tubulações do projeto
                foreach (Element tb in tubulações)
                {
                    RegrasAnteriores.Clear();

                    // Valida se a tubulação é válida e existe no documento
                    if (tb.IsValidObject && Doc.Document.GetElement(tb.Id) != null)
                    {
                        // Valida se é o tipo é realmente uma tubulação
                        if (tb is Pipe)
                        {
                            Parameter Comprimento = tb.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);

                            // Parâmetro de comprimento existe na tubulação, faz a leitura e compara o comprimento mínimo determinado pelo usuário
                            if (Comprimento != null)
                            {
                                double ValorComprimento = UnitUtils.Convert(Comprimento.AsDouble(), DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_METERS);

                                if (ValorComprimento >= JanelaPrincipal.ValorUsuarioLuva)
                                {
                                    try
                                    {
                                        Parameter Sistemas = tb.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);

                                        // Verifica se o sitema é o selecionado pelo usuário
                                        if (Sistemas != null && Sistemas.AsValueString() != null)
                                        {
                                            if (Sistemas.AsValueString().Equals(JanelaPrincipal.SistemaAlvo))
                                            {
                                                Pipe p = tb as Pipe;

                                                RoutingPreferenceManager routePrefManager = p.PipeType.RoutingPreferenceManager;

                                                // Acessa as preferências de roteamento do tubo
                                                routePrefManager.PreferredJunctionType = PreferredJunctionType.Tee;

                                                // Verifica a quantidade de regras do tipo união existem nas preferências de roteamento
                                                int numeroRegras = routePrefManager.GetNumberOfRules(RoutingPreferenceRuleGroupType.Unions);

                                                // Percorre todas as preferências e salva todas na lista RegrasAnteriores
                                                for (x = 0; x < numeroRegras; x++)
                                                {
                                                    RegrasAnteriores.Add(routePrefManager.GetRule(RoutingPreferenceRuleGroupType.Unions, 0));

                                                    // Remove a preferência de roteamento salva conforme o index passado
                                                    routePrefManager.RemoveRule(RoutingPreferenceRuleGroupType.Unions, 0);
                                                }

                                                // Cria uma nova única regra de roteamento (familia essa que seria a selecionada pelo usuário no programa)
                                                // simbolo = FamilySymbol da familia selecionada pelo usuário
                                                RoutingPreferenceRule newRule = new RoutingPreferenceRule(LuvaSelecionada.Id, "União");
                                                routePrefManager.AddRule(RoutingPreferenceRuleGroupType.Unions, newRule, 0);

                                                LocationCurve p1 = tb.Location as LocationCurve;

                                                var curve = p1.Curve;
                                                var ponto1 = curve.GetEndPoint(0);
                                                var ponto2 = curve.GetEndPoint(1);

                                                XYZ pInicial = ponto2.Subtract(ponto1).Normalize();

                                                Element cn1 = null;

                                                for (double comp = 1; comp <= ValorComprimento; comp++)
                                                {
                                                    if (comp < ValorComprimento)
                                                    {
                                                        XYZ quebra = ponto1.Add(pInicial.Multiply(comp * 3.281));

                                                        // Quebra a tubulação no ponto específico, visto que é necessário dois pontos para colocar a luva
                                                        cn1 = Doc.Document.GetElement(PlumbingUtils.BreakCurve(Doc.Document, tb.Id, quebra));

                                                        // Pega o conector mais distante na tubulação
                                                        var connectors = GetClosestConnector(cn1, tb);

                                                        if (connectors != null && connectors.Count > 1)
                                                        {
                                                            FamilyInstance uniaoCriada = Doc.Document.Create.NewUnionFitting(connectors.First(), connectors.Last());

                                                            if (uniaoCriada != null)
                                                            {
                                                                // Caso precise fazer algo com a união criada, tratar aqui
                                                            }
                                                        }
                                                    }
                                                }

                                                // Volta as alterações no ultimo tubo

                                                // Remove a única regra criada anteriormente
                                                routePrefManager.RemoveRule(RoutingPreferenceRuleGroupType.Unions, 0);

                                                // Agora precisamos adicionar todas as regras existentes novamente para não alterar nada do usuário
                                                for (x = 0; x <= RegrasAnteriores.Count; x++)
                                                {
                                                    routePrefManager.AddRule(RoutingPreferenceRuleGroupType.Unions, RegrasAnteriores[x], x);
                                                }

                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Tratar elementos que não são tubulações aqui
                        }

                    }


                }

                t.Commit();

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
}
