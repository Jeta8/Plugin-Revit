using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;

namespace ComandosRevit
{
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

            ICollection<ElementId> idsParaSeremApagadas = new Collection<ElementId>();


            if (uTag != null || ConTag != null || AcessTag != null || PecasTag != null)
            {

                try
                {
                    Transaction p = new Transaction(app.ActiveUIDocument.Document, "Limpar Tag");
                    p.Start();


                    foreach (ElementId i in uTag)
                    {
                        try
                        {
                            app.ActiveUIDocument.Document.Delete(i); 
                        }
                        catch (Exception)
                        {
                            if (i != null)
                            {
                                idsParaSeremApagadas.Add(i);
                                continue;
                            }
                        }
                    }

                    foreach (ElementId o in ConTag)
                    {
                        try
                        {
                            app.ActiveUIDocument.Document.Delete(o);
                        }
                        catch (Exception)
                        {
                            if (o != null)
                            {
                                idsParaSeremApagadas.Add(o);
                                continue;
                            }
                        }
                    }

                    foreach (ElementId u in AcessTag)
                    {
                        try
                        {
                            app.ActiveUIDocument.Document.Delete(u);
                        }
                        catch (Exception)
                        {
                            idsParaSeremApagadas.Add(u);
                            continue;
                        }
                    }

                    foreach (ElementId r in PecasTag)
                    {
                        try
                        {
                            app.ActiveUIDocument.Document.Delete(r);
                        }
                        catch (Exception)
                        {
                            idsParaSeremApagadas.Add(r);
                            continue;
                        }
                    }


                    p.Commit();
                    
                    foreach( ElementId g in idsParaSeremApagadas)
                    {
                        ComandoTags.TagsDoUnmep.Remove(g);
                        TagsAcessorios.TagsAcessoriosDoUnmep.Remove(g);
                        TagsConexoes.TagsConexoesDoUnmep.Remove(g);
                        TagsPecasHidro.TagsPecasDoUnmep.Remove(g);

                    }

                    uTag.Clear();
                    ConTag.Clear();
                    AcessTag.Clear();
                    PecasTag.Clear();
                }
                catch (Exception)
                {          
                }
            }
        }

        public string GetName()
        {
            return "Comando Limpeza";
        }
    }
}
