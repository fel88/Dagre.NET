using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dagre
{
    public class util
    {        
        /*
         * Adjusts the ranks for all nodes in the graph such that all nodes v have
         * rank(v) >= 0 and at least one node w has rank(w) = 0.
         */
        public static void normalizeRanks(DagreGraph g)
        {
            int min = int.MaxValue;
            //var min = g.nodesRaw().Min(z => g.nodeRaw(z)["rank"]);
            foreach (var v in g.nodesRaw())
            {

                var node = g.nodeRaw(v);
                if (node.ContainsKey("rank"))
                {
                    var rank = node["rank"];
                    if (rank < min)
                    {
                        min = rank;
                    }
                    //node["rank"] -= min;
                }
            }
            foreach (var v in g.nodesRaw())
            {
                var node = g.nodeRaw(v);
                if (node.ContainsKey("rank"))
                {
                    node["rank"] -= min;
                }
            }
        }

        public static DagreGraph asNonCompoundGraph(DagreGraph g)
        {
            var graph = new DagreGraph(false) { _isMultigraph = g.isMultigraph() };
            graph.setGraph(g.graph());
            foreach (var v in g.nodesRaw())
            {
                if (g.children(v).Length == 0)
                {
                    graph.setNode(v, g.nodeRaw(v));
                }
            }

            foreach (var e in g.edgesRaw())
            {
                graph.setEdge(new object[] { e, g.edgeRaw(e) });
            }

            return graph;
        }

        public static int[] range(int start, int end)
        {
            return Enumerable.Range(start, end - start).ToArray();
        }
        public static string uniqueId(string str)
        {
            uniqueCounter++;
            return str + uniqueCounter;
        }
        public static int uniqueCounter = 0;

        /*
 * Adds a dummy node to the graph and return v.
 */
        public static string addDummyNode(DagreGraph g, string type, object attrs, string name)
        {
            string v = null;

            do
            {
                v = uniqueId(name);
            } while (g.hasNode(v));

            var dic = attrs as IDictionary<string, object>;
            DagreGraph.addOrUpdate("dummy", dic, type);

            g.setNode(v, attrs);
            return v;
        }

        public static int maxRank(DagreGraph g)
        {
            return g.nodes().Where(z => g.node(z)["rank"] != null).Select(z => g.node(z)["rank"]).Max();

        }
        /*
 * Returns a new graph with only simple edges. Handles aggregation of data
 * associated with multi-edges.
 */
        public static DagreGraph simplify(DagreGraph g)
        {
            DagreGraph simplified = new DagreGraph(false).setGraph(g.graph());
            foreach (var v in g.nodesRaw())
            {
                simplified.setNode(v, g.nodeRaw(v));
            }
            foreach (dynamic e in g.edgesRaw())
            {
                var r = simplified.edgeRaw(new[] { e["v"], e["w"] });
                JavaScriptLikeObject jo = new JavaScriptLikeObject();
                jo.Add("minlen", 1);
                jo.Add("weight", 0);
                dynamic simpleLabel = r == null ? jo : r;
                dynamic label = g.edgeRaw(e);
                JavaScriptLikeObject jo2 = new JavaScriptLikeObject();
                jo2.Add("weight", simpleLabel["weight"] + label["weight"]);
                jo2.Add("minlen", Math.Max(simpleLabel["minlen"], label["minlen"]));

                simplified.setEdge(new object[] { e["v"], e["w"], jo2 });
            }
            return simplified;
        }

     

        /*
* Given a DAG with each node assigned "rank" and "order" properties, this
* function will produce a matrix with the ids of each node.
*/
        public static object buildLayerMatrix(DagreGraph g)
        {
            var rank = maxRank(g);
            var range = Enumerable.Range(0, rank + 1);
            List<object> layering = new List<object>();
            foreach (var item in Enumerable.Range(0, rank + 1))
            {
                layering.Add(new JavaScriptLikeObject());
            }
            //var layering = _.map(_.range(maxRank(g) + 1), function() { return []; });

            var nd = g.nodes();
            //Array.Sort(nd, (x, y) => string.CompareOrdinal(x, y));
            foreach (var v in nd)
            {
                var node = g.node(v);
                var rank1 = node["rank"];
                if (rank1 != null)
                {
                    /*while (layering[rank].Count < node["order"])
                    {
                        layering[rank].Add(null);
                    }*/
                    layering[rank1]["" + node["order"]] = v;
                }
            }



            return layering;
        }

        internal static object addBorderNode(DagreGraph g, string v)
        {
            throw new NotImplementedException();
        }

        internal static int[] range(int v1, int v2, int step)
        {
            List<int> ret = new List<int>();
            for (int i = v1; i != v2; i += step)
            {
                ret.Add(i);
            }
            return ret.ToArray();
        }

        /*
         * Finds where a line starting at point ({x, y}) would intersect a rectangle
         * ({x, y, width, height}) if it were pointing at the rectangle's center.
         */
        internal static dynamic intersectRect(dynamic rect, dynamic point)
        {
            var x = rect["x"];
            var y = rect["y"];

            // Rectangle intersection algorithm from:
            // http://math.stackexchange.com/questions/108113/find-edge-between-two-boxes
            var dx = point["x"] - x;
            var dy = point["y"] - y;
            dynamic w = rect["width"];
            dynamic h = rect["height"];
            w = (float)w / 2f;
            h = (float)h / 2f;
            if (dx == null && dy == null)
            {
                throw new DagreException("Not possible to find intersection inside of the rectangle");
            }

            double sx, sy;
            if (Math.Abs(dy) * w > Math.Abs(dx) * h)
            {
                // Intersection is top or bottom of rect.
                if (dy < 0)
                {
                    h = -h;
                }
                sx = h * dx / (float)dy;
                sy = h;
            }
            else
            {
                // Intersection is left or right of rect.
                if (dx < 0)
                {
                    w = -w;
                }
                sx = w;
                sy = w * dy / (float)dx;
            }

            return DagreLayout.makePoint(x + sx, y + sy);
        }


    }
}
