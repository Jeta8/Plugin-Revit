using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

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
                catch (Exception) { }
            }
        }

        public string GetName()
        {
            return "Comando Limpeza";
        }
    }
}
