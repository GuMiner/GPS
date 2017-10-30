using GPS.Common;
using System.Collections.Generic;

namespace GPS.Console
{
    public class CompressedWikiPageList
    {
        public CompressedWikiPageList()
        {
            this.ValidPages = new List<CompressedWikiPage>();
            this.InvalidPages = new List<CompressedWikiPage>();
        }

        public List<CompressedWikiPage> ValidPages { get; set; }

        /// <summary>
        /// Invalid pages are those where we cannot do an index lookup on (the titles are invalid).
        /// </summary>
        public List<CompressedWikiPage> InvalidPages { get; set; }
    }
}
