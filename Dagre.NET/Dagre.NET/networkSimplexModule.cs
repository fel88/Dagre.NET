using System;
using System.Collections.Generic;
using System.Linq;

namespace Dagre
{
    public class networkSimplexModule
    {

        /*
         * The network simplex algorithm assigns ranks to each node in the input graph
         * and iteratively improves the ranking to reduce the length of edges.
         *
         * Preconditions:
         *
         *    1. The input graph must be a DAG.
         *    2. All nodes in the graph must have an object value.
         *    3. All edges in the graph must have "minlen" and "weight" attributes.
         *
         * Postconditions:
         *
         *    1. All nodes in the graph will have an assigned "rank" attribute that has
         *       been optimized by the network simplex algorithm. Ranks start at 0.
         *
         *
         * A rough sketch of the algorithm is as follows:
         *
         *    1. Assign initial ranks to each node. We use the longest path algorithm,
         *       which assigns ranks to the lowest position possible. In general this
         *       leads to very wide bottom ranks and unnecessarily long edges.
         *    2. Construct a feasible tight tree. A tight tree is one such that all
         *       edges in the tree have no slack (difference between length of edge
         *       and minlen for the edge). This by itself greatly improves the assigned
         *       rankings by shorting edges.
         *    3. Iteratively find edges that have negative cut values. Generally a
         *       negative cut value indicates that the edge could be removed and a new
         *       tree edge could be added to produce a more compact graph.
         *
         * Much of the algorithms here are derived from Gansner, et al., "A Technique
         * for Drawing Directed Graphs." The structure of the file roughly follows the
         * structure of the overall algorithm.
         */



        public static void initLowLimValues(DagreGraph tree, string root = null)
        {
            if (root == null)
            {
                root = tree.nodesRaw()[0];
            }

            dfsAssignLowLim(tree, new JavaScriptLikeObject(), 1, root);
        }

        public static int dfsAssignLowLim(DagreGraph tree, JavaScriptLikeObject visited, int nextLim, string v, object parent = null)
        {
            var low = nextLim;
            var label = tree.nodeRaw(v);


            visited.AddOrUpdate(v, true);
            foreach (var w in tree.neighbors(v))
            {
                if (!visited.ContainsKey(w))
                {
                    nextLim = dfsAssignLowLim(tree, visited, nextLim, w, v);
                }
            }


            if (!label.ContainsKey("low"))
            {
                label.Add("low", low);
            }
            label["low"] = low;
            if (!label.ContainsKey("lim"))
            {
                label.Add("lim", low);
            }
            label["lim"] = nextLim++;
            if (parent != null)
            {
                label["parent"] = parent;
            }
            else
            {
                // TODO should be able to remove this when we incrementally update low lim
                //label["parent"] = null;
                dynamic d = label;
                if (d.ContainsKey("parent"))
                    d.Remove("parent");
                //delete label.parent;
            }

            return nextLim;
        }

        public static string[] postorder(DagreGraph t, string[] g)
        {
            return graphlib.dfs(t, g, "post");
        }
        public static string[] preorder(DagreGraph t, string[] g)
        {
            return graphlib.dfs(t, g, "pre");
        }
        /*
 * Initializes cut values for all edges in the tree.
 */
        public static void initCutValues(DagreGraph t, DagreGraph g)
        {
            var vs = postorder(t, t.nodesRaw());
            //vs = vs.slice(0, vs.length - 1);
            vs = vs.Take(vs.Length - 1).ToArray();
            foreach (var v in vs)
            {
                assignCutValue(t, g, v);
            }
        }
        public static void assignCutValue(DagreGraph t, DagreGraph g, string child)
        {
            var childLab = t.nodeRaw(child);
            var parent = childLab["parent"];
            var edge = t.edgeRaw(new object[] { child, parent });
            //TODO!!! check tha edge can be null
            if (edge != null)
            {
                var res = calcCutValue(t, g, child);
                edge["cutvalue"] = res;
            }
        }

        /*
         * Given the tight tree, its graph, and a child in the graph calculate and
         * return the cut value for the edge between the child and its parent.
         */
        public static int calcCutValue(DagreGraph t, DagreGraph g, string child)
        {
            var childLab = t.nodeRaw(child);
            var parent = childLab["parent"];
            // True if the child is on the tail end of the edge in the directed graph
            var childIsTail = true;
            // The graph's view of the tree edge we're inspecting
            var graphEdge = g.edgeRaw(new object[] { child, parent });
            // The accumulated cut value for the edge between this node and its parent
            var cutValue = 0;

            if (graphEdge == null)
            {
                childIsTail = false;
                graphEdge = g.edgeRaw(new object[] { parent, child });
            }

            cutValue = graphEdge["weight"];

            foreach (dynamic e in g.nodeEdges(child))
            {
                var isOutEdge = e["v"] == child;
                var other = isOutEdge ? e["w"] : e["v"];
                if (other != parent)
                {
                    var pointsToHead = isOutEdge == childIsTail;
                    var otherWeight = g.edgeRaw(e)["weight"];

                    cutValue += pointsToHead ? otherWeight : -otherWeight;
                    if (isTreeEdge(t, child, other))
                    {
                        var otherCutValue = t.edgeRaw(new object[] { child, other })["cutvalue"];
                        cutValue += pointsToHead ? -otherCutValue : otherCutValue;
                    }
                }
            }


            return cutValue;
        }

        /*
         * Returns true if the edge is in the tree.
         */
        public static bool isTreeEdge(DagreGraph tree, string u, string v)
        {
            return tree.hasEdgeRaw(new object[] { u, v });
        }

        public static dynamic enterEdge(DagreGraph t, DagreGraph g, dynamic edge)
        {
            var v = edge["v"];
            var w = edge["w"];

            // For the rest of this function we assume that v is the tail and w is the
            // head, so if we don't have this edge in the graph we should flip it to
            // match the correct orientation.
            if (!g.hasEdgeRaw(new object[] { v, w }))
            {
                v = edge["w"];
                w = edge["v"];
            }

            var vLabel = t.nodeRaw(v);
            var wLabel = t.nodeRaw(w);
            var tailLabel = vLabel;
            var flip = false;

            // If the root is in the tail of the edge then we need to flip the logic that
            // checks for the head and tail nodes in the candidates function below.
            if (vLabel["lim"] > wLabel["lim"])
            {
                tailLabel = wLabel;
                flip = true;
            }

            var candidates = g.edgesRaw().Where(ee =>
            {
                return flip == isDescendant(t, t.nodeRaw(ee["v"]), tailLabel) &&
                       flip != isDescendant(t, t.nodeRaw(ee["w"]), tailLabel);
            }).ToArray();

            //return _.minBy(candidates, function(edge) { return slack(g, edge); });
            if (candidates.Any())
            {
                return candidates.OrderBy(z => slack(g, z)).First();

            }
            return null;
        }

        /*
         * Returns true if the specified node is descendant of the root node per the
         * assigned low and lim attributes in the tree.
         */
        public static bool isDescendant(dynamic tree, dynamic vLabel, dynamic rootLabel)
        {
            return rootLabel["low"] <= vLabel["lim"] && vLabel["lim"] <= rootLabel["lim"];
        }

        public static void networkSimplex(DagreGraph g)
        {
            g = util.simplify(g);
         
            longestPath(g);
           
            var tree = feasibleTree(g);
           
            initLowLimValues(tree);

          


            initCutValues(tree, g);
           
            object e = null, f = null;
            int step = 0;
            while ((e = leaveEdge(tree)) != null)
            {
               
                f = enterEdge(tree, g, e);
                exchangeEdges(tree, g, e, f, step);
                step++;
            }
        }

        public static void exchangeEdges(DagreGraph t, dynamic g, dynamic e, dynamic f, int step)
        {
            var v = e["v"];
            var w = e["w"];
            t.removeEdge(new[] { v, w });
            t.setEdge(new object[] { f["v"], f["w"], new JavaScriptLikeObject() });
          
            initLowLimValues(t);
            initCutValues(t, g);
            updateRanks(t, g);
        }
        public static object leaveEdge(DagreGraph tree)
        {
            return tree.edgesRaw().FirstOrDefault(e => tree.edgeRaw(e) != null && tree.edgeRaw(e)["cutvalue"] < 0);
        }

        public static void updateRanks(DagreGraph t, DagreGraph g)
        {
            var root = t.nodes().FirstOrDefault(v => { return !g.node(v).ContainsKey("parent"); });
            var vs = preorder(t, new string[] { root });
            //vs = vs.slice(1);
            vs = vs.Skip(1).ToArray();
            foreach (var v in vs)
            {
                dynamic parent = null;
                //TODO!!! check that parent can be null
                if (t.node(v).ContainsKey("parent"))
                {
                    parent = t.node(v)["parent"];
                }

                var edge = g.edgeRaw(new object[] { v, parent });
                var flipped = false;

                if (edge == null)
                {
                    edge = g.edgeRaw(new object[] { parent, v });
                    flipped = true;
                }

                g.node(v)["rank"] = g.node(parent)["rank"] + (flipped ? edge["minlen"] : -edge["minlen"]);
            }
        }

        /*
         * Initializes ranks for the input graph using the longest path algorithm. This
         * algorithm scales well and is fast in practice, it yields rather poor
         * solutions. Nodes are pushed to the lowest layer possible, leaving the bottom
         * ranks wide and leaving edges longer than necessary. However, due to its
         * speed, this algorithm is good for getting an initial ranking that can be fed
         * into other algorithms.
         *
         * This algorithm does not normalize layers because it will be used by other
         * algorithms in most cases. If using this algorithm directly, be sure to
         * run normalize at the end.
         *
         * Pre-conditions:
         *
         *    1. Input graph is a DAG.
         *    2. Input graph node labels can be assigned properties.
         *
         * Post-conditions:
         *
         *    1. Each node will be assign an (unnormalized) "rank" property.
         */
        public static void longestPath(DagreGraph g)
        {
            HashSet<string> visited = new HashSet<string>();

            Func<string, dynamic> dfs = null;
            dfs = (v) =>
            {
                dynamic label = g.nodeRaw(v);
                if (visited.Contains(v))
                {
                    return label["rank"];
                }
                visited.Add(v);
                var rank = int.MaxValue;
                foreach (dynamic e in g.outEdges(v))
                {
                    var x = dfs(e["w"]) - g.edgeRaw(e)["minlen"];
                    if (x < rank)
                    {
                        rank = x;
                    }
                }
                //var rank = g.outEdges(v).Select((dynamic e) => dfs(e["w"]) - ((dynamic)g.edgeRaw(e))["minlen"]).Min();

                if (rank == int.MaxValue || // return value of _.map([]) for Lodash 3
                    rank == null // return value of _.map([]) for Lodash 4
                    )
                { // return value of _.map([null])
                    rank = 0;
                }

                label["rank"] = rank;
                return label["rank"];

            };

            foreach (var item in g.sources())
            {
                dfs(item);
            }
        }

        /*
         * Returns the amount of slack for the given edge. The slack is defined as the
         * difference between the length of the edge and its minimum length.
         */
        public static int? slack(dynamic g, dynamic e)
        {
            var node1 = g.nodeRaw(e["w"]);
            var node2 = g.nodeRaw(e["v"]);
            var edge = g.edgeRaw(e);
            if (!node1.ContainsKey("rank")) return null;
            if (!node2.ContainsKey("rank")) return null;
            if (!edge.ContainsKey("minlen")) return null;
            return (dynamic)node1["rank"] - (dynamic)node2["rank"] - edge["minlen"];
        }
        /*public static int slack(DagreGraph g, object e)
        {
            return slack(g, e as DagreEdgeIndex);
        }*/

        /*
         * Constructs a spanning tree with tight edges and adjusted the input node's
         * ranks to achieve this. A tight edge is one that is has a length that matches
         * its "minlen" attribute.
         *
         * The basic structure for this function is derived from Gansner, et al., "A
         * Technique for Drawing Directed Graphs."
         *
         * Pre-conditions:
         *
         *    1. Graph must be a DAG.
         *    2. Graph must be connected.
         *    3. Graph must have at least one node.
         *    5. Graph nodes must have been previously assigned a "rank" property that
         *       respects the "minlen" property of incident edges.
         *    6. Graph edges must have a "minlen" property.
         *
         * Post-conditions:
         *
         *    - Graph nodes will have their rank adjusted to ensure that all edges are
         *      tight.
         *
         * Returns a tree (undirected graph) that is constructed using only "tight"
         * edges.
         */

        public static DagreGraph feasibleTree(DagreGraph g)
        {
            var t = new DagreGraph(false) { _isDirected = false };

            // Choose arbitrary node from which to start our tree
            var start = g.nodesRaw()[0];
            var size = g.nodeCount();
            t.setNode(start, new JavaScriptLikeObject());

            dynamic edge;
            int delta;
            while (tightTree(t, g) < size)
            {
                edge = findMinSlackEdge(t, g);
                delta = t.hasNode(edge["v"]) ? slack(g, edge) : -slack(g, edge);
                shiftRanks(t, g, delta);
            }

            return t;
        }


        /*
         * Finds a maximal tree of tight edges and returns the number of nodes in the
         * tree.
         */
        public static int tightTree(DagreGraph t, DagreGraph g)
        {
            //Action<string> dfs = null;
            var stack = t.nodesRaw().Reverse().ToList();
            while (stack.Count > 0)
            {
                var v = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                foreach (dynamic e in g.nodeEdges(v))
                {
                    var edgeV = e["v"];
                    var w = (v == edgeV) ? e["w"] : edgeV;
                    var _slack = slack(g, e);
                    if (!t.hasNode(w) && (_slack == null || _slack == 0))
                    {
                        t.setNode(w, new JavaScriptLikeObject() { });
                        t.setEdge(new object[] { v, w, new JavaScriptLikeObject() { } });
                        stack.Add(w);
                    }
                }
            }
            //dfs = (v) =>
            //{
            //    foreach (var ee in g.nodeEdges(v))
            //    {
            //        var e = ee as DagreEdgeIndex;
            //        var edgeV = e.v;
            //        var w = (v == edgeV) ? e.w : edgeV;
            //        if (!t.hasNode(w) && slack(g, e) != 0)
            //        {
            //            t.setNodeRaw(w, null);
            //            t.setEdgeRaw(new object[] { v, w, null });
            //            dfs(w);
            //        }
            //    }
            //};

            //foreach (var item in t.nodes())
            //{
            //    dfs(item);
            //}
            return t.nodeCount();
        }

        /*
         * Finds the edge with the smallest slack that is incident on tree and returns
         * it.
         */
        public static dynamic findMinSlackEdge(DagreGraph t, DagreGraph g)
        {
            return g.edges().Where(e =>
            {
                return t.hasNode(e["v"]) != t.hasNode(e["w"]);
            }).OrderBy(e => slack(g, e)).First();
        }

        public static void shiftRanks(DagreGraph t, DagreGraph g, int delta)
        {
            foreach (var v in t.nodes())
            {
                g.node(v)["rank"] += delta;
            }

        }
    }

    public class graphlib
    {
        public static string[] dfs(DagreGraph g, string[] vs, string order)
        {

            JavaScriptLikeObject visited = new JavaScriptLikeObject();



            Func<string, string[]> navigation = null;
            if (g._isDirected) navigation = (u) =>
               {
                   return g.successors(u).OrderBy(z => z).ToArray();
               };
            else

                navigation = (u) => { return g.neighbors(u).OrderBy(z => z).ToArray(); };

            List<string> acc = new List<string>();

            foreach (var v in vs)
            {
                if (!g.hasNode(v))
                {
                    throw new DagreException("graph does not have node: " + v);
                }
                doDfs(g, v, order == "post", visited, navigation, acc);
            }
            return acc.ToArray();
        }

        public static void doDfs(DagreGraph g, string v, bool postorder, JavaScriptLikeObject visited, Func<string, string[]> navigation, List<string> acc)
        {
            if (visited.ContainsKey(v)) return;

            visited.Add(v, true);
            if (!postorder) { acc.Add(v); }
            foreach (var w in navigation(v))
            {
                doDfs(g, w, postorder, visited, navigation, acc);
            }
            if (postorder)
            {
                acc.Add(v);
            }

        }
    }
}
