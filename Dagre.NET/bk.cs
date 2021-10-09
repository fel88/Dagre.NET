using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Dagre
{
    public class bk
    {

        /*
               * Try to align nodes into vertical 'blocks' where possible. This algorithm
               * attempts to align a node with one of its median neighbors. If the edge
               * connecting a neighbor is a type-1 conflict then we ignore that possibility.
               * If a previous node has already formed a block with a node after the node
               * we're trying to form a block with, we also ignore that possibility - our
               * blocks would be split in that scenario.
               */
        public static dynamic verticalAlignment(DagreGraph g, dynamic layering, dynamic conflicts, dynamic neighborFnName)
        {


            dynamic root = new JavaScriptLikeObject();
            dynamic align = new JavaScriptLikeObject();
            dynamic pos = new JavaScriptLikeObject();
            // We cache the position here based on the layering because the graph and
            // layering may be out of sync. The layering matrix is manipulated to
            // generate different extreme alignments.
            foreach (var layer in layering)
            {
                int order = 0;
                foreach (dynamic v in layer)
                {
                    var str1 = (string)v;
                    root[str1] = str1;
                    align[str1] = str1;
                    pos[str1] = order;
                    order++;
                }
            }

            foreach (var layer in layering)
            {
                var prevIdx = -1;
                foreach (var v in layer)
                {
                    dynamic ws = null;
                    if (neighborFnName == "predecessors")
                    {
                        ws = g.predecessors(v);
                    }
                    else
                    {
                        ws = g.successors(v);
                    }
                    if (ws.Length != 0)
                    {
                        var temp1 = (ws as string[]);
                        if (temp1.Length > 1)
                        {

                        }
                        Array.Sort(temp1, (a, b) => (int)((pos)[a]) - (int)(pos[b]));
                        //ws = ws.sort((a, b) => pos[a] - pos[b]);
                        double mp = (ws.Length - 1) / 2.0;
                        for (int i = (int)Math.Floor(mp), il = (int)Math.Ceiling(mp); i <= il; ++i)
                        {
                            var w = ws[i];
                            if (align[v] == v && prevIdx < pos[w] && !hasConflict(conflicts, v, w))
                            {
                                align[w] = v;
                                align[v] = root[v] = root[w];
                                prevIdx = pos[w];
                            }
                        }
                    }
                }
            }
            JavaScriptLikeObject ret = new JavaScriptLikeObject();
            ret.Add("root", root);
            ret.Add("align", align);
            return ret;
        }


        public static List<object> entries(dynamic oo)
        {
            var ret = new List<object>();
            foreach (var item in oo.Keys)
            {
                ret.Add(new object[] { item, oo[item] });
            }
            return ret;
        }
        public static dynamic keys(dynamic oo)
        {
            dynamic ret = new List<object>();
            foreach (var item in oo.Keys)
            {
                ret.Add(item);

            }
            return ret;
        }
        public static dynamic buildBlockGraph(DagreGraph g, dynamic layering, dynamic root, dynamic reverseSep)
        {
            Func<dynamic, dynamic, dynamic, dynamic> sep = (nodeSep, edgeSep, _reverseSep) =>
            {
                Func<dynamic, dynamic, dynamic, dynamic> ret = (dynamic _g, dynamic v, dynamic w) =>
                {
                    dynamic vLabel = g.node(v);
                    dynamic wLabel = g.node(w);
                    dynamic sum = 0;
                    dynamic delta = null;
                    sum += (float)vLabel["width"] / 2f;
                    if (vLabel.ContainsKey("labelpos"))
                    {
                        switch (vLabel["labelpos"].ToLower())
                        {
                            case "l": delta = -(float)vLabel["width"] / 2f; break;
                            case "r": delta = (float)vLabel["width"] / 2f; break;
                        }
                    }
                    if (delta != null && delta != 0)
                    {
                        sum += reverseSep ? delta : -delta;
                    }
                    delta = 0;
                    sum += (vLabel.ContainsKey("dummy") ? edgeSep : nodeSep) / 2f;
                    sum += (wLabel.ContainsKey("dummy") ? edgeSep : nodeSep) / 2f;
                    sum += (float)wLabel["width"] / 2f;
                    if (wLabel.ContainsKey("labelpos"))
                    {
                        switch (wLabel["labelpos"].ToLower())
                        {
                            case "l": delta = (float)wLabel["width"] / 2f; break;
                            case "r": delta = -(float)wLabel["width"] / 2f; break;
                        }
                    }
                    if (delta != null && delta != 0)
                    {
                        sum += reverseSep ? delta : -delta;
                    }
                    delta = 0;
                    return sum;
                };
                return ret;
            };
            var blockGraph = new DagreGraph(false);
            var graphLabel = g.graph();
            var sepFn = sep(graphLabel["nodesep"], graphLabel["edgesep"], reverseSep);

            foreach (var layer in layering)
            {
                dynamic u = null;
                foreach (var v in layer)
                {
                    var vRoot = root[v];
                    blockGraph.setNode(vRoot);
                    if (u != null)
                    {
                        var uRoot = root[u];
                        var prevMax = blockGraph.edgeRaw(new object[] { uRoot, vRoot });
                        blockGraph.setEdge(new object[] {
                            uRoot,
                            vRoot,
                            Math.Max(sepFn(g, v, u), (prevMax != null ? prevMax : 0))});
                    }
                    u = v;
                }
            }

            return blockGraph;


        }


        public static dynamic horizontalCompaction(DagreGraph g, dynamic layering, dynamic root, dynamic align, bool reverseSep)
        {
            // This portion of the algorithm differs from BK due to a number of problems.
            // Instead of their algorithm we construct a new block graph and do two
            // sweeps. The first sweep places blocks with the smallest possible
            // coordinates. The second sweep removes unused space by moving blocks to the
            // greatest coordinates without violating separation.
            dynamic xs = new JavaScriptLikeObject();
            //if (!DagreGraph.FromJson(util.ReadResourceTxt("beforeHorizontalCompaction.txt")).Compare(g)) throw new DagreException("wrong");


            DagreGraph blockG = buildBlockGraph(g, layering, root, reverseSep) as DagreGraph;
            // if (!DagreGraph.FromJson(util.ReadResourceTxt("blockGtemp1.txt")).Compare(blockG)) throw new DagreException("wrong");

            var borderType = reverseSep ? "borderLeft" : "borderRight";
            Action<dynamic, dynamic> iterate = (setXsFunc, nextNodesFunc) =>
            {
                dynamic stack = blockG.nodes().ToList();
                dynamic elem = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                dynamic visited = new JavaScriptLikeObject();
                while (elem != null)
                {
                    if (visited.ContainsKey(elem))
                    {
                        setXsFunc(elem);
                    }
                    else
                    {
                        visited[elem] = true;
                        stack.Add(elem);
                        dynamic temp1 = null;
                        if (nextNodesFunc == "predecessors")
                            temp1 = blockG.predecessors(elem);
                        if (nextNodesFunc == "successors")
                            temp1 = blockG.successors(elem);

                        foreach (var item in temp1)
                        {
                            stack.Add(item);
                        }
                        //stack = stack.concat(temp1);
                    }
                    if (stack.Count == 0) break;
                    elem = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                }
            };
            //// First pass, assign smallest coordinates
            Action<dynamic> pass1 = (elem) =>
            {
                dynamic temp1 = blockG.inEdges(elem) as Array;
                dynamic acc = 0;

                for (int i = 0; i < temp1.Length; i++)
                {
                    dynamic e = temp1[i];
                    acc = Math.Max(acc, xs[e["v"]] + blockG.edge(e));
                }
                //xs[elem] = temp1.reduce((acc, e) => Math.Max(acc, xs[e.v] + blockG.edge(e)), 0);
                xs[elem] = acc;
            };
            //// Second pass, assign greatest coordinates
            Action<dynamic> pass2 = (elem) =>
            {
                var temp1 = blockG.outEdges(elem) as object[];

                dynamic acc = float.PositiveInfinity;
                dynamic len = temp1.Length;
                for (int i = 0; i < temp1.Length; i++)
                {
                    dynamic e = temp1[i];
                    acc = Math.Min(acc, xs[e["w"]] - blockG.edge(e));
                }
                dynamic min = acc;
                dynamic node = g.node(elem);
                string nb = null;
                if (node.ContainsKey("borderType"))
                {
                    nb = node["borderType"];
                }
                if (min != float.PositiveInfinity && nb != borderType)
                {
                    xs[elem] = Math.Max(xs[elem], min);
                }
            };
            //iterate(pass1, blockG.predecessors.bind(blockG));
            //iterate(pass2, blockG.successors.bind(blockG));
            iterate(pass1, "predecessors");
            iterate(pass2, "successors");
            // Assign x coordinates to all nodes
            foreach (var v in values(align))
            {
                xs[v] = xs[root[v]];
            }
            return xs;
        }

        public static dynamic values(dynamic v)
        {
            if (v is Array) return v;
            List<object> ret = new List<object>();
            if (v is IDictionary<string, object>)
            {
                foreach (var item in v.Values)
                {
                    ret.Add(item);
                }
            }
            else
            foreach (var item in v)
            {

                if (item is Array)
                {
                    List<object> ff = new List<object>();
                    foreach (var zz in item)
                    {
                        ff.Add(zz);
                    }
                    ret.Add(ff.ToArray());

                }
                else
                        ret.Add(item);
            }
            return ret;
        }

        // Returns the alignment that has the smallest width of the given alignments.
        public static dynamic findSmallestWidthAlignment(DagreGraph g, dynamic xss)
        {
            float minKey = float.PositiveInfinity;
            dynamic minValue = null;
            foreach (var xs in values(xss))
            {
                var max = float.NegativeInfinity;
                var min = float.PositiveInfinity;
                foreach (var entry in entries(xs))
                {
                    dynamic v = entry[0];
                    dynamic x = entry[1];
                    dynamic halfWidth = (float)((dynamic)g.node(v)["width"]) / 2f;
                    max = Math.Max(x + halfWidth, max);
                    min = Math.Min(x - halfWidth, min);
                }
                dynamic key = max - min;
                if (key < minKey)
                {
                    minKey = key;
                    minValue = xs;
                }
            }
            return minValue;
        }

        public static dynamic balance(dynamic xss, dynamic align)
        {
            dynamic value = new JavaScriptLikeObject();
            if (align != null)
            {
                dynamic xs = xss[align.toLowerCase()];
                foreach (dynamic v in keys(xss["ul"]))
                {
                    value[v] = xs[v];
                }
            }
            else
            {
                foreach (dynamic v in keys(xss["ul"]))
                {
                    var xs = new float[] { xss["ul"][v], xss["ur"][v], xss["dl"][v], xss["dr"][v] };
                    Array.Sort(xs, (a, b) => a.CompareTo(b));
                    value[v] = (xs[1] + xs[2]) / 2f;
                }
            }
            return value;
        }

        /*
               * Align the coordinates of each of the layout alignments such that
               * left-biased alignments have their minimum coordinate at the same point as
               * the minimum coordinate of the smallest width alignment and right-biased
               * alignments have their maximum coordinate at the same point as the maximum
               * coordinate of the smallest width alignment.
               */
        public static void alignCoordinates(dynamic xss, dynamic alignTo)
        {
            Func<dynamic, dynamic> range = (values) =>
            {
                dynamic min = int.MaxValue;
                dynamic max = int.MinValue;
                foreach (dynamic value in values)
                {
                    if (value < min)
                    {
                        min = value;
                    }
                    if (value > max)
                    {
                        max = value;
                    }
                }
                return new[] { min, max };
            };

            dynamic alignToRange = range(values(alignTo));
            foreach (dynamic vert in new[] { "u", "d" })
            {
                foreach (dynamic horiz in new[] { "l", "r" })
                {
                    dynamic alignment = vert + horiz;
                    dynamic xs = xss[alignment];
                    dynamic delta;
                    if (xs != alignTo)
                    {
                        dynamic vsValsRange = range(values(xs));
                        delta = horiz == "l" ? alignToRange[0] - vsValsRange[0] : alignToRange[1] - vsValsRange[1];
                        if (delta != 0)
                        {
                            dynamic list = new JavaScriptLikeObject();
                            foreach (dynamic key in keys(xs))
                            {
                                list[key] = xs[key] + delta;
                            }
                            xss[alignment] = list; //_.mapValues(xs, function(x) { return x + delta; });
                        }
                    }
                }
            }
        }
        /*
        * This module provides coordinate assignment based on Brandes and Köpf, "Fast
        * and Simple Horizontal Coordinate Assignment."
        */

        public static dynamic positionX(DagreGraph g)
        {
            dynamic layering = util.buildLayerMatrix(g);
            ////// Dic-> Array
            List<object> toArr = new List<object>();
            foreach (dynamic item in layering)
            {
                var d = item as IDictionary<string, object>;
                List<object> arr2 = new List<object>();
                foreach (var zitem in item.Values)
                {
                    arr2.Add(zitem);
                }

                toArr.Add(arr2.ToArray());
            }
            layering = toArr;
            //////

            dynamic conflicts1 = findType1Conflicts(g, layering);
            dynamic conflicts2 = findType2Conflicts(g, layering);
            dynamic conflicts = new JavaScriptLikeObject();
            foreach (var item in conflicts1)
            {
                conflicts.Add(item);
            }
            foreach (var item in conflicts2)
            {
                conflicts.Add(item);
            }

            JavaScriptLikeObject xss = new JavaScriptLikeObject();
            foreach (var vert in new[] { "u", "d" })
            {
                dynamic adjustedLayering = null;
                if (vert == "u")
                {
                    adjustedLayering = layering;
                }
                else
                {
                    adjustedLayering = values(layering);
                    adjustedLayering.Reverse();
                }

                foreach (var horiz in new[] { "l", "r" })
                {
                    if (horiz == "r")
                    {
                        //adjustedLayering = adjustedLayering.Select((inner) => values(inner).reverse());
                        List<object[]> rett = new List<object[]>();
                        foreach (var item in adjustedLayering)
                        {
                            List<object> ret1 = new List<object>();
                            foreach (var ree in values(item))
                            {
                                ret1.Add(ree);
                            }
                            ret1.Reverse();
                            rett.Add(ret1.ToArray());
                        }
                        adjustedLayering = rett.ToArray();
                    }
                    var neighborFn = (vert == "u" ? "predecessors" : "successors");
                    var align = verticalAlignment(g, adjustedLayering, conflicts, neighborFn);
                    var xs = horizontalCompaction(g, adjustedLayering, align["root"], align["align"], horiz == "r");
                    if (horiz == "r")
                    {
                        foreach (var entry in entries(xs))
                        {
                            xs[entry[0]] = -entry[1];
                        }
                    }
                    xss[vert + horiz] = xs;
                }
            }
          
            dynamic smallestWidth = findSmallestWidthAlignment(g, xss);

            alignCoordinates(xss, smallestWidth);
            dynamic _align = null;
            if (g.graph().ContainsKey("align"))
            {
                _align = g.graph()["align"];
            }
          
           

            return balance(xss, _align);
        }
        public static bool hasConflict(dynamic conflicts, dynamic v, dynamic w)
        {
            if (string.CompareOrdinal(v, w) > 0)//v>w
            {
                var tmp = v;
                v = w;
                w = tmp;
            }

            return conflicts.ContainsKey(v) && conflicts[v].ContainsKey(w);
        }
        public static void addConflict(dynamic conflicts, dynamic v, dynamic w)
        {
            if (string.CompareOrdinal(v, w) == 1)
            {
                var tmp = v;
                v = w;
                w = tmp;
            }
            dynamic conflictsV = null;
            if (conflicts.ContainsKey(v) && conflicts[v] == null)
            {
                conflicts[v] = new JavaScriptLikeObject();

            }
            if (!conflicts.ContainsKey(v))
            {
                conflicts.Add((string)v, new JavaScriptLikeObject());

            }
            conflictsV = conflicts[v];
            conflictsV[w] = true;
        }

        public static dynamic slice(dynamic input, int start, int len)
        {
            int cnt = 0;
            List<object> ret = new List<object>();
            for (int i = start; i < input.Length; i++)
            {
                ret.Add(input[i]);
                cnt++;
                if (cnt == len) break;
            }
            return ret;
        }
        public static string findOtherInnerSegmentNode(DagreGraph g, string v)
        {
            if (g.nodeRaw(v).ContainsKey("dummy"))
            {
                return g.predecessors(v).FirstOrDefault(u => g.nodeRaw(u).ContainsKey("dummy"));
            }
            return null;
        }
        /*
         * Marks all edges in the graph with a type-1 conflict with the "type1Conflict"
         * property. A type-1 conflict is one where a non-inner segment crosses an
         * inner segment. An inner segment is an edge with both incident nodes marked
         * with the "dummy" property.
         *
         * This algorithm scans layer by layer, starting with the second, for type-1
         * conflicts between the current layer and the previous layer. For each layer
         * it scans the nodes from left to right until it reaches one that is incident
         * on an inner segment. It then scans predecessors to determine if they have
         * edges that cross that inner segment. At the end a final scan is done for all
         * nodes on the current rank to see if they cross the last visited inner
         * segment.
         *
         * This algorithm (safely) assumes that a dummy node will only be incident on a
         * single node in the layers being scanned.
         */


        public static object findType1Conflicts(DagreGraph g, dynamic layering)
        {
            Dictionary<string, object> conflicts = new Dictionary<string, object>();

            Func<dynamic, dynamic, dynamic> visitLayer = (prevLayer, layer) =>
               {
                   // last visited node in the previous layer that is incident on an inner
                   // segment.
                   int?

        k0 = 0;
                   // Tracks the last node in this layer scanned for crossings with a type-1
                   // segment.
                   var scanPos = 0;
                   var prevLayerLength = prevLayer.Length;
                   var lastNode = layer[layer.Length - 1];

                   foreach (var v in layer)
                   {
                       var w = findOtherInnerSegmentNode(g, v);
                       var k1 = w != null ? g.node(w)["order"] : prevLayerLength;

                       int i = 0;
                       if (w != null || v == lastNode)
                       {
                           foreach (var scanNode in slice(layer, scanPos, i + 1))
                           {
                               foreach (var u in g.predecessors(scanNode))
                               {
                                   var uLabel = g.node(u);
                                   var uPos = uLabel["order"];
                                   if ((uPos < k0 || k1 < uPos) &&
                                   !(uLabel.ContainsKey("dummy") && g.node(scanNode).ContainsKey("dummy")))
                                   {
                                       addConflict(conflicts, u, scanNode);
                                   }
                               }
                               scanPos = i + 1;
                               k0 = k1;
                           }
                       }
                   }


                   return layer;
               };

            if (layering.Count > 0)
            {
                /*List<object> toArr = new List<object>();
                foreach (dynamic item in layering)
                {
                    var d = item as IDictionary<string, object>;
                    List<object> arr2 = new List<object>();
                    foreach (var zitem in item.Values)
                    {
                        arr2.Add(zitem);
                    }
                    //var fr = (item.Values as IEnumerable<object>).First();
                    toArr.Add(arr2.ToArray());
                }*/
                dynamic prev = layering[0];
                bool first = true;
                foreach (var item in layering)
                {
                    if (first) { first = false; continue; }
                    prev = visitLayer(prev, item);
                    //prev = item;
                }
            }
            return conflicts;
            //return conflicts.ToArray();
        }


        public static object findType2Conflicts(DagreGraph g, dynamic layering)
        {

            Dictionary<string, object> conflicts = new Dictionary<string, object>();
            Action<dynamic, dynamic, dynamic, dynamic, dynamic> scan = (south, southPos, southEnd, prevNorthBorder, nextNorthBorder) =>
              {
                  dynamic v = null;
                  for (var i = southPos; i < southEnd; i++)
                  {
                      v = south[i];
                      if (g.node(v).ContainsKey("dummy"))
                      {
                          foreach (var u in g.predecessors(v))
                          {
                              var uNode = g.node(u);
                              if (uNode.ContainsKey("dummy") && (uNode["order"] < prevNorthBorder || uNode["order"] > nextNorthBorder))
                              {
                                  addConflict(conflicts, u, v);
                              }
                          }
                      }
                  }


              };

            Func<dynamic, dynamic, dynamic> visitLayer = (north, south) =>
            {
                var prevNorthPos = -1;
                dynamic nextNorthPos = null;
                dynamic southPos = 0;
                object southLookahead = null;
                foreach (var v in south)
                {
                    var nd = g.node(v);
                    if (nd.ContainsKey("dummy") && nd["dummy"] == "border")
                    {
                        var predecessors = g.predecessors(v);
                        if (predecessors.Length != 0)
                        {
                            nextNorthPos = g.node(predecessors[0])["order"];
                            scan(south, southPos, southLookahead, prevNorthPos, nextNorthPos);
                            southPos = southLookahead;
                            prevNorthPos = nextNorthPos;
                        }
                    }
                    scan(south, southPos, south.Length, nextNorthPos, north.Length);
                }


                return south;
            };

            if (layering.Count > 0)
            {
                /*List<object> toArr = new List<object>();
                foreach (dynamic item in layering)
                {
                    var d = item as IDictionary<string, object>;
                    List<object> arr2 = new List<object>();
                    foreach (var zitem in item.Values)
                    {
                        arr2.Add(zitem);
                    }

                    toArr.Add(arr2.ToArray());
                }*/
                dynamic prev = layering[0];
                bool first = true;
                foreach (var item in layering)
                {
                    if (first) { first = false; continue; }
                    prev = visitLayer(prev, item);
                }
            }
            return conflicts;
            //return conflicts.ToArray();
        }
    }
}
