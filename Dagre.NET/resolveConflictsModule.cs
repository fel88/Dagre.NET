using System;
using System.Collections.Generic;
using System.Linq;

namespace Dagre
{
    public class resolveConflictsModule
    {

        /*
         * Given a list of entries of the form {v, barycenter, weight} and a
         * constraint graph this function will resolve any conflicts between the
         * constraint graph and the barycenters for the entries. If the barycenters for
         * an entry would violate a constraint in the constraint graph then we coalesce
         * the nodes in the conflict into a new node that respects the contraint and
         * aggregates barycenter and weight information.
         *
         * This implementation is based on the description in Forster, "A Fast and
         * Simple Hueristic for Constrained Two-Level Crossing Reduction," thought it
         * differs in some specific details.
         *
         * Pre-conditions:
         *
         *    1. Each entry has the form {v, barycenter, weight}, or if the node has
         *       no barycenter, then {v}.
         *
         * Returns:
         *
         *    A new list of entries of the form {vs, i, barycenter, weight}. The list
         *    `vs` may either be a singleton or it may be an aggregation of nodes
         *    ordered such that they do not violate constraints from the constraint
         *    graph. The property `i` is the lowest original index of any of the
         *    elements in `vs`.
         */

        public class resolveDto
        {
            public int i;
            public int? indegree;
            public string[] _in;
            public string[] _out;
            public string[] vs;
            public int barycenter;
            public int weight;
            internal bool? merged;
        }
        public static void mergeEntries(dynamic target, dynamic source)
        {
            var sum = 0;
            var weight = 0;
            if (target.ContainsKey("weight") && target["weight"] != 0)
            {
                sum += target["barycenter"] * target["weight"];
                weight += target["weight"];
            }
            if (source.ContainsKey("weight") && source["weight"] != 0)
            {
                sum += source["barycenter"] * source["weight"];
                weight += source["weight"];
            }
            target["vs"] = source["vs"].Union(target["vs"]);
            target["barycenter"] = sum / weight;
            target["weight"] = weight;
            target["i"] = Math.Min(source["i"], target["i"]);
            source["merged"] = true;
        }

        public static object resolveConflicts(dynamic entities, DagreGraph cg)
        {
            Dictionary<string, object> mappedEntries = new Dictionary<string, object>();
            for (int i = 0; i < entities.Length; i++)
            {
                dynamic entry = entities[i];

                JavaScriptLikeObject tmp = new JavaScriptLikeObject();
                tmp.Add("indegree", 0);
                tmp.Add("in", new List<object>());
                tmp.Add("out", new List<object>());
                tmp.Add("vs", new List<object> { entry["v"] });
                tmp.Add("i", i);

                mappedEntries.Add(entry["v"], tmp);
                if (entry.ContainsKey("barycenter"))
                {
                    tmp["barycenter"] = entry["barycenter"];
                    tmp["weight"] = entry["weight"];
                }
            }

            foreach (var e in cg.edges())
            {
                dynamic entryV = mappedEntries[e["v"]];
                dynamic entryW = mappedEntries[e["w"]];
                if (entryV != null && entryW != null)
                {
                    entryW["indegree"]++;
                    entryV["out"].Add(mappedEntries[e["w"]]);
                }

            }
            var sourceSet = mappedEntries.Values.Where(z => ((dynamic)z)["indegree"] != null).ToList();


            //return doResolveConflicts(sourceSet);
            List<object> results = new List<object>();
            while (sourceSet.Count != 0)
            {
                dynamic entry = sourceSet[sourceSet.Count - 1];
                sourceSet.RemoveAt(sourceSet.Count - 1);
                results.Add(entry);
                //entry["in"].reverse().forEach(handleIn(entry));
                var aa = (dynamic)entry["in"];
                aa.Reverse();
                foreach (var uEntry in aa)
                {
                    var vEntry = entry;
                    if (uEntry["merged"])
                    {
                        continue;
                    }
                    if (uEntry["barycenter"] == null || vEntry["barycenter"] == null
                        || uEntry["barycenter"] >= vEntry["barycenter"])
                    {
                        mergeEntries(vEntry, uEntry);
                    }
                }
                //entry["out"]forEach(handleOut(entry));
                foreach (var wEntry in (dynamic)entry["out"])
                {
                    var vEntry = entry;
                    wEntry["in"].Add(vEntry);
                    if (--wEntry["indegree"] == 0)
                    {
                        sourceSet.Add(wEntry);
                    }
                }
            }
            var temp1 = results.Where(z => !((dynamic)z).ContainsKey("merged"));

            return temp1.Select(obj =>
            {
                dynamic value = new JavaScriptLikeObject();
                var attrs = new string[] { "vs", "i", "barycenter", "weight" };
                dynamic _obj = obj;
                foreach (var key in attrs)
                {
                    if (_obj.ContainsKey(key))
                    {
                        value[key] = _obj[key];
                    }
                }
                return value;
            }).ToArray();
        }

        private static resolveDto1[] doResolveConflicts(dynamic _sourceSet)
        {
            List<KeyValuePair<string, resolveDto>> entries = new List<KeyValuePair<string, resolveDto>>();
            var sourceSet = _sourceSet.ToList();
            Action<KeyValuePair<string, resolveDto>> handleIn = (vEntry) =>
            {
                /*
                 *     return function(uEntry) {
           if (uEntry.merged)
           {
               return;
           }
           if (_.isUndefined(uEntry.barycenter) ||
               _.isUndefined(vEntry.barycenter) ||
               uEntry.barycenter >= vEntry.barycenter)
           {
               mergeEntries(vEntry, uEntry);
           }
       };
                 */
            };
            Action<KeyValuePair<string, resolveDto>> handleOut = (vEntry) =>
            {
                /*
                 *    return function(wEntry) {
           wEntry["in"].push(vEntry);
           if (--wEntry.indegree === 0)
           {
               sourceSet.push(wEntry);
           }
       };
                 */
            };
            while (sourceSet.Count > 0)
            {
                var entry = sourceSet.Last();
                sourceSet.RemoveAt(sourceSet.Count - 1);
                entries.Add(entry);
                foreach (var item in entry.Value._in.Reverse())
                {
                    handleIn(entry);
                }
                foreach (var item in entry.Value._out)
                {
                    handleOut(entry);
                }
            }
            var ww1 = entries.Where(entry => entry.Value.merged != null);
            return ww1.Select(entry => new resolveDto1 { i = entry.Value.i, weight = entry.Value.weight, barycenter = entry.Value.barycenter, vs = entry.Value.vs }).ToArray();
        }

        public class resolveDto1
        {
            public int barycenter;
            public int i;
            public int weight;
            public string[] vs;
        }
        /*





    return _.map(_.filter(entries, function(entry) { return !entry.merged; }),
    function(entry) {
       return _.pick(entry, ["vs", "i", "barycenter", "weight"]);
    });

    }

    function mergeEntries(target, source)
    {
    var sum = 0;
    var weight = 0;

    if (target.weight)
    {
       sum += target.barycenter * target.weight;
       weight += target.weight;
    }

    if (source.weight)
    {
       sum += source.barycenter * source.weight;
       weight += source.weight;
    }

    target.vs = source.vs.concat(target.vs);
    target.barycenter = sum / weight;
    target.weight = weight;
    target.i = Math.min(source.i, target.i);
    source.merged = true;
    }
    */
    }
}
