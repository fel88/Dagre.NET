using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dagre
{
    public class sortSubGraphModule
    {
        public static object barycenter(DagreGraph g, string[] movable)
        {
            return movable.Select(v =>
            {
                var inV = g.inEdges(v);
                if (inV.Length == 0)
                {
                    JavaScriptLikeObject ret = new JavaScriptLikeObject();
                    ret.Add("v", v);
                    return ret;
                }
                else
                {
                    JavaScriptLikeObject inp = new JavaScriptLikeObject();
                    inp.Add("sum", 0);
                    inp.Add("weight", 0);
                    var result = inV.Aggregate(inp, (acc, e) =>
                       {
                           var edge = g.edge(e);
                           var nodeU = g.node(((dynamic)e)["v"]);
                           JavaScriptLikeObject ret2 = new JavaScriptLikeObject();
                           ret2.Add("sum", (dynamic)acc["sum"] + ((dynamic)edge["weight"] * (dynamic)nodeU["order"]));
                           ret2.Add("weight", (dynamic)acc["weight"] + (dynamic)edge["weight"]);
                           return ret2;
                       });
                    JavaScriptLikeObject ret3 = new JavaScriptLikeObject();
                    ret3.Add("v", v);
                    ret3.Add("barycenter", (dynamic)result["sum"] / (dynamic)result["weight"]);
                    ret3.Add("weight", result["weight"]);

                    return ret3;


                }
            }).ToArray();

        }
        public static void mergeBarycenters(dynamic target, dynamic other)
        {
            if (target.ContainsKey("barycenter"))
            {
                target["barycenter"] = (target["barycenter"] * target["weight"] + other["barycenter"] * other["weight"]) / (target["weight"] + other["weight"]);
                target["weight"] += other["weight"];
            }
            else
            {
                target["barycenter"] = other["barycenter"];
                target["weight"] = other["weight"];
            }
        }
        public static object sortSubraph(DagreGraph g, string v, DagreGraph cg, bool biasRight)
        {
            var movable = g.children(v);
            var node = g.node(v);
            var bl = node != null ? node["borderLeft"] : null;
            var br = node != null ? node["borderRight"] : null;
            dynamic subgraphs = new Dictionary<string, object>();

            if (bl != null)
            {
                movable = movable.Where(z => z != bl && z != br).ToArray();
            }

            dynamic barycenters = barycenter(g, movable);
            foreach (dynamic entry in barycenters)
            {
                if (g.children(entry["v"]).Length != 0)
                {
                    var subgraphResult = sortSubGraphModule.sortSubraph(g, entry["v"], cg, biasRight);
                    subgraphs[entry["v"]] = subgraphResult;
                    if (subgraphResult.ContainsKey("barycenter"))
                    {
                        mergeBarycenters(entry, subgraphResult);
                    }
                }
            }


            dynamic entries = resolveConflictsModule.resolveConflicts(barycenters, cg);
            // expand subgraphs
            foreach (dynamic entry in entries)
            {
                dynamic v1 = (entry["vs"]);
                List<object> rett = new List<object>();
                foreach (var item in v1)
                {
                    if (subgraphs.ContainsKey(item))
                    {
                        var a1 = subgraphs[item]["vs"];
                        if (a1 is Array || a1 is IList)
                        {
                            rett.AddRange(a1);
                        }
                        else
                            rett.Add(a1);
                    }
                    else
                    {
                        if (item is Array || item is IList)
                        {
                            rett.AddRange(item);
                        }
                        else
                        rett.Add(item);
                    }

                }
                entry["vs"] = rett.ToArray();

                /*entry["vs"] = v1.Select((v2) =>
                 {

                    //subgraphs[v1] != null ? subgraphs[v1]["vs"] : v1
                    return null;

                 });*/
                /* if (Array.isArray(entry.vs) && entry.vs.length !== 1 || Array.isArray(entry.vs[0]))
                 {
                     entry.vs = entry.vs.flat();
                 }*/
            }

            dynamic result = sort(entries, biasRight);
            if (bl != null)
            {
                //result.vs = [bl, result.vs, br].flat();
                List<object> ll = new List<object>();
                ll.Add(bl);
                ll.AddRange(result["vs"]);
                ll.Add(br);
                result["vs"] = ll;
                if (g.predecessors(bl).Length != 0)
                {
                    var blPred = g.node(g.predecessors(bl)[0]);
                    var brPred = g.node(g.predecessors(br)[0]);
                    if (!(result.ContainsKey("barycenter")))
                    {
                        result["barycenter"] = 0;
                        result["weight"] = 0;
                    }
                    result["barycenter"] = (result["barycenter"] * result["weight"] + blPred["order"] + brPred["order"]) / (result["weight"] + 2);
                    result["weight"] += 2;
                }
            }
            /*var ar1 = (result["vs"] as List<object>);
            ar1.Sort((x, y) => string.CompareOrdinal((string)x, (string)y));
            result["vs"] = ar1;*/
            return result;
        }
        public static dynamic consumeUnsortable(dynamic vs, dynamic unsortable, dynamic index)
        {
            dynamic last = null;
            while (unsortable.Count != 0 && (last = unsortable[unsortable.Count - 1])["i"] <= index)
            {
                unsortable.RemoveAt(unsortable.Count - 1);
                vs.Add(last["vs"]);
                index++;
            }
            return index;
        }
        public static dynamic compareWithBias(dynamic bias)
        {
            Func<dynamic, dynamic, dynamic> ret = (entryV, entryW) =>
             {
                 if (entryV.barycenter < entryW.barycenter)
                 {
                     return -1;
                 }
                 else if (entryV.barycenter > entryW.barycenter)
                 {
                     return 1;
                 }
                 return !bias ? entryV.i - entryW.i : entryW.i - entryV.i;
             };
            return ret;

        }
        public static object sort(dynamic entries, dynamic biasRight)
        {
            // partition
            JavaScriptLikeObject parts = new JavaScriptLikeObject();
            parts.Add("lhs", new List<object>());
            parts.Add("rhs", new List<object>());
            foreach (var value in entries)
            {
                if (value.ContainsKey("barycenter"))
                {
                    ((dynamic)parts["lhs"]).Add(value);
                }
                else
                {
                    ((dynamic)parts["rhs"]).Add(value);
                }
            }
            List<object> sortable = (dynamic)parts["lhs"];
            List<object> unsortable = (dynamic)(parts["rhs"]);

            unsortable.Sort((a, b) => -((dynamic)a)["i"] + ((dynamic)b)["i"]);
            dynamic vs = new List<object>();
            dynamic sum = 0;
            dynamic weight = 0;
            int vsIndex = 0;
            sortable.Sort((entryV, entryW) =>
            {
                dynamic v = ((dynamic)entryV)["barycenter"];
                dynamic w = ((dynamic)entryW)["barycenter"];
                if (v < w)
                {
                    return -1;
                }
                else if (v > w)
                {
                    return 1;
                }
                return !biasRight ? (((dynamic)entryV)["i"] - ((dynamic)entryW)["i"] ): (((dynamic)entryW)["i"] - ((dynamic)entryV)["i"]);
            });
            

            //sortable.sort(compareWithBias(!!biasRight));
            vsIndex = consumeUnsortable(vs, unsortable, vsIndex);
            foreach (dynamic entry in sortable)
            {
                vsIndex += entry["vs"].Length;
                vs.Add(entry["vs"]);
                sum += entry["barycenter"] * entry["weight"];
                weight += (dynamic)entry["weight"];
                vsIndex = consumeUnsortable(vs, unsortable, vsIndex);
            }
            JavaScriptLikeObject result = new JavaScriptLikeObject();
            List<object> rr = new List<object>();
            foreach (var i1 in vs)
            {
                foreach (var i2 in i1)
                {
                    rr.Add(i2);
                }
            }
            //result.Add("vs", vs.flat());
            result.Add("vs", rr);

            if (weight != null && weight != 0)
            {
                result["barycenter"] = sum / (float)weight;
                result["weight"] = weight;
            }
            return result;
        }
        /*
         * 
function sortSubgraph(g, v, cg, biasRight) {
  var movable = g.children(v);
  var node = g.node(v);
  var bl = node ? node.borderLeft : undefined;
  var br = node ? node.borderRight: undefined;
  var subgraphs = {};

  if (bl) {
    movable = _.filter(movable, function(w) {
      return w !== bl && w !== br;
    });
  }

  var barycenters = barycenter(g, movable);
  _.forEach(barycenters, function(entry) {
    if (g.children(entry.v).length) {
      var subgraphResult = sortSubgraph(g, entry.v, cg, biasRight);
      subgraphs[entry.v] = subgraphResult;
      if (_.has(subgraphResult, "barycenter")) {
        mergeBarycenters(entry, subgraphResult);
      }
    }
  });

  var entries = resolveConflicts(barycenters, cg);
  expandSubgraphs(entries, subgraphs);

  var result = sort(entries, biasRight);

  if (bl) {
    result.vs = _.flatten([bl, result.vs, br], true);
    if (g.predecessors(bl).length) {
      var blPred = g.node(g.predecessors(bl)[0]),
        brPred = g.node(g.predecessors(br)[0]);
      if (!_.has(result, "barycenter")) {
        result.barycenter = 0;
        result.weight = 0;
      }
      result.barycenter = (result.barycenter * result.weight +
                           blPred.order + brPred.order) / (result.weight + 2);
      result.weight += 2;
    }
  }

  return result;
}

function expandSubgraphs(entries, subgraphs) {
  _.forEach(entries, function(entry) {
    entry.vs = _.flatten(entry.vs.map(function(v) {
      if (subgraphs[v]) {
        return subgraphs[v].vs;
      }
      return v;
    }), true);
  });
}

function mergeBarycenters(target, other) {
  if (!_.isUndefined(target.barycenter)) {
    target.barycenter = (target.barycenter * target.weight +
                         other.barycenter * other.weight) /
                        (target.weight + other.weight);
    target.weight += other.weight;
  } else {
    target.barycenter = other.barycenter;
    target.weight = other.weight;
  }
}

         */
    }
}
