using System;
using System.Collections.Generic;
using System.Linq;

namespace Dagre
{

    public class DagreInputGraph
    {
        List<DagreInputNode> nodes = new List<DagreInputNode>();
        List<DagreInputEdge> edges = new List<DagreInputEdge>();

        public bool VerticalLayout { get; set; } = true;

        public DagreInputNode GetNode(object tag)
        {
            return nodes.FirstOrDefault(z => z.Tag == tag);
        }
        public DagreInputEdge[] Edges()
        {
            return edges.ToArray();
        }
        public DagreInputNode[] Nodes()
        {
            return nodes.ToArray();
        }
        public static bool ExceptionOnDuplicateEdge = false;
        public static bool ExceptionOnReverseDuplicateEdge = true;
        public DagreInputEdge GetEdge(DagreInputNode from, DagreInputNode to)
        {
            return edges.First(z => z.From == from && z.To == to);
        }
        public DagreInputEdge AddEdge(DagreInputNode from, DagreInputNode to, int minLen = 1)
        {
            if (edges.Any(z => z.From == from && z.To == to))
            {
                var fr = GetEdge(from, to);
                if (ExceptionOnDuplicateEdge)
                    throw new DagreException("duplicate edge");
                else
                    return fr;
            }
            if (edges.Any(z => z.From == to && z.To == from))
            {
                var fr = GetEdge(to, from);
                if (ExceptionOnReverseDuplicateEdge)
                    throw new DagreException("reverse duplicate edge");
                else
                    return fr;

            }
            if (to.Parents.Contains(from)) throw new DagreException("duplciate parent");
            to.Parents.Add(from);
            if (from.Childs.Contains(to)) throw new DagreException("duplciate child");
            from.Childs.Add(to);
            var edge = new DagreInputEdge() { From = from, To = to, MinLen = minLen };
            edges.Add(edge);
            return edge;

        }


        public void AddNode(DagreInputNode node)
        {
            if (nodes.Contains(node)) throw new DagreException("duplciate node");
            nodes.Add(node);
        }
        public DagreInputNode AddNode(object tag = null, float? width = null, float? height = null)
        {
            var ret = new DagreInputNode();
            ret.Tag = tag;
            if (width != null && width.Value > 0)
                ret.Width = width.Value;
            if (height != null && height.Value > 0)
                ret.Height = height.Value;
            AddNode(ret);
            return ret;
        }
        public DagreInputGroup AddGroup(object tag = null)
        {
            var ret = new DagreInputGroup();
            ret.Tag = tag;

            AddNode(ret);
            return ret;
        }
        void check()
        {
            foreach (var item in nodes)
            {
                foreach (var ch in item.Childs)
                {

                }
            }
        }

        public void Layout(Action<ExtProgressInfo> progress = null)
        {
            check();
            DagreGraph dg = new DagreGraph(true);

            var list1 = nodes.Where(z => z is DagreInputGroup || z.Childs.Any() || z.Parents.Any()).ToList();

            foreach (var gg in list1.Where(z => !(z is DagreInputGroup)))
            {
                var ind = list1.IndexOf(gg);
                dg.setNode(ind + "", new JavaScriptLikeObject());
                var nd = dg.node(ind + "");

                nd["source"] = gg;
                nd["width"] = gg.Width;
                nd["height"] = gg.Height;
            }

            foreach (var gg in list1.Where(z => (z is DagreInputGroup)))
            {
                var ind = list1.IndexOf(gg);
                dg.setNode(ind + "", new JavaScriptLikeObject());
                var nd = dg.node(ind + "");

                nd["source"] = gg;


                nd["isGroup"] = true;
                nd["width"] = 0;
                nd["height"] = 0;


            }
            foreach (var gg in list1.Where(z => z.Group != null && !(z is DagreInputGroup)))
            {
                var ind = list1.IndexOf(gg);
                var nd = dg.node(ind.ToString());
                var ind2 = list1.IndexOf(gg.Group);
                dg.setParent(ind.ToString(), ind2.ToString());
            }

            foreach (var gg in list1)
            {
                var ind = list1.IndexOf(gg);

                foreach (var item in gg.Childs)
                {
                    var edge = edges.First(z => z.From == gg && z.To == item);
                    JavaScriptLikeObject jj = new JavaScriptLikeObject();

                    jj["minlen"] = edge.MinLen;

                    jj.Add("weight", 1);
                    jj.Add("width", 0);
                    jj.Add("height", 0);
                    jj.Add("labeloffset", 10);
                    jj.Add("labelpos", "r");
                    jj.Add("source", edge);
                    dg.setEdge(new object[] { ind + "", list1.IndexOf(item) + "", jj });
                }
            }
            dg.graph()["ranksep"] = 20;
            dg.graph()["edgesep"] = 20;
            dg.graph()["nodesep"] = 25;
            if (VerticalLayout)
                dg.graph()["rankdir"] = "tb";
            else
                dg.graph()["rankdir"] = "lr";
            DagreLayout.runLayout(dg, (f) =>
            {
                progress?.Invoke(f);
            });            

            //back
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = dg.node(i + "");
                var n = nodes[i];
                dynamic xx = node["x"];
                dynamic yy = node["y"];
                dynamic ww = node["width"];
                dynamic hh = node["height"];
                n.X = (float)xx - (float)ww / 2;
                n.Y = (float)yy - (float)hh / 2;
                n.Width = (float)ww;
                n.Height = (float)hh;

            }

            foreach (var item in dg.edges())
            {
                var edge = dg.edge(item);
                var src = edge["source"] as DagreInputEdge;
                dynamic pnts = edge["points"];
                List<DagreCurvePoint> rr = new List<DagreCurvePoint>();
                foreach (dynamic itemz in pnts)
                {
                    rr.Add(new DagreCurvePoint((float)itemz["x"], (float)itemz["y"]));
                }

                src.Points = rr.ToArray();
            }
        }

    public void SetGroup(DagreInputNode node, DagreInputNode dagreInputNode)
        {
            node.Group = dagreInputNode;
        }
    }
}
