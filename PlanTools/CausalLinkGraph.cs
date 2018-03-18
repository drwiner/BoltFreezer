using BoltFreezer.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class CausalLinkGraph : ICausalLinkGraph
    {
        private List<CausalLink> edges;

        public CausalLinkGraph()
        {
            edges = new List<CausalLink>();
        }

        public CausalLinkGraph(List<CausalLink> _edges)
        {
            edges = _edges;
        }

        public List<CausalLink> Edges
        {
            get
            {
                return edges;
            }

            set
            {
                edges = value;
            }
        }


        public Object Clone()
        {
            var clonedEdges = from edge in edges select edge.Clone() as CausalLink;
            return new CausalLinkGraph(clonedEdges.ToList());
        }

        public void Insert(CausalLink causalLink)
        {
            edges.Add(causalLink);
        }


    }
}