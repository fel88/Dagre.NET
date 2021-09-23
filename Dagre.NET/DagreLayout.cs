using System;
using System.Collections.Generic;
using System.Linq;

namespace Dagre
{
    public static class DagreLayout
    {
        /*public void layout(DagreGraph g)
        {
            var layoutGraph = buildLayoutGraph(g);
            runLayout(layoutGraph);
            updateInputGraph(g, layoutGraph);
        }*/

        /*
         * Copies final layout information from the layout graph back to the input
         * graph. This process only copies whitelisted attributes from the layout graph
         * to the input graph, so it serves as a good place to determine what
         * attributes can influence layout.
         */
        //private void updateInputGraph(DagreGraph inputGraph, DagreGraph layoutGraph)
        //{
        //    foreach (var v in inputGraph.nodes())
        //    {
        //        var inputLabel = inputGraph.node(v);
        //        var layoutLabel = layoutGraph.node(v);

        //        if (inputLabel != null)
        //        {
        //            inputLabel.x = layoutLabel.x;
        //            inputLabel.y = layoutLabel.y;

        //        }
        //        if (layoutGraph.children(v).Length > 0)
        //        {
        //            inputLabel.width = layoutLabel.width;
        //            inputLabel.height = layoutLabel.height;
        //        }
        //    }

        //    foreach (var e in inputGraph.edges())
        //    {
        //        var inputLabel = inputGraph.edge(e);
        //        var layoutLabel = layoutGraph.edge(e);
        //        inputLabel.points = layoutLabel.points;
        //        if (layoutLabel.x != null)
        //        {
        //            inputLabel.x = layoutLabel.x;
        //            inputLabel.y = layoutLabel.y;
        //        }
        //    }

        //    inputGraph.graph().width = layoutGraph.graph().width;
        //    inputGraph.graph().height = layoutGraph.graph().height;

        //}

        /*
         * This idea comes from the Gansner paper: to account for edge labels in our
         * layout we split each rank in half by doubling minlen and halving ranksep.
         * Then we can place labels at these mid-points between nodes.
         *
         * We also add some minimal padding to the width to push the label for the edge
         * away from the edge itself a bit.
         */
        public static void makeSpaceForEdgeLabels(DagreGraph g)
        {
            var graph = g.graph();
            graph["ranksep"] /= 2;
            foreach (var e in g.edgesRaw())
            {
                dynamic edge = g.edgeRaw(e);
                var aa = ((int)edge["minlen"]) * 2;
                edge["minlen"] = aa;
                if (edge["labelpos"].ToLower() != "c")
                {
                    if (graph["rankdir"] == "TB" || graph["rankdir"] == "BT")
                    {
                        edge["width"] += edge["labeloffset"];
                    }
                    else
                    {
                        edge["height"] += edge["labeloffset"];
                    }
                }
            }
        }

        public static object canonicalize(object attrs)
        {
            if (attrs is DagreLabel dl)
            {
                DagreLabel ret = new DagreLabel();
                ret.nodesep = dl.nodesep;
                ret.ranksep = dl.ranksep;
                return ret;
            }
            if (attrs is DagreEdge de)
            {
                DagreEdge ret = new DagreEdge();
                ret.forwardName = de.forwardName;
                return de;
            }
            if (attrs is DagreNode dn)
            {
                DagreNode ret = new DagreNode();
                ret.width = dn.width;
                ret.height = dn.height;
                return ret;
            }
            /*var newAttrs = { };
            _.forEach(attrs, function(v, k) {
                newAttrs[k.toLowerCase()] = v;
            });
            return newAttrs;*/
            return new object();
        }

        public class GraphDefaults
        {
            public int edgesep = 20;
            public int nodesep = 50;
            public string rankdir = "tb";
            public int ranksep = 50;
        }


        public static object merge(object[] list)
        {
            DagreLabel lab = new DagreLabel();
            lab.edgesep = 20;
            lab.nodesep = 25;
            lab.rankdir = "tb";
            lab.ranksep = 20;
            return lab;
            return new object();
        }

        public static object pick(object o1, object o2)
        {
            return new object();
        }
        public static object selectNumberAttrs(object label, string[] attrs)
        {
            //clone with selected args??
            return label;
        }

        public static object defaults(object o1, object o2)
        {
            if (o1 is DagreNode dn)
            {
                if (o2 is NodeDefaults nd)
                {
                    if (dn.width == null)
                    {
                        dn.width = nd.width;
                    }
                    if (dn.height == null)
                    {
                        dn.height = nd.height;
                    }
                    return dn;
                }
                else
                {
                    throw new NotImplementedException();
                }

            }
            throw new NotImplementedException();
        }

        public class EdgeDefaults
        {
            public int height = 0;
            public int labeloffset = 10;
            public string labelpos = "r";
            public int minlen = 1;
            public int weight = 1;
            public int width = 0;
        }
        public class NodeDefaults
        {
            public int height = 0;
            public int width = 0;
        }
        /*
         * Constructs a new graph from the input graph, which can be used for layout.
         * This process copies only whitelisted attributes from the input graph to the
         * layout graph. Thus this function serves as a good place to determine what
         * attributes can influence layout.
         */
        //public static DagreGraph buildLayoutGraph(DagreGraph inputGraph)
        //{
        //    string[] graphNumAttrs = new string[] { "nodesep", "edgesep", "ranksep", "marginx", "narginy" };
        //    string[] edgeNumAttrs = new string[] { "minlen", "weight", "width", "height", "labeloffset" };
        //    string[] nodeNumAttrs = new string[] { "width", "height" };
        //    string[] graphAttrs = new string[] { "acyclicer", "ranker", "rankdir", "align" };
        //    string[] edgeAttrs = new string[] { "labelpos" };
        //    var g = new DagreGraph(true) { _isMultigraph = true, _isCompound = true };
        //    var graph = canonicalize(inputGraph.graph());

        //    g.setGraph(merge(new object[] { null, new GraphDefaults(), selectNumberAttrs(graph, graphNumAttrs), pick(graph, graphAttrs) }));


        //    foreach (var v in inputGraph.nodesRaw())
        //    {
        //        var node = canonicalize(inputGraph.nodeRaw(v));
        //        g.setNode(v, defaults(selectNumberAttrs(node, nodeNumAttrs), new NodeDefaults()));
        //        g.setParent2(v, inputGraph.parent(v));
        //    }


        //    foreach (var e in inputGraph.edgesRaw())
        //    {
        //        var edge = canonicalize(inputGraph.edgeRaw(e));
        //        g.setEdgeRaw(new object[] { e, merge(new object[] { null, new EdgeDefaults(), selectNumberAttrs(edge, edgeNumAttrs), pick(edge, edgeAttrs) }) });
        //    }
        //    return g;
        //}

        public static void removeSelfEdges(DagreGraph g)
        {
            var ar = g.edgesRaw().ToArray();
            foreach (dynamic e in ar)
            {
                if (e["v"] == e["w"])
                {
                    dynamic node = g.nodeRaw(e.v);
                    if (node["selfEdges"] == null)
                    {
                        node["selfEdges"] = new List<SelfEdgeInfo>();
                    }
                    node["selfEdges"].Add(new SelfEdgeInfo() { e = e, label = g.edgeRaw(e) });
                    g.removeEdge(e);
                }
            }
        }

        public static void rank(DagreGraph g)
        {
            string res = null;
            if (g.graph().ContainsKey("ranker"))
                res = g.graph()["ranker"];
            switch (res)
            {
                case "network-simplex":
                    throw new NotImplementedException();
                    break;
                case "tight-tree":
                    throw new NotImplementedException();
                    break;
                case "longest-path":
                    throw new NotImplementedException();
                    break;
                default:
                    networkSimplexRanker(g);
                    break;
            }
        }
        public static void networkSimplexRanker(DagreGraph g)
        {
            networkSimplexModule.networkSimplex(g);
        }
        /*
 * Creates temporary dummy nodes that capture the rank in which each edge's
 * label is going to, if it has one of non-zero width and height. We do this
 * so that we can safely remove empty ranks while preserving balance for the
 * label's position.
 */
        public static void injectEdgeLabelProxies(DagreGraph g)
        {
            foreach (dynamic e in g.edgesRaw())
            {
                var edge = g.edgeRaw(e);
                if (edge.ContainsKey("width") && edge["width"] != 0 && edge.ContainsKey("height") && edge["height"] != 0)
                {
                    dynamic v = g.nodeRaw(e["v"]);
                    dynamic w = g.nodeRaw(e["w"]);

                    JavaScriptLikeObject label = new JavaScriptLikeObject();
                    label.AddOrUpdate("rank", (w["rank"] - v["rank"]) / 2 + v["rank"]);
                    label.AddOrUpdate("e", e);
                    util.addDummyNode(g, "edge-proxy", label, "_ep");
                }
            }
        }

        public static void removeEmptyRanks(DagreGraph g)
        {
            Dictionary<int, List<string>> layers = new Dictionary<int, List<string>>();

            // Ranks may not start at 0, so we need to offset them
            if (g.nodesRaw().Length > 0)
            {
                var offset = g.nodesRaw().Select(v => g.nodeRaw(v)["rank"]).Min();
                //var offset = _.min(_.map(g.nodes(), function(v) { return g.node(v).rank; }));

                foreach (var v in g.nodesRaw())
                {
                    var rank = (g.nodeRaw(v)["rank"] - offset);
                    if (!layers.ContainsKey(rank))
                    {
                        layers.Add(rank, new List<string>());
                    }
                    layers[rank].Add(v);
                }
            }

            var delta = 0;
            var nodeRankFactor = g.graph()["nodeRankFactor"];
            foreach (var pair in layers.OrderBy(z => z.Key))
            {

                var vs = pair.Value;
                var i = pair.Key;
                if (vs == null && i % nodeRankFactor != 0)
                {
                    --delta;
                }
                else if (delta != 0)
                {
                    foreach (var v in vs)
                    {
                        g.nodeRaw(v)["rank"] += delta;
                    }
                }
            }
        }

        public static void runLayout(DagreGraph g)
        {
            makeSpaceForEdgeLabels(g);
            removeSelfEdges(g);
            acyclic.run(g);

            nestingGraph.run(g);

            rank(util.asNonCompoundGraph(g));

            injectEdgeLabelProxies(g);

            removeEmptyRanks(g);
            nestingGraph.cleanup(g);

            util.normalizeRanks(g);

            assignRankMinMax(g);

            removeEdgeLabelProxies(g);

            normalize.run(g);

            parentDummyChains._parentDummyChains(g);

            addBorderSegments._addBorderSegments(g);
            order._order(g);

            insertSelfEdges(g);

            coordinateSystem.adjust(g);
            position(g);
            positionSelfEdges(g);
            removeBorderNodes(g);

            normalize.undo(g);

            fixupEdgeLabelCoords(g);
            coordinateSystem.undo(g);
            translateGraph(g);
            assignNodeIntersects(g);
            reversePointsForReversedEdges(g);
            acyclic.undo(g);
        }

        public static void reversePointsForReversedEdges(DagreGraph g)
        {
            foreach (var e in g.edges())
            {
                var edge = g.edge(e);
                if (edge.ContainsKey("reversed"))
                {
                    edge["points"].Reverse();
                }
            }
        }

        public static void assignNodeIntersects(DagreGraph g)
        {
            foreach (var e in g.edges())
            {
                var edge = g.edge(e);
                var nodeV = g.node(e["v"]);
                var nodeW = g.node(e["w"]);
                dynamic p1, p2;
                if (!edge.ContainsKey("points"))
                {
                    edge["points"] = new List<object>();
                    p1 = DagreLayout.makePoint(nodeW["x"], nodeW["y"]);
                    p2 = DagreLayout.makePoint(nodeV["x"], nodeV["y"]);
                }
                else
                {
                    p1 = edge["points"][0];
                    p2 = edge["points"][edge["points"].Count - 1];
                }
                edge["points"].Insert(0, util.intersectRect(nodeV, p1));
                edge["points"].Add(util.intersectRect(nodeW, p2));
            }

        }
        public static void translateGraph(DagreGraph g)
        {
            double minX = double.PositiveInfinity;
            double maxX = 0;
            double minY = double.PositiveInfinity;
            double maxY = 0;
            var graphLabel = g.graph();
            dynamic marginX = 0;
            if (graphLabel.ContainsKey("marginx"))
            {
                marginX = graphLabel["marginx"];
            }
            dynamic marginY = 0;
            if (graphLabel.ContainsKey("marginy"))
            {
                marginY = graphLabel["marginy"];
            }

            Action<dynamic> getExtremes = (_attrs) =>
              {
                  dynamic attrs = _attrs;
                  dynamic x = attrs["x"];
                  dynamic y = attrs["y"];
                  dynamic w = attrs["width"];
                  dynamic h = attrs["height"];
                  minX = Math.Min(minX, (float)x - (float)w / 2f);
                  maxX = Math.Max(maxX, (float)x + (float)w / 2f);
                  minY = Math.Min(minY, (float)y - (float)h / 2f);
                  maxY = Math.Max(maxY, (float)y + (float)h / 2f);
              };

            foreach (var v in g.nodes())
            {
                getExtremes(g.node(v));
            }

            foreach (var e in g.edges())
            {
                var edge = g.edge(e);
                if (edge.ContainsKey("x"))
                {
                    getExtremes(edge);
                }
            }



            minX -= marginX;
            minY -= marginY;

            foreach (var v in g.nodes())
            {
                var node = g.node(v);
                node["x"] -= minX;
                node["y"] -= minY;
            }

            foreach (var e in g.edges())
            {
                var edge = g.edge(e);
                foreach (var p in edge["points"])
                {
                    p["x"] -= minX;
                    p["y"] -= minY;
                }

                if (edge.ContainsKey("x")) { edge["x"] -= minX; }
                if (edge.ContainsKey("н")) { edge["y"] -= minY; }
            }




            graphLabel["width"] = maxX - minX + marginX;
            graphLabel["height"] = maxY - minY + marginY;
        }
        public static void fixupEdgeLabelCoords(DagreGraph g)
        {
            foreach (var e in g.edges())
            {
                var edge = g.edge(e);
                if (edge.ContainsKey("x"))
                {
                    if (edge["labelpos"] == "l" || edge["labelpos"] == "r")
                    {
                        edge["width"] -= edge["labeloffset"];
                    }
                    switch (edge["labelpos"])
                    {
                        case "l": edge["x"] -= (float)edge["width"] / 2f + (float)edge["labeloffset"]; break;
                        case "r": edge["x"] += (float)edge["width"] / 2f + (float)edge["labeloffset"]; break;
                    }
                }
            }

        }
        public static object makePoint(object x, object y)
        {
            JavaScriptLikeObject j = new JavaScriptLikeObject();
            j.Add("x", x);
            j.Add("y", y);
            return j;
        }
        public static void positionSelfEdges(DagreGraph g)
        {
            foreach (var v in g.nodes())
            {
                var node = g.node(v);
                if (node.ContainsKey("dummy") && node["dummy"] == "selfedge")
                {
                    var selfNode = g.node(node["e"]["v"]);
                    var x = (selfNode["x"] + selfNode["width"] / 2);
                    var y = selfNode["y"];
                    var dx = (node["x"] - x);
                    var dy = (selfNode["height"] / 2);
                    g.setEdge(new object[] { node["e"], node["label"] });
                    g.removeNode(v);
                    node["label"]["points"] = new List<object>{
                    makePoint(  x + 2 * dx / 3, y - dy ),
         makePoint( x + 5 * dx / 6,  y - dy ),
         makePoint( x + dx    ,  y ),
         makePoint( x + 5 * dx / 6,  y + dy ),
         makePoint( x + 2 * dx / 3,  y + dy)
                    };
                    node["label"]["x"] = node["x"];
                    node["label"]["y"] = node["y"];
                }
            }
        }

        public static void position(DagreGraph g)
        {
            g = util.asNonCompoundGraph(g);

            

            dynamic layering = util.buildLayerMatrix(g);
            var rankSep = g.graph()["ranksep"];
            double prevY = 0;
            foreach (var layer in layering)
            {
                List<dynamic> oo = new List<dynamic>();
                foreach (var item in layer)
                {
                    oo.Add((float)(g.node(item.Value)["height"]));
                }
                //var maxHeight = (layer as IEnumerable<object>).Select(v => g.node(v)["height"]).Max().Value;
                var maxHeight = oo.Max();
                foreach (var v in layer)
                {
                    g.node(v.Value)["y"] = prevY + maxHeight / 2f;
                }

                prevY += maxHeight + rankSep;
            }
            foreach (var item in bk.entries(bk.positionX(g)))
            {
                g.node(item[0])["x"] = item[1];
            }
        }




        public static void removeBorderNodes(DagreGraph g)
        {
            foreach (var v in g.nodes())
            {
                if (g.children(v).Length > 0)
                {
                    var node = g.node(v);
                    var t = g.node(node["borderTop"]);
                    var b = g.node(node["borderBottom"]);
                    var l = g.node(node["borderLeft"][node["borderLeft"].Count - 1]);
                    var r = g.node(node["borderRight"][node["borderRight"].Count - 1]);
                    node.width = Math.Abs(r.x - l.x);
                    node.height = Math.Abs(b.y - t.y);
                    node.x = l.x + node.width / 2;
                    node.y = t.y + node.height / 2;
                }
            }

            foreach (var v in g.nodes())
            {
                var nd = g.node(v);
                if (nd.ContainsKey("dummy") && nd["dummy"] == "border")
                {
                    g.removeNode(v);
                }
            }
        }

        public static void insertSelfEdges(DagreGraph g)
        {
            dynamic layers = util.buildLayerMatrix(g);
            foreach (var layer in layers)
            {
                var orderShift = 0;
                for (int i = 0; i < layer.Count; i++)
                {
                    var v = layer["" + i];
                    var node = g.node(v);

                    node["order"] = i + orderShift;
                    if (node.ContainsKey("selfEdges"))
                    {
                        foreach (var selfEdge in node["selfEdges"])
                        {
                            JavaScriptLikeObject attrs = new JavaScriptLikeObject();
                            attrs.Add("width", selfEdge["label"]["width"]);
                            attrs.Add("height", selfEdge["label"]["height"]);
                            attrs.Add("rank", node["rank"]);
                            attrs.Add("order", i + (++orderShift));
                            attrs.Add("e", selfEdge["e"]);
                            attrs.Add("label", selfEdge["label"]);
                            util.addDummyNode(g, "selfedge", attrs, "_se");
                        }
                        node.Remove("selfEdges");
                    }
                }
            }
        }

        public static void removeEdgeLabelProxies(DagreGraph g)
        {
            foreach (var v in g.nodesRaw())
            {
                var node = g.nodeRaw(v);
                if (node.ContainsKey("dummy") && node["dummy"] == "edge-proxy")
                {
                    g.edgeRaw(node["e"])["labelRank"] = node["rank"];
                    g.removeNode(v);
                }
            }

        }

        public static void assignRankMinMax(DagreGraph g)
        {
            int maxRank = 0;
            foreach (var v in g.nodesRaw())
            {
                var node = g.nodeRaw(v);
                if (node.ContainsKey("borderTop"))
                {
                    node["minRank"] = g.nodeRaw(node["borderTop"])["rank"];
                    //node.maxRank = g.node(node.borderLeft).rank;
                    maxRank = Math.Max(maxRank, node["maxRank"]);
                }
            }

            g.graph()["maxRank"] = maxRank;
        }
    }
}
