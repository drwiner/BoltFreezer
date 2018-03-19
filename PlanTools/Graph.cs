using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace BoltFreezer.PlanTools
{
    
    public class Graph<T>
    {
        private List<T> nodes;
        private List<Tuple<T, T>> edges;

        public Graph()
        {
            nodes = new List<T>();
            edges = new List<Tuple<T, T>>();
        }

        public Graph(List<T> _nodes,  List<Tuple<T,T>> _edges)
        {
            nodes = _nodes;
            edges = _edges;
        }

        public void AddEdge(Tuple<T,T> edge)
        {
            if (!nodes.Contains(edge.First))
                nodes.Add(edge.First);
            if (!nodes.Contains(edge.Second))
                nodes.Add(edge.Second);
            edges.Add(edge);
        }

        public void Insert(T elm1, T elm2)
        {
            if (!nodes.Contains(elm1))
                nodes.Add(elm1);
            if (!nodes.Contains(elm2))
                nodes.Add(elm2);
            edges.Add(new Tuple<T,T>(elm1, elm2));
        }

        public List<T> GetDescendants(T element)
        {
            if (!nodes.Contains(element))
            {
                throw new System.Exception();
            }

            var descendants = new List<T>();
            var unexplored = new Stack<T>();
            unexplored.Push(element);
            
            while(unexplored.Count > 0)
            {
                var elm = unexplored.Pop();
                var tails = edges.FindAll(edge => edge.First.Equals(elm)).Select(edge => edge.Second);
                foreach (var tail in tails)
                {
                    if (!descendants.Contains(tail))
                    {
                        unexplored.Push(tail);
                        descendants.Add(tail);
                    }

                }
            }
            return descendants;
        }

        public bool IsPath(T elm1, T elm2)
        {
            if(!nodes.Contains(elm1) || !nodes.Contains(elm2))
            {
                throw new System.Exception();
            }

            var desc = GetDescendants(elm1);
            if (desc.Contains(elm2))
            {
                return true;
            }
            return false;
        }

        public bool HasCycle()
        {
            foreach (var elm in nodes)
            {
                var descendants = GetDescendants(elm);
                var predecessors = edges.FindAll(edge => edge.Second.Equals(elm)).Select(edge => edge.First);
                foreach (var desc in descendants)
                {
                    if (predecessors.Contains(desc))
                        return true;
                }
            }
            return false;
        }

        public List<T> TopoSort()
        {
            
            List<T> sortedList = new List<T>();
            foreach (var node in nodes)
            {
                var insertionPoint = sortedList.Count;
                
                for (int i=0; i < sortedList.Count; i++)
                {
                    if (sortedList[i].Equals(node))
                    {
                        continue;
                    }
                    if (IsPath(node, sortedList[i])){
                        if (i < insertionPoint)
                        {
                            insertionPoint = i;
                        }
                    }
                }

                if (insertionPoint == sortedList.Count)
                {
                    sortedList.Add(node);
                }
                else
                    sortedList.Insert(insertionPoint, node);
                
            }

            return sortedList;
        }
        

        /// <summary>
        /// Clones just the graph, and not the members. The members will never be mutated.
        /// </summary>
        /// <returns> new Graph<typeparamref name="T"/> (nodes, edges) </returns>
        public Object Clone()
        {
            return new Graph<T>(nodes, edges);
        }

    }



}