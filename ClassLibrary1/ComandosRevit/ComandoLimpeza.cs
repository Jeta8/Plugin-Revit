using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
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
                                ComandoTags.TagsDoUnmep.Remove(i);
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
                                TagsConexoes.TagsConexoesDoUnmep.Remove(o);
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
                            TagsAcessorios.TagsAcessoriosDoUnmep.Remove(u);
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
                            TagsPecasHidro.TagsPecasDoUnmep.Remove(r);
                        }
                    }


                    p.Commit();
                    uTag.Clear();
                    ConTag.Clear();
                    AcessTag.Clear();
                    PecasTag.Clear();
                }
                catch (Exception)
                {
                    //uTag.Clear();
                    //ConTag.Clear();
                    //AcessTag.Clear();
                    //PecasTag.Clear();


                    //foreach (ElementId i in uTag)
                    //{
                    //    ComandoTags.TagsDoUnmep.Remove(i);
                    //}

                    //foreach (ElementId o in ConTag)
                    //{
                    //    TagsConexoes.TagsConexoesDoUnmep.Remove(o);
                    //}

                    //foreach (ElementId u in AcessTag)
                    //{
                    //    TagsAcessorios.TagsAcessoriosDoUnmep.Remove(u);
                    //}

                    //foreach (ElementId r in PecasTag)
                    //{
                    //    TagsPecasHidro.TagsPecasDoUnmep.Remove(r);
                    //}
                }
            }
        }

        public string GetName()
        {
            return "Comando Limpeza";
        }
    }
}
