using System;
using System.Collections.Generic;

namespace Dagre
{
    public class normalize
    {

        /*
         * Breaks any long edges in the graph into short segments that span 1 layer
         * each. This operation is undoable with the denormalize function.
         *
         * Pre-conditions:
         *
         *    1. The input graph is a DAG.
         *    2. Each node in the graph has a "rank" property.
         *
         * Post-condition:
         *
         *    1. All edges in the graph have a length of 1.
         *    2. Dummy nodes are added where edges have been split into segments.
         *    3. The graph is augmented with a "dummyChains" attribute which contains
         *       the first dummy in each chain of dummy nodes produced.
         */
        public static void run(DagreGraph g)
        {
            g.graph()["dummyChains"] = new List<string>();
            foreach (var edge in g.edgesRaw())
            {
                normalizeEdge(g, edge);
            }

        }

        /*denormalize
         */
        public static void undo(DagreGraph g, Action<float> progress = null)
        {
            var gg = g.graph();
            if (gg.ContainsKey("dummyChains"))
            {
                dynamic list = gg["dummyChains"];
                int i = 0;
                int count = 0;
                if (list is Array)
                {
                    count = list.Length;
                }
                else { count = list.Count; }
                foreach (var vv in list)
                {
                    var perc = (float)i / count;
                    progress?.Invoke(perc);
                    i++;
                    var v = vv;
                    var node = g.node(v);
                    var origLabel = node["edgeLabel"];
                    string w = null;
                    g.setEdge(new object[] { node["edgeObj"], origLabel });
                    while (node.ContainsKey("dummy") && node["dummy"] != null)
                    {
                        w = g.successors(v)[0];
                        g.removeNode(v);
                        if (!origLabel.ContainsKey("points"))
                        {
                            origLabel["points"] = new List<object>();
                        }
                        if (origLabel["points"] is Array ar1)
                        {
                            List<object> temp1 = new List<object>();
                            foreach (var item in ar1)
                            {
                                temp1.Add(item);
                            }
                            origLabel["points"] = temp1;
                        }
                        origLabel["points"].Add(DagreLayout.makePoint(node["x"], node["y"]));
                        if (node["dummy"] == "edge-label")
                        {
                            origLabel["x"] = node["x"];
                            origLabel["y"] = node["y"];
                            origLabel["width"] = node["width"];
                            origLabel["height"] = node["height"];
                        }
                        v = w;
                        node = g.node(v);
                    }
                }
            }

        }

        public static void normalizeEdge(DagreGraph g, dynamic e)
        {
            var v = e["v"];
            var vRank = (int)g.nodeRaw(v)["rank"];
            var w = e["w"];
            var wRank = (int)(g.nodeRaw(w)["rank"]);
            string name = null;
            if (e.ContainsKey("name"))
                name = (string)e["name"];
            var edgeLabel = g.edgeRaw(e);
            object labelRank = null;
            if (edgeLabel.ContainsKey("labelRank"))
                labelRank = edgeLabel["labelRank"];
            if (wRank != vRank + 1)
            {
                g.removeEdge(e);
                object dummy = null;
                //    let attrs;
                ++vRank;
                for (int i = 0; vRank < wRank; ++i, ++vRank)
                {
                    //        edgeLabel.points = [];

                    JavaScriptLikeObject attrs = new JavaScriptLikeObject();


                    attrs.Add("width", 0);
                    attrs.Add("height", 0);
                    attrs.Add("edgeLabel", edgeLabel);
                    attrs.Add("edgeObj", e);
                    attrs.Add("rank", vRank);
                    dummy = util.addDummyNode(g, "edge", attrs, "_d");
                    if (labelRank != null && vRank == (int)labelRank)
                    {
                        attrs["width"] = edgeLabel["width"];
                        attrs["height"] = edgeLabel["height"];
                        attrs["dummy"] = "edge-label";
                        attrs["labelpos"] = edgeLabel["labelpos"];
                    }
                    JavaScriptLikeObject jo1 = new JavaScriptLikeObject();
                    jo1.Add("weight", edgeLabel["weight"]);

                    g.setEdge(new object[] { v, (string)dummy, jo1, name });
                    if (i == 0)
                    {
                        g.graph()["dummyChains"].Add((string)dummy);
                    }
                    v = dummy;
                }
                JavaScriptLikeObject jo2 = new JavaScriptLikeObject();
                jo2.Add("weight", edgeLabel["weight"]);

                g.setEdge(new object[] { v, w, jo2, name });
            }
        }
    }

}
