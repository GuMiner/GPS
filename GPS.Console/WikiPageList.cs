using GPS.Common;
using System.Collections.Generic;

namespace GPS.Console
{
    public class WikiPageList
    {
        public WikiPageList()
        {
            this.ValidPages = new List<WikiPage>();
            this.InvalidPages = new List<WikiPage>();
        }

        public List<WikiPage> ValidPages { get; set; }

        /// <summary>
        /// Invalid pages are those where we cannot do an index lookup on (the titles are invalid).
        /// </summary>
        public List<WikiPage> InvalidPages { get; set; }
    }
}
