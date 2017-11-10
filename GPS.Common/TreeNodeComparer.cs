using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPS.Common.Data
{
    /// <summary>
    /// Defines how to compare tree nodes for sorting.
    /// </summary>
    public class TreeNodeComparer : IComparer<TreeNode>
    {
        public int Compare(TreeNode x, TreeNode y)
        {
            return x.Character.CompareTo(y.Character);
        }
    }
}
