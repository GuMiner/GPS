using System;
using GPS.Common;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace GPS.Console
{
    internal class DictionaryGenerator
    {
        private object lockObject = new object();

        public DictionaryGenerator()
        {
            this.Dictionary = new WikiDictionary()
            {
                TitleMap = new Dictionary<long, string>(),
                WordMap = new Dictionary<string, HashSet<long>>(StringComparer.InvariantCultureIgnoreCase),
                WordFrequencies = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
            };
        }

        public WikiDictionary Dictionary { get; private set; }

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

        internal void ProcessPage(WikiPage page)
        {
            // Extract the text out of the HTML page for processing.
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(page.Content);

            // Extract a line of text with a control character
            Dictionary<string, int> cleanWords = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            this.ExtractBlocks(doc.DocumentNode.InnerText,
                (blockId, word) =>
                {
                    string cleanWord = WebUtility.HtmlDecode(WebUtility.UrlDecode(word)).ToLowerInvariant();
                    for (int i = 0; i < cleanWord.Length; i++)
                    {
                        // Skip if we don't consider this a valid word
                        if (!this.IsCharValid(cleanWord[i]))
                        {
                            return;
                        }
                    }

                    cleanWord = StripWellKnownPunctuation(cleanWord);
                    if (!string.IsNullOrWhiteSpace(cleanWord))
                    {
                        int wordCount;
                        if (!cleanWords.TryGetValue(cleanWord, out wordCount))
                        {
                            wordCount = 0;
                            cleanWords.Add(cleanWord, wordCount);
                        }

                        cleanWords[cleanWord]++;
                    }
                });

            // Wait for all words to finish saving.
            string compositeTitle = page.PrimaryTitle?.Replace('_', ' ') ?? "<Invalid Title>" + " " + string.Join(";", page.SecondaryTitles ?? new List<string>() { "<null>" });
            this.SaveWord(page.Id, compositeTitle, cleanWords);
        }

        private void SaveWord(long pageId, string compositeTitle, Dictionary<string, int> cleanWords)
        {
            lock (lockObject)
            {
                this.Dictionary.TitleMap.Add(pageId, compositeTitle);

                foreach (KeyValuePair<string, int> cleanWord in cleanWords)
                {
                    HashSet<long> referenceList;
                    if (!this.Dictionary.WordMap.TryGetValue(cleanWord.Key, out referenceList))
                    {
                        referenceList = new HashSet<long>();
                        this.Dictionary.WordMap.Add(cleanWord.Key, referenceList);
                    }

                    referenceList.Add(pageId);

                    int frequency;
                    if (!this.Dictionary.WordFrequencies.TryGetValue(cleanWord.Key, out frequency))
                    {
                        frequency = 0;
                        this.Dictionary.WordFrequencies.Add(cleanWord.Key, frequency);
                    }

                    this.Dictionary.WordFrequencies[cleanWord.Key] += cleanWord.Value;
                }
            }
        }

        private string StripWellKnownPunctuation(string word)
        {
            HashSet<char> wellKnownPunctuation = new HashSet<char>
            {
                ',', '.', ':', ';', '"', '(', ')', '?' // Notably, we allow ' and - and # but disallow "
            };

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < word.Length; i++)
            {
                if (!wellKnownPunctuation.Contains(word[i]))
                {
                    builder.Append(word[i]);
                }
            }

            return builder.ToString();
        }

        private bool IsCharValid(char c)
        {
            // We alreday have lowercased everyyhing
            if ((c >= 48 && c <= 57) || (c >= 97 && c <= 122)) // a-z0-9
            {
                return true;
            }

            HashSet<char> validChars = new HashSet<char>()
            {
                ',', '.', ':', ';', '#','\'', '"', '(', ')', '-', '?' // Notably, we omit $
            };

            return validChars.Contains(c);
        }
        
        private void ExtractBlocks(string text, Action<int, string> blockWordAction)
        {
            int blockId = 0;
            StringBuilder wordBuilder = new StringBuilder();
            
            for (int i = 0; i < text.Length; i++)
            {
                switch(text[i])
                {
                    case '': // ESC.
                        if (wordBuilder.Length != 0)
                        {
                            blockWordAction(blockId, wordBuilder.ToString());
                            wordBuilder.Clear();
                        }

                        ++blockId;
                        break;
                    case '\r':
                    case '\n':
                    case ' ':
                    case '\t':
                        if (wordBuilder.Length != 0)
                        {
                            blockWordAction(blockId, wordBuilder.ToString());
                            wordBuilder.Clear();
                        }
                        break;
                    default:
                        wordBuilder.Append(text[i]);
                        break;
                }
            }
        }
    }
}