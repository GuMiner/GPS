using GPS.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPS.Common
{
    /// <summary>
    /// Defines methods for searching our <see cref="TreeDictionary"/>
    /// </summary>
    public class TreeSearcher
    {
        private readonly TreeDictionary dictionary;

        public TreeSearcher(TreeDictionary dictionary)
        {
            this.dictionary = dictionary;
        }
    }
}
