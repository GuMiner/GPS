using System;
using GPS.Common;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text;

namespace GPS.Console
{
    internal class DictionaryGenerator
    {
        private HtmlDocument doc;

        public DictionaryGenerator()
        {
            doc = new HtmlDocument();
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

        internal void ProcessPage(WikiPage page)
        {
            // Extract the text out of the HTML page for processing.
            doc.LoadHtml(page.Content);

            // Extracts links and text, HTML delimiters and punctuation included.
            this.ExtractLinksAndText(doc.DocumentNode.InnerText,
                (linkPrefix, linkParts) =>
                {
                    System.Console.WriteLine(linkPrefix + " " + string.Join(" - ", linkParts));
                },
                (word) =>
                {
                    System.Console.WriteLine(word);
                });

            System.Console.WriteLine(page.PrimaryTitle ?? "<Invalid Title>" + " " + string.Join(";", page.SecondaryTitles ?? new List<string>() { "<null>"}));
        }

        public delegate TResult FuncWithOut<T1, T2, T3, TResult>(T1 obj, T2 obj2, out T3 obj3);

        private void ExtractLinksAndText(string text, Action<string, List<string>> linkProcessor, Action<string> wordProcessor)
        {
            List<string> skipPrefixes = new List<string>()
                { "#T" }; // Media types that don't work well
            List<string> singleBodyLinkPrefixes = new List<string>()
                { "$a'", "$a1", "$e'", "%QA", "$!", "$%", "\"#", "\"$", "#*", "\"%", "#!", "\"&" };
            List<string> dualBodyLinkPrefixes = new List<string>()
                { "${$", "${*", "$\"", "$c'", "$b'", "$b\"", "$d1", "%Q1", "${'#", "%R4", "\".", "\"-",
                  "$b1", "$b/", "$b)", "$d'", "$$", "$#", "#'", "$h'", "\"+", ")!", "$'", "\",", "$B", "&-#N", "&-#?", "&-\"g" };
            List<string> tripleBodyLinkPrefixes = new List<string>()
                { "${'$", "${'T", "${'d'", "%]5" };
            List<string> tripleWithSuffixNewlineHandling = new List<string>()
                { "&%", "&-#K" };

            bool inWord = false;
            StringBuilder wordBuilder = new StringBuilder();

            StringBuilder[] linkBuilders = new StringBuilder[] { new StringBuilder(), new StringBuilder(), new StringBuilder() };
            string linkPrefix = null;
            bool inLink = false;
            int linkSize = -1;
            bool considerNewlineToTerminateLink = false;

            FuncWithOut<int, List<string>, string, bool> hasSubstring = (int idx, List<string> prefixes, out string prefix) =>
            {
                prefix = null;
                foreach (string testPrefix in prefixes)
                {
                    bool isMatch = true;
                    for (int i = 0; i < testPrefix.Length && isMatch; i++)
                    {
                        int textIdx = i + 1 + idx;
                        if (textIdx < text.Length)
                        {
                            if (testPrefix[i] != text[textIdx])
                            {
                                // Character mismatch
                                isMatch = false;
                            }
                        }
                        else
                        {
                            // Can't match this prefix, it's OOB
                            isMatch = false;
                        }
                    }

                    if (isMatch)
                    {
                        prefix = testPrefix;
                        return true;
                    }
                }

                return false;
            };

            Func<int, int> handleLinkDelimiter = (int idx) =>
            {
                if (inLink)
                {
                    --linkSize;
                    if (linkSize == 0)
                    {
                        inLink = false;
                        considerNewlineToTerminateLink = false;
                        List<string> linkParts = new List<string>();
                        for (int i = linkBuilders.Length - 1; i >= 0; i--)
                        {
                            if (linkBuilders[i].Length != 0)
                            {
                                linkParts.Add(linkBuilders[i].ToString());
                                linkBuilders[i].Clear();
                            }
                        }
                        
                        linkProcessor(linkPrefix, linkParts);
                    }

                    return idx;
                }
                else
                {
                    inLink = true;
                    
                    if (hasSubstring(idx, tripleWithSuffixNewlineHandling, out linkPrefix))
                    {
                        linkSize = 3;
                        considerNewlineToTerminateLink = true; // Only applies to the last part of the triple.
                    }
                    else if(hasSubstring(idx, tripleBodyLinkPrefixes, out linkPrefix))
                    {
                        linkSize = 3;
                    }
                    else if (hasSubstring(idx, dualBodyLinkPrefixes, out linkPrefix))
                    {
                        linkSize = 2;
                    }
                    else if (hasSubstring(idx, singleBodyLinkPrefixes, out linkPrefix))
                    {
                        linkSize = 1;
                    }
                    else if (hasSubstring(idx, skipPrefixes, out linkPrefix))
                    {
                        linkSize = 0;
                        inLink = false;
                        linkPrefix = string.Empty;
                    }
                    else
                    {
                        System.Console.WriteLine($"Unknown prefix in text. Idx: {idx}. Text: {text.Substring(idx, 10)}");
                        inLink = false;
                        return idx;
                    }

                    return idx + linkPrefix.Length;
                }
            };

            Action<char> continueOrStartWord = (char character) =>
            {
                if (inWord)
                {
                    // Continue new word
                    wordBuilder.Append(character);
                }
                else
                {
                    if (inLink)
                    {
                        linkBuilders[linkSize - 1].Append(character);
                    }
                    else
                    {
                        // Start new word
                        inWord = true;
                        wordBuilder.Append(character);
                    }
                }
            };

            Action endWord = () =>
            {
                if (inWord)
                {
                    inWord = false;
                    string word = wordBuilder.ToString();
                    if (word.Length != 0)
                    {
                        wordProcessor(word);
                    }

                    wordBuilder.Clear();
                }
            };

            Func<char,int, int> endWordOrContinueLink = (char character, int idx) =>
            {
                endWord();
                if (inLink)
                {
                    if (considerNewlineToTerminateLink && linkSize == 1 && character == '\n')
                    {
                        return handleLinkDelimiter(idx);
                    }
                    else
                    {
                        linkBuilders[linkSize - 1].Append(character);
                    }
                }

                return idx;
            };

            for (int i = 0; i < text.Length; i++)
            {
                switch(text[i])
                {
                    case '': // ESC.
                        endWord();
                        i = handleLinkDelimiter(i);
                        break;
                    case '\r':
                    case '\n':
                    case ' ':
                    case '\t':
                        i = endWordOrContinueLink(text[i], i);
                        break;
                    default:
                        continueOrStartWord(text[i]);
                        break;
                }
            }

            endWord();
            while (inLink)
            {
                handleLinkDelimiter(text.Length);
            }
        }
    }
}