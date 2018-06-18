using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Scheduling
{
    [Serializable]
    public class MergeManager
    {
        public HashSet<Tuple<int, int>> Merges;

        public MergeManager()
        {
            Merges = new HashSet<Tuple<int, int>>();
        }

        public MergeManager(HashSet<Tuple<int, int>> merges)
        {
            Merges = merges;
        }

        public MergeManager(List<Tuple<int, int>> merges)
        {
            Merges = new HashSet<Tuple<int, int>>(merges);
        }

        public void Insert(int parent, int child)
        {
            // Both of these must be already roots, but now one is child.
            Merges.Add(new Tuple<int, int>(parent, child));
        }

        public int FindRoot(int mergeNode)
        {
            foreach (var merge in Merges)
            {
                if (merge.Second == mergeNode)
                {
                    // There's only 1 parent per node
                    return FindRoot(merge.First);
                }
            }

            // It has no parents.
            return mergeNode;
        }

        public Dictionary<int, int> ToRootMap()
        {
            var rootMap = new Dictionary<int, int>();
            foreach(var merge in Merges)
            {
                // map child to root of parent
                rootMap[merge.Second] = FindRoot(merge.First);
            }
            return rootMap;
        }

        public MergeManager Clone()
        {
            var newList = new List<Tuple<int, int>>();
            foreach(var merge in Merges)
            {
                newList.Add(merge);
            }

            return new MergeManager(newList);
        }
    }
}
