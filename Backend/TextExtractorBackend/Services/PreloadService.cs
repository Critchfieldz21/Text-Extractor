namespace BackendLibrary
{
    /// <summary>
    /// Class for preloading PDF processing libraries to reduce first-use latency.
    /// </summary>
    public class PreloadService
    {
        /// <summary>
        /// Dummy PDF content.
        /// </summary>
        private static byte[] _tinyPdf => File.ReadAllBytes("Services/tinypdf.pdf");

        /// <summary>
        /// Preloads PDF processing libraries to reduce first-use latency.
        /// </summary>
        /// <param name="logger"></param>
        public static void PreloadPackages(ILogger<PreloadService> logger)
        {
            var jobs = new List<(Action Preload, string label)>
            {
                (() =>  PreloadPdfPig(),            "PdfPig"),
                (() =>  PreloadPdfSharp(),         "PdfSharp"),
            };

            // Only preload PdfiumViewer on Windows
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                jobs.Add((() =>  PreloadPdfiumViewer(),      "PdfiumViewer"));
            }

            Parallel.Invoke(
                jobs.Select(job => (Action)(() =>
                {
                    try
                    {
                        job.Preload();
                        logger.LogDebug("Preloaded {job.label} sucessfully.", job.label);
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug("Failed to load {job.label}: {ex.Message}.", job.label, ex.Message);
                    }
                }))
                .ToArray()
            );
        }

        /// <summary>
        /// Preloads PdfPig's PdfDocument Open, GetPages, and GetWords methods.
        /// </summary>
        private static void PreloadPdfPig()
        {     
            using var pdf = UglyToad.PdfPig.PdfDocument.Open(_tinyPdf);
            _ = pdf.NumberOfPages;
            List<UglyToad.PdfPig.Content.Page> pages = pdf.GetPages().ToList();
            _ = pages[0].GetWords();
        }

        /// <summary>
        /// Preloads PdfiumViewer's PdfDocument Load, PageCount, and Render methods.
        /// </summary>
        private static void PreloadPdfiumViewer()
        {
            // Only available on Windows
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("PdfiumViewer is only supported on Windows");
            }

            var pdfiumViewerType = Type.GetType("PdfiumViewer.PdfDocument, PdfiumViewer");
            if (pdfiumViewerType == null)
            {
                throw new InvalidOperationException("PdfiumViewer assembly not found");
            }

            var loadMethod = pdfiumViewerType.GetMethod("Load", new[] { typeof(Stream) });
            if (loadMethod == null)
            {
                throw new InvalidOperationException("PdfiumViewer Load method not found");
            }

            MemoryStream stream = new MemoryStream(_tinyPdf);
            dynamic? document = loadMethod.Invoke(null, new object[] { stream });
            if (document != null)
            {
                var pageCountProperty = pdfiumViewerType.GetProperty("PageCount");
                _ = pageCountProperty?.GetValue(document);
                
                var renderMethod = pdfiumViewerType.GetMethod("Render", new[] { typeof(int), typeof(float), typeof(float), typeof(bool) });
                _ = renderMethod?.Invoke(document, new object[] { 0, 72f, 72f, true });
            }
        }

        /// <summary>
        /// Preloads PdfSharp's PdfReader Open and PageCount methods.
        /// </summary>
        private static void PreloadPdfSharp()
        {
            using var stream = new MemoryStream(_tinyPdf);
            using var document = PdfSharp.Pdf.IO.PdfReader.Open(stream, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
            _ = document.PageCount;
        }
    }
}
