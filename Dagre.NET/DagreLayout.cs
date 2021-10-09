using System;
using System.Collections.Generic;
using System.Linq;

namespace Dagre
{
    public static class DagreLayout
    {      
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
                case "tight-tree":
                    throw new NotImplementedException();                    
                case "longest-path":
                    throw new NotImplementedException();                    
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
            Dictionary<int, object> layers = new Dictionary<int, object>();

            // Ranks may not start at 0, so we need to offset them
            if (g.nodesRaw().Length > 0)
            {
                var offset = g.nodesRaw().Where(z => g.nodeRaw(z).ContainsKey("rank")).Select(v => g.nodeRaw(v)["rank"]).Min();
                //var offset = _.min(_.map(g.nodes(), function(v) { return g.node(v).rank; }));

                foreach (var v in g.nodesRaw())
                {
                    if (!g.nodeRaw(v).ContainsKey("rank")) continue;
                    var rank = -offset;

                    rank += g.nodeRaw(v)["rank"];
                    if (!layers.ContainsKey(rank))
                    {
                        layers.Add(rank, new List<string>());
                    }
                    ((dynamic)layers[rank]).Add(v);
                }
            }

            var delta = 0;
            var nodeRankFactor = g.graph()["nodeRankFactor"];
            for (int i = 0; i <= layers.Keys.Max(); i++)
            {
                if (!layers.ContainsKey(i) && i % nodeRankFactor != 0)
                {
                    --delta;
                }
                else if (delta != 0)
                {
                    if (layers.ContainsKey(i))
                    {
                        dynamic vs = layers[i];
                        foreach (var v in vs)
                        {
                            g.nodeRaw(v)["rank"] += delta;
                        }
                    }
                }
            }
            /* foreach (var pair in layers.OrderBy(z => z.Key))
            {

                 dynamic vs = pair.Value;
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
             }*/
        }


        public static void runLayout(DagreGraph g, Action<ExtProgressInfo> progress = null)        
        {
            ExtProgressInfo ext = new ExtProgressInfo();

            progress?.Invoke(ext);

            makeSpaceForEdgeLabels(g);
            removeSelfEdges(g);
            acyclic.run(g);

            nestingGraph.run(g);

            ext.Caption = "rank";
            rank(util.asNonCompoundGraph(g));

            injectEdgeLabelProxies(g);

            removeEmptyRanks(g);
            nestingGraph.cleanup(g);

            util.normalizeRanks(g);

            assignRankMinMax(g);

            removeEdgeLabelProxies(g);

            ext.MainProgress = 0.1f;
            progress?.Invoke(ext);
            ext.Caption = "normalize.run";
            normalize.run(g);

            parentDummyChains._parentDummyChains(g);

            addBorderSegments._addBorderSegments(g);
            ext.Caption = "order";
            ext.MainProgress = 0.3f;
            progress?.Invoke(ext);
            order._order(g, (f) =>
            {
                ext.AdditionalProgress = f;
                progress?.Invoke(ext);
            });


            ext.MainProgress = 0.5f;
            progress?.Invoke(ext);
            insertSelfEdges(g);

            coordinateSystem.adjust(g);
            position(g);
            positionSelfEdges(g);
            removeBorderNodes(g);

            ext.Caption = "undo";
            normalize.undo(g, (f) =>
            {
                ext.AdditionalProgress = f;
                progress?.Invoke(ext);
            });



            fixupEdgeLabelCoords(g);
            coordinateSystem.undo(g);
            translateGraph(g);
            assignNodeIntersects(g);
            reversePointsForReversedEdges(g);
            acyclic.undo(g);

            ext.AdditionalProgress = 1;
            ext.MainProgress = 1;
            progress?.Invoke(ext);
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
                foreach (var item in layer.Values)
                {
                    oo.Add((float)(g.node(item)["height"]));
                }
                //var maxHeight = (layer as IEnumerable<object>).Select(v => g.node(v)["height"]).Max().Value;
                var maxHeight = oo.Max();
                foreach (var v in layer.Values)
                {
                    g.node(v)["y"] = prevY + maxHeight / 2f;
                }

                prevY += maxHeight + rankSep;
            }
            var list = bk.entries(bk.positionX(g));
            foreach (var item in list)
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
                    var lastKey1 = node["borderLeft"].Keys[node["borderLeft"].Keys.Count - 1];
                    var l = g.node(node["borderLeft"][lastKey1]);
                    var lastKey2 = node["borderRight"].Keys[node["borderRight"].Keys.Count - 1];
                    var r = g.node(node["borderRight"][lastKey2]);
                    node["width"] = Math.Abs(r["x"] - l["x"]);
                    node["height"] = Math.Abs(b["y"] - t["y"]);
                    node["x"] = l["x"] + node["width"] / 2;
                    node["y"] = t["y"] + node["height"] / 2;
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
                    node["maxRank"] = g.nodeRaw(node["borderBottom"])["rank"];
                    maxRank = Math.Max(maxRank, node["maxRank"]);
                }
            }

            g.graph()["maxRank"] = maxRank;
        }
    }
}
