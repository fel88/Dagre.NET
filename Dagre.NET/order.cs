using System;
using System.Collections.Generic;
using System.Linq;


namespace Dagre
{
    public class order
    {

        /*
         * Applies heuristics to minimize edge crossings in the graph and sets the best
         * order solution as an order attribute on each node.
         *
         * Pre-conditions:
         *
         *    1. Graph must be DAG
         *    2. Graph nodes must be objects with a "rank" attribute
         *    3. Graph edges must have the "weight" attribute
         *
         * Post-conditions:
         *
         *    1. Graph nodes will have an "order" attribute based on the results of the
         *       algorithm.
         */
        /*public static DagreGraph[] buildLayerGraphs(DagreGraph g, int[] ranks, string relationship)
        {
            return ranks.Select(z => buildLayerGraph(g, z, relationship)).ToArray();
        }*/


        public static void _order(DagreGraph g, Action<float> progress = null)
        {
            List<DagreGraph> downLayerGraphs = new List<DagreGraph>();
            List<DagreGraph> upLayerGraphs = new List<DagreGraph>();
            var rank = util.maxRank(g);

            for (int i = 0; i < rank; i++)
            {
                downLayerGraphs.Add(buildLayerGraph(g, i + 1, "inEdges"));
                upLayerGraphs.Add(buildLayerGraph(g, rank - i - 1, "outEdges"));
                progress?.Invoke((float)i / rank);
            }



            dynamic layering = initOrder(g) as object[][];
            assignOrder(g, layering);
            

            //var or = DagreGraph.FromJson(DagreTester.ReadResourceTxt("afterAssignOrder"));
            // or.Compare(g);
            dynamic bestCC = float.PositiveInfinity;
            object[] best = null;

            var t1 = g.node("0");
            foreach (var item in upLayerGraphs)
            {
                var t2 = item.node("0");
                if (t1 == t2)
                {

                }
            }
            for (int i = 0, lastBest = 0; lastBest < 4; ++i, ++lastBest)
            {
                sweepLayerGraphs((i % 2 != 0) ? downLayerGraphs.ToArray() : upLayerGraphs.ToArray(), i % 4 >= 2);
                
                layering = util.buildLayerMatrix(g);
                var cc = crossCount(g, layering);
                if (cc < bestCC)
                {
                    lastBest = 0;
                    var length = layering.Count;
                    best = new object[length];
                    for (var j = 0; j < length; j++)
                    {
                        // best[i] = layering[i].slice()
                        //clone here
                        List<object> cln = new List<object>();
                        foreach (var item in layering[j].Values)
                        {
                            cln.Add(item);
                        }
                        best[j] = cln;
                    }
                    bestCC = cc;
                    /////
                    //lastBest = 0;
                    //best = util.cloneDeep(layering);
                    // bestCC = cc;
                }
            }
            
            assignOrder(g, best);

        }

        public static string[] reorderKeys(string[] ret)
        {
           
            var dgts = ret.Where(z => z.All(char.IsDigit)).ToArray();
            Array.Sort(dgts, (x, y) => int.Parse(x) - int.Parse(y));
            var remains = ret.Except(dgts).ToArray();
            Array.Sort(remains, (x, y) => string.CompareOrdinal(x, y));
            ret = dgts.Union(remains).ToArray();
            return ret;
        }

        public static void sweepLayerGraphs(DagreGraph[] layerGraphs, bool biasRight)
        {
            var cg = new DagreGraph(false);
            foreach (var lg in layerGraphs)
            {
                var root = lg.graph()["root"];
                var sorted = sortSubGraphModule.sortSubraph(lg as DagreGraph, root, cg, biasRight);
                var vs = sorted["vs"] as List<object>;
              /*  vs=reorderKeys(vs.Cast<string>().ToArray()).Cast<object>().ToList();
                sorted["vs"] = vs;*/
                var length = vs.Count;
                for (var i = 0; i < length; i++)
                {
                    var vv = vs[i];
                    if (vv == "0")
                    {

                    }
                    (lg as DagreGraph).node(vv)["order"] = i;
                }
                addSubgraphConstraints(lg, cg, sorted["vs"]);
            }

        }
        public static void addSubgraphConstraints(DagreGraph g, DagreGraph cg, dynamic vs)
        {
            dynamic prev = new JavaScriptLikeObject();
            object rootPrev = null;
            foreach (dynamic v in vs)
            {
                var child = g.parent(v);
                object parent = null;
                object prevChild = null;
                while (child != null)
                {
                    parent = g.parent(child);
                    if (parent != null)
                    {
                        prevChild = prev[parent];
                        prev[parent] = child;
                    }
                    else
                    {
                        prevChild = rootPrev;
                        rootPrev = child;
                    }
                    if (prevChild != null && prevChild != child)
                    {
                        cg.setEdge(new object[] { prevChild, child });
                        return;
                    }
                    child = parent;
                }
            }
        }
        public static void assignOrder(DagreGraph g, dynamic layering)
        {
            foreach (var layer in layering)
            {
                int len = 0;
                if (layer is Array) len = layer.Length;
                else len = layer.Count;
                for (int i = 0; i < len; i++)
                {
                    var v = layer[i];
                    g.node(v)["order"] = i;
                }


            }
            //_.forEach(layering, function(layer) {
            //    _.forEach(layer, function(v, i) {
            //        g.node(v).order = i;
            //    });
            //});
        }

        #region buildLayerGraph

        /*
         * Constructs a graph that can be used to sort a layer of nodes. The graph will
         * contain all base and subgraph nodes from the request layer in their original
         * hierarchy and any edges that are incident on these nodes and are of the type
         * requested by the "relationship" parameter.
         *
         * Nodes from the requested rank that do not have parents are assigned a root
         * node in the output graph, which is set in the root graph attribute. This
         * makes it easy to walk the hierarchy of movable nodes during ordering.
         *
         * Pre-conditions:
         *
         *    1. Input graph is a DAG
         *    2. Base nodes in the input graph have a rank attribute
         *    3. Subgraph nodes in the input graph has minRank and maxRank attributes
         *    4. Edges have an assigned weight
         *
         * Post-conditions:
         *
         *    1. Output graph has all nodes in the movable rank with preserved
         *       hierarchy.
         *    2. Root nodes in the movable layer are made children of the node
         *       indicated by the root attribute of the graph.
         *    3. Non-movable nodes incident on movable nodes, selected by the
         *       relationship parameter, are included in the graph (without hierarchy).
         *    4. Edges incident on movable nodes, selected by the relationship
         *       parameter, are added to the output graph.
         *    5. The weights for copied edges are aggregated as need, since the output
         *       graph is not a multi-graph.
         */
        public static DagreGraph buildLayerGraph(DagreGraph g, int rank, string relationship)
        {
            object root;
            while (true)
            {
                root = util.uniqueId("_root");
                if (!g.hasNode((string)root)) break;
            }
            var graph = new DagreGraph(true) { _isCompound = true };
            JavaScriptLikeObject jo = new JavaScriptLikeObject();
            jo.Add("root", root);
            graph.setGraph(jo);
            graph.setDefaultNodeLabel((v) => g.node(v));
            foreach (var v in g.nodes())
            {
                var node = g.node(v);
                if (node["rank"] == rank || (node.ContainsKey("minRank") && node.ContainsKey("maxRank") && node["minRank"] <= rank && rank <= node["maxRank"]))
                {
                    graph.setNode(v);
                    var parent = g.parent(v);
                    graph.setParent2(v, parent != null ? parent : root);
                    // This assumes we have only short edges!
                    object[] rr = null;
                    if (relationship == "inEdges")
                    {
                        rr = g.inEdges(v);
                    }
                    else if (relationship == "outEdges")
                    {
                        rr = g.outEdges(v);
                    }
                    else
                    {
                        throw new DagreException();
                    }
                    foreach (dynamic e in rr)
                    {
                        var u = e["v"] == v ? e["w"] : e["v"];
                        var edge = graph.edgeRaw(new object[] { u, v });
                        var weight = edge != null ? edge["weight"] : 0;
                        JavaScriptLikeObject j = new JavaScriptLikeObject();
                        j.Add("weight", g.edge(e)["weight"] + weight);
                        graph.setEdge(new object[] { u, v, j });
                    }
                    if (node.ContainsKey("minRank"))
                    {
                        var jj = new JavaScriptLikeObject();
                        jj.Add("borderLeft", node["borderLeft"][rank]);
                        jj.Add("borderRight", node["borderRight"][rank]);

                        graph.setNode(v, jj);
                    }
                }
            }
            return graph;
        }

        public static void createRootNode(DagreGraph g)
        {
            //util.uniqueId("_root")
            /*   var v;
               while (g.hasNode((v = _.uniqueId("_root")))) ;
               return v;*/
        }
        #endregion

        #region init order
        /*
        * Assigns an initial order value for each node by performing a DFS search
        * starting from nodes in the first rank. Nodes are assigned an order in their
        * rank as they are first visited.
        *
        * This approach comes from Gansner, et al., "A Technique for Drawing Directed
        * Graphs."
        *
        * Returns a layering matrix with an array per layer and each layer sorted by
        * the order of its nodes.
        */
        public static object initOrder(DagreGraph g)
        {
            List<List<string>> ret = new List<List<string>>();
            JavaScriptLikeObject visited = new JavaScriptLikeObject();
            var nodes = g.nodes().Where((v) => g.children(v).Length == 0).ToArray();
            dynamic maxRank = null;
            foreach (dynamic v in nodes)
            {
                var temp1 = g.children((string)v);
                if (!(temp1.Length > 0))
                {
                    var rank = g.node(v)["rank"];
                    if (maxRank == null || (rank != null && rank > maxRank))
                    {
                        maxRank = rank;
                    }
                }
            }
            if (maxRank != null)
            {
                List<List<object>> layers = new List<List<object>>();
                for (int i = 0; i < maxRank + 1; i++)
                {
                    layers.Add(new List<object>());
                }
                //var layers = Array.from(new Array(maxRank + 1), () => []);
                var temp1 = nodes.Select((v) => new object[] { g.node(v)["rank"], v }).ToArray();
                Array.Sort(temp1, (a, b) =>
                {
                    dynamic aa = a[0];
                    dynamic bb = b[0];
                    return aa - bb;
                });
                var temp2 = temp1.Select(z => z[1]).ToArray();
                foreach (var v in temp2)
                {
                    List<object> queue = new List<object>();
                    queue.Add(v);

                    while (queue.Count > 0)
                    {
                        var v2 = queue[0];
                        queue.RemoveAt(0);
                        if (!visited.ContainsKey((string)v2))
                        {
                            visited.Add((string)v2, true);
                            var rank = g.node(v2)["rank"];
                            layers[rank].Add(v2);
                            queue.AddRange(g.successors((string)v2));
                        }
                    }
                }

                return layers.Select(z => z.ToArray()).ToArray();
            }
            return new JavaScriptLikeObject();
        }

        #endregion

        #region cross-count

        /*
         * A function that takes a layering (an array of layers, each with an array of
         * ordererd nodes) and a graph and returns a weighted crossing count.
         *
         * Pre-conditions:
         *
         *    1. Input graph must be simple (not a multigraph), directed, and include
         *       only simple edges.
         *    2. Edges in the input graph must have assigned weights.
         *
         * Post-conditions:
         *
         *    1. The graph and layering matrix are left unchanged.
         *
         * This algorithm is derived from Barth, et al., "Bilayer Cross Counting."
         */
        public static int crossCount(DagreGraph g, dynamic layering)
        {
            var cc = 0;
            for (var i = 1; i < layering.Count; ++i)
            {
                cc += twoLayerCrossCount(g, layering[i - 1], layering[i]);
            }
            return cc;
        }

        public static int twoLayerCrossCount(DagreGraph g, dynamic northLayer, dynamic southLayer)
        {
            // Sort all of the edges between the north and south layers by their position
            // in the north layer and then the south. Map these edges to the position of
            // their head in the south layer.
            JavaScriptLikeObject southPos = new JavaScriptLikeObject();
            for (var i = 0; i < southLayer.Count; i++)
            {
                southPos[(string)southLayer["" + i]] = i;
            }
            //const southEntries = northLayer.map((v) => g.outEdges(v)
            //.map((e) => {return { pos: southPos[e.w], weight: g.edge(e).weight }; })
            //.sort((a, b) => a.pos - b.pos)).flat();
            List<object> southEntries = new List<object>();
            foreach (dynamic item in northLayer)
            {
                var temp1 = g.outEdges(item.Value);
                List<object> ret2 = new List<object>();
                foreach (var e in temp1)
                {
                    JavaScriptLikeObject ss = new JavaScriptLikeObject();
                    ss.Add("pos", southPos[e["w"]]);
                    ss.Add("weight", g.edge(e)["weight"]);
                    ret2.Add(ss);
                }
                southEntries.AddRange(ret2);
            }
            // Build the accumulator tree
            var firstIndex = 1;
            while (firstIndex < southLayer.Count)
            {
                firstIndex <<= 1;
            }
            //const tree = Array.from(new Array(2 * firstIndex - 1), () => 0);
            var tree = new List<object>() { };
            for (int i = 0; i < 2 * firstIndex - 1; i++)
            {
                tree.Add(0);
            }
            firstIndex -= 1;
            // Calculate the weighted crossings
            var cc = 0;
            foreach (dynamic entry in southEntries)
            {
                var index = entry["pos"] + firstIndex;
                tree[index] += entry["weight"];
                dynamic weightSum = 0;
                while (index > 0)
                {
                    if (index % 2 != 0)
                    {
                        weightSum += tree[index + 1];
                    }
                    index = (index - 1) >> 1;
                    tree[index] += entry["weight"];
                }
                cc += entry["weight"] * weightSum;
            }
            return cc;
        }
        #endregion
    }
    public class barycenterDto
    {

        public string v;
        public int? barycenter = null;
        public int weight;
    }

}

