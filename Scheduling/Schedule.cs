using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BoltFreezer.Scheduling
{
    [Serializable]
    public class Schedule : Graph<IPlanStep>
    {

        public Schedule() : base()
        {

        }
        public Schedule(HashSet<IPlanStep> nodes, HashSet<Tuple<IPlanStep, IPlanStep>> edges) : base(nodes, edges, new Dictionary<IPlanStep, HashSet<IPlanStep>>())
        {
        }

        public Schedule(HashSet<IPlanStep> nodes, HashSet<Tuple<IPlanStep, IPlanStep>> edges, HashSet<Tuple<IPlanStep, IPlanStep>> cntgs) : base(nodes, edges, new Dictionary<IPlanStep, HashSet<IPlanStep>>())
        {
        }

        public Schedule(HashSet<Tuple<IPlanStep, IPlanStep>> cntgs) : base(new HashSet<IPlanStep>(), cntgs, new Dictionary<IPlanStep, HashSet<IPlanStep>>())
        {
        }

        public Schedule(List<Tuple<IPlanStep, IPlanStep>> cntgs) : base(new HashSet<IPlanStep>(), new HashSet<Tuple<IPlanStep, IPlanStep>>(cntgs), new Dictionary<IPlanStep, HashSet<IPlanStep>>())
        {
        }


        public bool HasFault(Graph<IPlanStep> orderings)
        {
            /* PYTHON CODE: 
             * 
             * def cntg_consistent(self):
            cntg_edges = [edge for edge in self.edges if edge.label == "cntg"]
            sources = []
            sinks = []
            # cntg must be s -> t -> u and cannot exist another s -> s' or t' -> u
            for edge in cntg_edges:
                if edge.sink in sinks:
                    return False
                if edge.source in sources:
                    return False
                sources.append(edge.source)
                sinks.append(edge.sink)
                # cannot be an ordering between cntg edge e.g. s --cntg--> t and s < u < t
                ordering_edges = [ord for ord in self.edges if ord.source == edge.source and ord.sink != edge.sink and ord.label == "<"]
                for ord in ordering_edges:
                    if self.isPath(ord.sink, edge.sink):
                        return False
                ordering_edges = [ord for ord in self.edges if ord.sink == edge.sink and ord.source != edge.source and ord.label == "<"]
                for ord in ordering_edges:
                    if self.isPath(edge.source, ord.source):
                        return False

            return True
            */
            List<IPlanStep> sources = new List<IPlanStep>();
            List<IPlanStep> sinks = new List<IPlanStep>();
            foreach (var edge in edges)
            {
                // base cases
                if (sinks.Contains(edge.Second))
                {
                    return true;
                }
                if (sources.Contains(edge.First))
                {
                    return true;
                }

                sources.Add(edge.First);
                sinks.Add(edge.Second);

                foreach (var ordering in orderings.edges)
                {
                   

                    if (ordering.First.Equals(edge.First) && !ordering.Second.Equals(edge.Second))
                    {
                        if (ordering.Second.Name.Equals("DummyGoal") || ordering.Second.Name.Equals("DummyInit") || ordering.Second.Height > 0)
                        {
                            continue;
                        }

                        // There cannot be a path from the ordering to the tail of the cntg edge
                        if (orderings.IsPath(ordering.Second, edge.Second))
                        {
                            return true;
                        }
                    }

                    if (ordering.Second.Equals(edge.Second) && !ordering.First.Equals(edge.First))
                    {
                        if (ordering.First.Name.Equals("DummyGoal") || ordering.First.Name.Equals("DummyInit") || ordering.First.Height > 0)
                        {
                            continue;
                        }

                        // There cannot be a path from the head of cntg edge to the head of the ordering
                        if (orderings.IsPath(edge.First, ordering.First))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}

