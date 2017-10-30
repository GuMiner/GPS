using GPS.Common;
using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPS.Console
{
    public class XowaParser
    {
        private const string CoreSuffix = "-core.xowa";
        private const string HtmlSearchTermSuffix = "-html-ns*xowa";

        private string inputFolder;
        private string wikiName;

        public XowaParser(string inputFolder)
        {
            this.inputFolder = inputFolder;
            this.wikiName = Path.GetFileName(this.inputFolder);
        }

        public Dictionary<Int64, PageIndex> ReadPageIndices()
        {
            string coreFilename = Path.Combine(this.inputFolder, $"{this.wikiName}{XowaParser.CoreSuffix}");
            string sql = "SELECT page_id, page_title, page_is_redirect, page_redirect_id FROM page";

            Dictionary<Int64, PageIndex> pageIndices = new Dictionary<Int64, PageIndex>();

            System.Console.WriteLine("Reading page indices...");
            XowaParser.ExecuteSql(coreFilename, sql, (reader) =>
            {
                Int64 pageId = (Int64)reader["page_id"];
                string title = (string)reader["page_title"];
                bool isRedirect = (Int64)reader["page_is_redirect"] == 1;
                Int64 redirectId = (Int64)reader["page_redirect_id"];

                if (isRedirect)
                {
                    PageIndex idx;
                    if (pageIndices.TryGetValue(redirectId, out idx))
                    {
                        idx.SecondaryTitles.Add(title);
                    }
                    else
                    {
                        // New page with secondary title but no primary title
                        pageIndices.Add(redirectId, new PageIndex
                        {
                            Id = redirectId,
                            PrimaryTitle = "<Unset>",
                            SecondaryTitles = new List<string>
                            {
                                title
                            }
                        });
                    }
                }
                else
                {
                    PageIndex idx;
                    if (pageIndices.TryGetValue(pageId, out idx))
                    {
                        // A redirect added the main title for us.
                        idx.PrimaryTitle = title;
                    }
                    else
                    {
                        // New page with primary title and no secondary titles
                        pageIndices.Add(pageId, new PageIndex
                        {
                            Id = pageId,
                            PrimaryTitle = title,
                            SecondaryTitles = new List<string>()
                        });
                    }
                }
            });
            System.Console.WriteLine($"Read {pageIndices.Count} unique page indices.");

            return pageIndices;
        }

        public List<string> GetHtmlDbFileNames()
            => Directory.GetFiles(this.inputFolder, $"{this.wikiName}{XowaParser.HtmlSearchTermSuffix}", SearchOption.TopDirectoryOnly).ToList();

        public WikiPageList ReadHtmlDbFile(string filename, Dictionary<Int64, PageIndex> pageIndices)
        {
            string sql = "SELECT page_id, body FROM html";

            WikiPageList pages = new WikiPageList();
            System.Console.WriteLine($"Reading HTML files from {Path.GetFileName(filename)}...");
            XowaParser.ExecuteSql(filename, sql, (reader) =>
            {
                Int64 pageId = (Int64)reader["page_id"];
                byte[] body = (byte[])reader["body"];

                WikiPage page = new WikiPage()
                {
                    Id = pageId
                };

                // Decode body (GZip'd data)
                using (MemoryStream stream = new MemoryStream(body))
                using (GZipInputStream gzipStream = new GZipInputStream(stream))
                using (StreamReader streamReader = new StreamReader(gzipStream))
                {
                    page.Content = streamReader.ReadToEnd();
                }

                // Associated and save
                PageIndex idx = null;
                if (pageIndices.TryGetValue(pageId, out idx))
                {
                    page.PrimaryTitle = idx.PrimaryTitle;
                    page.SecondaryTitles = idx.SecondaryTitles;
                    pages.ValidPages.Add(page);
                }
                else
                {
                    page.PrimaryTitle = "<Unknown>";
                    page.SecondaryTitles = new List<string>();
                    pages.InvalidPages.Add(page);
                }
            });

            System.Console.WriteLine($"Read {pages.ValidPages.Count} total valid pages and {pages.InvalidPages.Count} invalid pages.");
            return pages;
        }

        /// <summary>
        /// TODO: Deduplicate
        /// </summary>
        public CompressedWikiPageList ReadCompressedHtmlDbFile(string filename, Dictionary<Int64, PageIndex> pageIndices)
        {
            string sql = "SELECT page_id, body FROM html";

            CompressedWikiPageList pages = new CompressedWikiPageList();
            System.Console.WriteLine($"Reading HTML files from {Path.GetFileName(filename)}...");
            XowaParser.ExecuteSql(filename, sql, (reader) =>
            {
                Int64 pageId = (Int64)reader["page_id"];
                byte[] body = (byte[])reader["body"];

                CompressedWikiPage page = new CompressedWikiPage()
                {
                    Id = pageId,
                    Content = body
                };

                // Associated and save
                PageIndex idx = null;
                if (pageIndices.TryGetValue(pageId, out idx))
                {
                    page.PrimaryTitle = idx.PrimaryTitle;
                    page.SecondaryTitles = idx.SecondaryTitles;
                    pages.ValidPages.Add(page);
                }
                else
                {
                    page.PrimaryTitle = "<Unknown>";
                    page.SecondaryTitles = new List<string>();
                    pages.InvalidPages.Add(page);
                }
            });

            System.Console.WriteLine($"Read {pages.ValidPages.Count} total valid pages and {pages.InvalidPages.Count} invalid pages.");
            return pages;
        }

        public static void ExecuteSql(string fileName, string sql, Action<SQLiteDataReader> readAction)
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={fileName};Version=3;"))
            {
                conn.Open();

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            readAction(reader);

                            ++count;
                            if (count % 1000 == 0)
                            {
                                System.Console.WriteLine($"  Read {count} rows...");
                            }
                        }
                    }
                }
            }
        }
    }
}
