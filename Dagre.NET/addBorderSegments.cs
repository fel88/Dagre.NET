using System;
using System.Collections.Generic;

namespace Dagre
{
    public class addBorderSegments
    {

        public static void addBorderNode(DagreGraph g, string prop, string prefix, dynamic sg, dynamic sgNode, int rank)
        {
            var label = new JavaScriptLikeObject();
            label.Add("width", 0);
            label.Add("height", 0);
            label.Add("rank", rank);
            label.Add("borderType", prop);

            dynamic prev = null;
            if (sgNode.ContainsKey(prop) && sgNode[prop] != null)
            {
                if (sgNode[prop].ContainsKey((rank - 1).ToString()))
                    prev = sgNode[prop][(rank - 1).ToString()];
            }
        var curr = util.addDummyNode(g, "border", label, prefix);
            sgNode[prop][rank.ToString()] = curr;
  g.setParent(curr, sg);
            if (prev != null)
            {
                JavaScriptLikeObject j1 = new JavaScriptLikeObject();
                j1.Add("weight", 1);
                g.setEdge(new object[] { prev, curr, j1 });
            }
        }


        public static void _addBorderSegments(DagreGraph g)
        {
            Action<string> dfs = null;
            dfs = (v) =>
           {
               var children = g.children(v);
               var node = g.nodeRaw(v);
               if (children != null && children.Length > 0)
               {
                   foreach (var item in children)
                   {
                       dfs(item);
                   }
               }

               if (node.ContainsKey("minRank"))
               {
                   node["borderLeft"] = new JavaScriptLikeObject();
                   node["borderRight"] = new JavaScriptLikeObject();
                   for (int rank = node["minRank"], maxRank = node["maxRank"] + 1; rank < maxRank;
                     ++rank)
                   {
                       addBorderNode(g, "borderLeft", "_bl", v, node, rank);
                       addBorderNode(g, "borderRight", "_br", v, node, rank);
                   }
               }
           };

            foreach (var item in g.children())
            {
                dfs(item);
            }
        }
    }
}
