using System;
using GPS.Common;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using HtmlAgilityPack;

namespace GPS.Console
{
    internal class DictionaryGenerator
    {
        private HtmlDocument doc;

        public DictionaryGenerator()
        {
            doc = new HtmlDocument();
        }

        internal void ProcessPage(WikiPage page)
        {
            // Extract the text out of the HTML page for processing.
            doc.Load(page.Content);

            System.Console.WriteLine(page.PrimaryTitle + " " + string.Join(";", page.SecondaryTitles));
        }

        internal void ProcessPage(CompressedWikiPage page)
        {
            using (MemoryStream stream = new MemoryStream(page.Content))
            using (GZipInputStream gzipStream = new GZipInputStream(stream))
            using (StreamReader streamReader = new StreamReader(gzipStream))
            {
                this.ProcessPage(new WikiPage()
                {
                    Id = page.Id,
                    PrimaryTitle = page.PrimaryTitle,
                    SecondaryTitles = page.SecondaryTitles,
                    Content = streamReader.ReadToEnd()
                });
            }
        }
    }
}