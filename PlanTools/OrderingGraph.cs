using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class OrderingGraph : IOrderingGraph
{
    private List<Tuple<IOperator, IOperator>> edges;

    public OrderingGraph()
    {
        edges = new List<Tuple<IOperator,IOperator>>();
    }

    public OrderingGraph(List<Tuple<IOperator,IOperator>> _edges)
    {
        edges = _edges;
    }

    public List<Tuple<IOperator, IOperator>> Edges
    {
        get{return edges; }
        set {  edges = value; }
    }

    public Object Clone()
    {
        var clonedEdges = from edge in edges select edge.Clone() as Tuple<IOperator, IOperator>;
        return new OrderingGraph(clonedEdges.ToList());
    }

    public bool HasCycle()
    {
        throw new NotImplementedException();
    }

    public void Insert(Tuple<IOperator, IOperator> ordering)
    {
        edges.Add(ordering);
    }


}