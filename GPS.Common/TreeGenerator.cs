using GPS.Common.Data;
using System.Collections.Generic;
using System;

namespace GPS.Common
{
    /// <summary>
    /// Defines methods for generating a <see cref="TreeDictionary"/>
    /// </summary>
    public class TreeGenerator
    {
        public static readonly TreeNodeComparer TreeNodeComparer = new TreeNodeComparer();

        /// <summary>
        /// Adds a new word to the dictionary.
        /// </summary>
        /// <returns>The number of new edges added to the dictionary.</returns>
        public static int AddWord(TreeDictionary dictionary, string word, int occurrences)
        {
            return TreeGenerator.AddWord(dictionary.RootNodes, 0, word, occurrences);
        }

        private static int AddWord(List<TreeNode> nodes, int characterIndex, string word, int occurrences)
        {
            int edgesAdded = 0;

            TreeNode characterNode = new TreeNode() { Character = word[characterIndex] };
            int idx = nodes.BinarySearch(characterNode, TreeGenerator.TreeNodeComparer);
            if (idx < 0)
            {
                // The index is the bitwise complement of the next largest item. Convert that to a location, as we want to add this value *at* that location.
                // By doing so, we never explicitly need to resort the trees -- they'll be generated in that order.
                idx = ~idx;

                characterNode.Count = occurrences;
                characterNode.TerminationCount = 0;
                characterNode.Children = new List<TreeNode>();
                nodes.Insert(idx, characterNode);
                edgesAdded++;
            }

            characterIndex++;
            if (characterIndex != word.Length)
            {
                // Recurse down, adding nodes (each node which adds a single edge) as necessary.
                edgesAdded += TreeGenerator.AddWord(nodes[idx].Children, characterIndex, word, occurrences);
            }
            else
            {
                // This is a terminating word. There should only be a single word that adds to TerminationCount, but just in case...
                characterNode.TerminationCount += occurrences;
            }

            return edgesAdded;
        }

        /// <summary>
        /// Prunes the dictionary of words strings occurring with too minimum a frequency.
        /// </summary>
        /// <returns>The number of edges / nodes pruned</returns>
        public static int Prune(TreeDictionary dictionary, int minFrequency)
        {
            return TreeGenerator.Prune(dictionary.RootNodes, minFrequency);
        }

        private static int Prune(List<TreeNode> nodes, int minFrequency)
        {
            int nodesPruned = 0;
            for (int i = nodes.Count; i >= 0; i++)
            {
                if (nodes[i].Count < minFrequency)
                {
                    nodes.RemoveAt(i);
                    ++nodesPruned;
                }
                else
                {
                    nodesPruned += TreeGenerator.Prune(nodes[i].Children, minFrequency);
                }
            }

            return nodesPruned;
        }
    }
}
