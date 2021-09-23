using System;
using System.Collections.Generic;
using System.Linq;

namespace Dagre
{
    public static class parentDummyChains
    {

        static void dummyChainIteration(DagreGraph g, string v, object postorderNums)
        {
            var node = g.nodeRaw(v);
            var edgeObj = node["edgeObj"];
            var pathData = findPath(g, postorderNums, edgeObj["v"], edgeObj["w"]);
            var path = pathData["path"];
            var lca = pathData["lca"];
            var pathIdx = 0;
            var pathV = path[pathIdx];
            var ascending = true;
            while (v != edgeObj["w"])
            {
                node = g.nodeRaw(v);
                if (ascending)
                {
                    while ((pathV = path[pathIdx]) != lca && g.node(pathV)["maxRank"] < node["rank"])
                    {
                        pathIdx++;
                    }
                    if (pathV == lca)
                    {
                        ascending = false;
                    }
                }
                if (!ascending)
                {
                    while (pathIdx < (path as Array).Length - 1 && g.node(pathV = path[pathIdx + 1])["minRank"] <= node["rank"])
                    {
                        pathIdx++;
                    }
                    pathV = path[pathIdx];
                }
                g.setParent(v, pathV);
                v = g.successors(v)[0];
            }
        }
        public static void _parentDummyChains(DagreGraph g)
        {
            var postorderNums = postorder(g);
            var dummyChains = g.graph()["dummyChains"];
            if (dummyChains != null && dummyChains.Count > 0)
            {
                foreach (var v in dummyChains)
                {
                    dummyChainIteration(g, v, postorderNums);
                }
            }
        }

        // Find a path from v to w through the lowest common ancestor (LCA). Return the
        // full path and the LCA.
        public static object findPath(DagreGraph g, dynamic postorderNums, dynamic v, dynamic w)
        {
            List<object> vPath = new List<object>();
            List<object> wPath = new List<object>();
            var low = Math.Min((int)postorderNums[v]["low"], (int)postorderNums[w]["low"]);
            var lim = Math.Max((int)postorderNums[v]["lim"], (int)postorderNums[w]["lim"]);
            // Traverse up from v to find the LCA
            var parent = v;
            do
            {
                parent = g.parent((string)parent);
                vPath.Add(parent);
            }
            while (parent != null && (postorderNums[parent].low > low || lim > postorderNums[parent].lim));
            var lca = parent;
            // Traverse from w to LCA
            parent = w;
            while ((parent = g.parent((string)parent)) != lca)
            {
                wPath.Add(parent);
            }
            JavaScriptLikeObject jo = new JavaScriptLikeObject();
            wPath.Reverse();
            jo.Add("path", vPath.Union(wPath).ToArray());
            jo.Add("lca", lca);
            return jo;
            //return { path: vPath.concat(wPath.reverse()), lca: lca };
        }

        public static object postorder(DagreGraph g)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            var lim = 0;
            Action<string> dfs = null;
            dfs = (v) =>
            {
                var low = lim;
                foreach (var item in g.children(v))
                {
                    dfs(item);
                }
                JavaScriptLikeObject jo = new JavaScriptLikeObject();
                jo.Add("low", low);
                jo.Add("lim", lim++);
                result.Add(v, jo);
            };
            foreach (var item in g.children())
            {
                dfs(item);
            }


            return result;
        }

        public class Dto1
        {
            public int low;
            public int lim;
        }
    }

}
