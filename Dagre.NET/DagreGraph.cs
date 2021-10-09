using System;
using System.Collections.Generic;
using System.Linq;


namespace Dagre
{
    public class DagreGraph
    {
        public DagreGraph(bool compound)
        {
            _isCompound = compound;
            if (compound)
            {
                _children.Add(GRAPH_NODE as string, new JavaScriptLikeObject());
            }
        }


        //public bool compound;
        // public bool directed = true;
        public bool _isDirected = true;
        public bool _isCompound = false;
        public bool _isMultigraph = false;

        public void ClearNulls()
        {
            var ar1 = _parent.Where(z => (dynamic)(z.Value) == GRAPH_NODE || z.Value == null).Select(z => z.Key).ToArray();
            foreach (var item in ar1)
            {
                _parent.Remove(item);
            }
        }

        public void CompareNodes(DagreGraph gr)
        {
            if (gr._nodeCount != _nodeCount) throw new DagreException();
            if (_nodesRaw.Keys.Count != gr._nodesRaw.Keys.Count) throw new DagreException();
            for (int i = 0; i < _nodesRaw.Keys.Count; i++)
            {
                var key1 = _nodesRaw.Keys.ToArray()[i];
                if (!gr._nodesRaw.ContainsKey(key1)) throw new DagreException();
            }
            for (int i = 0; i < _nodesRaw.Keys.Count; i++)
            {
                var key1 = _nodesRaw.Keys.ToArray()[i];
                //if (_nodesRaw.Keys.ToArray()[i] != gr._nodesRaw.Keys.ToArray()[i]) throw new DagreException();
                if (!gr._nodesRaw.ContainsKey(key1)) throw new DagreException();
                dynamic node1 = _nodesRaw[key1];
                dynamic node2 = gr._nodesRaw[key1];

                if (node1 is IDictionary<string, object>)
                {
                    continue;
                    if (node1.Keys.Count != node2.Keys.Count) throw new DagreException();

                    foreach (var key in node1.Keys)
                    {
                        dynamic val1 = node1[key];
                        dynamic val2 = node2[key];
                        if (val1 is IDictionary<string, object>)
                        {

                        }
                        else
                        {
                            if (val1 is decimal || val2 is decimal)
                            {
                                if ((float)val1 != (float)val2) throw new DagreException();
                            }
                            else if (val1 != val2) throw new DagreException();
                        }
                    }
                }
                else
                {
                    if (node1 != node2) throw new DagreException();
                }
            }
        }
        public void CompareEdges(DagreGraph gr)
        {
            if (gr._edgeCount != _edgeCount) throw new DagreException();
            if (_edgeObjs.Keys.Count != gr._edgeObjs.Keys.Count) throw new DagreException();
            if (_edgeLabels.Keys.Count != gr._edgeLabels.Keys.Count) throw new DagreException();
            for (int i = 0; i < _edgeLabels.Keys.Count; i++)
            {
                var key1 = _edgeLabels.Keys.ToArray()[i];
                if (!gr._edgeLabels.ContainsKey(key1)) throw new DagreException();
            }
            for (int i = 0; i < _edgeLabels.Keys.Count; i++)
            {
                var key1 = _edgeLabels.Keys.ToArray()[i];
                if (!gr._edgeLabels.ContainsKey(key1)) throw new DagreException();

                dynamic edge1 = _edgeLabels[key1];
                dynamic edge2 = gr._edgeLabels[key1];
                if (edge1 is IDictionary<string, object>)
                {
                    if (edge1.Keys.Count != edge2.Keys.Count) throw new DagreException();
                    foreach (var key in edge1.Keys)
                    {
                        dynamic val1 = edge1[key];
                        dynamic val2 = edge2[key];

                        if (val1 is IEnumerable<object> || val2 is IEnumerable<object>)
                        {
                            int index = 0;
                            foreach (var item in val1)
                            {
                                var second = val2[index++];
                                if (item is IDictionary<string, object>)
                                {
                                    foreach (var zitem in item)
                                    {
                                        var key2 = zitem.Key;
                                        if (!second.ContainsKey(key2)) throw new DagreException("wrong");
                                        if (zitem.Value is float || zitem.Value is decimal)
                                        {
                                            if ((float)zitem.Value != (float)second[key2]) throw new DagreException("wrong");
                                        }
                                        else
                                        if (zitem.Value != second[key2]) throw new DagreException("wrong");
                                    }
                                }

                            }
                        }
                        else if (val2 is decimal || val1 is decimal)
                        {
                            if ((float)val1 != (float)val2) throw new DagreException();
                        }
                        else
                        if (val1 != val2) throw new DagreException();
                    }
                }
                else
                {
                    if (edge1 != edge2) throw new DagreException();

                }

            }
            for (int i = 0; i < _edgeObjs.Keys.Count; i++)
            {
                var key1 = _edgeObjs.Keys.ToArray()[i];
                if (!gr._edgeObjs.ContainsKey(key1)) throw new DagreException();

                //if (_edgeLabels.Keys.ToArray()[i] != gr._edgeLabels.Keys.ToArray()[i]) throw new DagreException();
                dynamic edge1 = _edgeObjs[key1];
                dynamic edge2 = gr._edgeObjs[key1];
                if (edge1.Keys.Count != edge2.Keys.Count) throw new DagreException();
                foreach (var key in edge1.Keys)
                {
                    dynamic val1 = edge1[key];
                    dynamic val2 = edge2[key];
                    if (val1 != val2) throw new DagreException();
                }

            }
        }
        public bool Compare(DagreGraph gr)
        {
            ClearNulls();
            gr.ClearNulls();


            if (gr._edgeCount != _edgeCount) throw new DagreException();
            if (gr._nodeCount != _nodeCount) throw new DagreException();
            if (gr._isDirected != _isDirected) throw new DagreException();
            if (gr._isCompound != _isCompound) throw new DagreException();
            if (gr._isMultigraph != _isMultigraph) throw new DagreException();
            if (_nodesRaw.Keys.Count != gr._nodesRaw.Keys.Count) throw new DagreException();
            if (_in.Keys.Count != gr._in.Keys.Count) throw new DagreException();
            if (_out.Keys.Count != gr._out.Keys.Count) throw new DagreException();
            if (_successors.Keys.Count != gr._successors.Keys.Count) throw new DagreException();
            if (_predecessors.Keys.Count != gr._predecessors.Keys.Count) throw new DagreException();
            if (_edgeObjs.Keys.Count != gr._edgeObjs.Keys.Count) throw new DagreException();
            if (_edgeLabels.Keys.Count != gr._edgeLabels.Keys.Count) throw new DagreException();
            if (_parent.Keys.Count != gr._parent.Keys.Count) throw new DagreException();
            if (_children.Keys.Count != gr._children.Keys.Count) throw new DagreException();


            for (int i = 0; i < _parent.Keys.Count; i++)
            {
                var key1 = _parent.Keys.ToArray()[i];
                if (!gr._parent.ContainsKey(key1)) throw new DagreException();
            }
            for (int i = 0; i < _parent.Keys.Count; i++)
            {
                var key1 = _parent.Keys.ToArray()[i];
                if (!gr._parent.ContainsKey(key1)) throw new DagreException();
                dynamic node1 = _parent[key1];
                dynamic node2 = gr._parent[key1];
                if (node1 is IDictionary<string, object>)
                {
                    if (node1.Keys.Count != node2.Keys.Count) throw new DagreException();

                    foreach (var key in node1.Keys)
                    {
                        dynamic val1 = node1[key];
                        dynamic val2 = node2[key];
                        if (val1 is IDictionary<string, object>)
                        {

                        }
                        else
                        {
                            if (val1 != val2) throw new DagreException();
                        }
                    }
                }
                else
                {
                    if (node1 != node2) throw new DagreException();
                }
            }
            for (int i = 0; i < _children.Keys.Count; i++)
            {
                var key1 = _children.Keys.ToArray()[i];
                if (!gr._children.ContainsKey(key1)) throw new DagreException();
                dynamic v0 = _children[key1];
                dynamic v1 = gr._children[key1];
                if (v0.Keys.Count != v1.Keys.Count) throw new DagreException("wrong");
                foreach (var item in v0.Keys)
                {
                    var vv0 = v0[item];
                    var vv1 = v1[item];
                    if (vv0 != vv1) throw new DagreException("wrong");
                }
            }
            for (int i = 0; i < children().Length; i++)
            {
                var v0 = children()[i];
                var v1 = gr.children()[i];
                if (v0 != v1) throw new DagreException("wrong");
            }
            for (int i = 0; i < _successors.Keys.Count; i++)
            {
                var key1 = _successors.Keys.ToArray()[i];
                if (!gr._successors.ContainsKey(key1)) throw new DagreException();
                dynamic node1 = _successors[key1];
                dynamic node2 = gr._successors[key1];
                if (node1.Keys.Count != node2.Keys.Count) throw new DagreException();
                foreach (var key in node1.Keys)
                {
                    dynamic val1 = node1[key];
                    dynamic val2 = node2[key];
                    if (val1 is IDictionary<string, object>)
                    {

                    }
                    else
                    {
                        if (val1 != val2) throw new DagreException();
                    }
                }
            }
            for (int i = 0; i < _predecessors.Keys.Count; i++)
            {
                var key1 = _predecessors.Keys.ToArray()[i];
                if (!gr._predecessors.ContainsKey(key1)) throw new DagreException();
                dynamic node1 = _predecessors[key1];
                dynamic node2 = gr._predecessors[key1];
                if (node1.Keys.Count != node2.Keys.Count) throw new DagreException();
                foreach (var key in node1.Keys)
                {
                    dynamic val1 = node1[key];
                    dynamic val2 = node2[key];
                    if (val1 is IDictionary<string, object>)
                    {

                    }
                    else
                    {
                        if (val1 != val2) throw new DagreException();
                    }
                }
            }
            for (int i = 0; i < _children.Keys.Count; i++)
            {
                var key1 = _children.Keys.ToArray()[i];
                if (!gr._children.ContainsKey(key1)) throw new DagreException();
                dynamic node1 = _children[key1];
                dynamic node2 = gr._children[key1];
                if (node1.Keys.Count != node2.Keys.Count) throw new DagreException();

                foreach (var key in node1.Keys)
                {
                    dynamic val1 = node1[key];
                    dynamic val2 = node2[key];
                    if (val1 is IDictionary<string, object>)
                    {

                    }
                    else
                    {
                        if (val1 != val2) throw new DagreException();
                    }
                }

            }
            for (int i = 0; i < _nodesRaw.Keys.Count; i++)
            {
                var key1 = _nodesRaw.Keys.ToArray()[i];
                if (!gr._nodesRaw.ContainsKey(key1)) throw new DagreException();
            }
            for (int i = 0; i < _nodesRaw.Keys.Count; i++)
            {
                var key1 = _nodesRaw.Keys.ToArray()[i];
                //if (_nodesRaw.Keys.ToArray()[i] != gr._nodesRaw.Keys.ToArray()[i]) throw new DagreException();
                if (!gr._nodesRaw.ContainsKey(key1)) throw new DagreException();
                dynamic node1 = _nodesRaw[key1];
                dynamic node2 = gr._nodesRaw[key1];

                if (node1 is IDictionary<string, object>)
                {
                    if (node1.Keys.Count != node2.Keys.Count) throw new DagreException();

                    foreach (var key in node1.Keys)
                    {
                        dynamic val1 = node1[key];
                        dynamic val2 = node2[key];
                        if (val1 is IDictionary<string, object>)
                        {

                        }
                        else if (val1 is Array)
                        {

                        }
                        else
                        {
                            if (val1 is decimal || val2 is decimal)
                            {
                                if ((float)val1 != (float)val2) throw new DagreException();
                            }
                            else if (val1 != val2) throw new DagreException();
                        }
                    }
                }
                else
                {
                    if (node1 != node2) throw new DagreException();
                }
            }
            for (int i = 0; i < _edgeLabels.Keys.Count; i++)
            {
                var key1 = _edgeLabels.Keys.ToArray()[i];
                if (!gr._edgeLabels.ContainsKey(key1)) throw new DagreException();
            }
            for (int i = 0; i < _edgeLabels.Keys.Count; i++)
            {
                var key1 = _edgeLabels.Keys.ToArray()[i];
                if (!gr._edgeLabels.ContainsKey(key1)) throw new DagreException();

                dynamic edge1 = _edgeLabels[key1];
                dynamic edge2 = gr._edgeLabels[key1];
                if (edge1 is IDictionary<string, object>)
                {
                    if (edge1.Keys.Count != edge2.Keys.Count) throw new DagreException();
                    foreach (var key in edge1.Keys)
                    {
                        dynamic val1 = edge1[key];
                        dynamic val2 = edge2[key];

                        if (val1 is IEnumerable<object> || val2 is IEnumerable<object>)
                        {
                            int index = 0;
                            foreach (var item in val1)
                            {
                                var second = val2[index++];
                                if (item is IDictionary<string, object>)
                                {
                                    foreach (var zitem in item)
                                    {
                                        var key2 = zitem.Key;
                                        if (!second.ContainsKey(key2)) throw new DagreException("wrong");
                                        if (zitem.Value is float || zitem.Value is decimal)
                                        {
                                            if ((float)zitem.Value != (float)second[key2]) throw new DagreException("wrong");
                                        }
                                        else
                                        if (zitem.Value != second[key2]) throw new DagreException("wrong");
                                    }
                                }

                            }
                        }
                        else if (val2 is decimal || val1 is decimal)
                        {
                            if ((float)val1 != (float)val2) throw new DagreException();
                        }
                        else
                        if (val1 != val2) throw new DagreException();
                    }
                }
                else
                {
                    if (edge1 != edge2) throw new DagreException();

                }

            }
            for (int i = 0; i < _edgeObjs.Keys.Count; i++)
            {
                var key1 = _edgeObjs.Keys.ToArray()[i];
                if (!gr._edgeObjs.ContainsKey(key1)) throw new DagreException();

                //if (_edgeLabels.Keys.ToArray()[i] != gr._edgeLabels.Keys.ToArray()[i]) throw new DagreException();
                dynamic edge1 = _edgeObjs[key1];
                dynamic edge2 = gr._edgeObjs[key1];
                if (edge1.Keys.Count != edge2.Keys.Count) throw new DagreException();
                foreach (var key in edge1.Keys)
                {
                    dynamic val1 = edge1[key];
                    dynamic val2 = edge2[key];
                    if (val1 != val2) throw new DagreException();
                }

            }
            if (!DagreLabel.Compare(_label, gr._label)) throw new DagreException();
            var a1 = gr.nodes();
            var a2 = nodes();
            for (int i = 0; i < a1.Length; i++)
            {
                var res1 = a1[i];
                var res2 = a2[i];
                for (int j = 0; j < res1.Length; j++)
                {
                    if (res1[j] != res2[j])
                    {
                        throw new DagreException();
                    }
                }
            }
            for (int i = 0; i < _nodesRaw.Keys.Count; i++)
            {
                var key = _nodesRaw.Keys.ToArray()[i];

                var res1 = gr.neighbors(key);
                var res = neighbors(key);
                if (res1 == null && res == null) continue;
                for (int j = 0; j < res1.Length; j++)
                {
                    if (res1[j] != res[j])
                    {
                        throw new DagreException();
                    }
                }
            }

            for (int i = 0; i < _nodesRaw.Keys.Count; i++)
            {
                var key = _nodesRaw.Keys.ToArray()[i];

                var res1 = gr.children(key);
                var res = children(key);
                for (int j = 0; j < res1.Length; j++)
                {
                    if (res1[j] != res[j])
                    {
                        throw new DagreException();
                    }
                }
            }
            return true;
        }

        public DagreGraph setDefaultNodeLabel(Func<object, dynamic> p)
        {
            _defaultNodeLabelFn = p;
            return this;
        }        

        public void LoadJson(object des)
        {
            var d = des as Dictionary<string, object>;
            _children.Clear();

            foreach (var item in d)
            {
                switch (item.Key)
                {
                    case "_isMultigraph":

                        {
                            _isMultigraph = (bool)item.Value;
                            break;
                        }
                    case "_isCompound":

                        {
                            _isCompound = (bool)item.Value;
                            break;
                        }
                    case "_isDirected":
                        {
                            _isDirected = (bool)item.Value;
                            break;
                        }
                    case "_label":
                        {
                            if (item.Value != null)
                            {
                                var dic = item.Value as Dictionary<string, object>;
                                dynamic lb = _label;
                                if (dic.ContainsKey("ranksep"))
                                    lb.Add("ranksep", (int)dic["ranksep"]);
                                if (dic.ContainsKey("maxrank"))
                                    lb.Add("maxrank", (int)dic["maxrank"]);
                                if (dic.ContainsKey("edgesep"))
                                    lb.Add("edgesep", (int)dic["edgesep"]);
                                if (dic.ContainsKey("nodesep"))
                                    lb.Add("nodesep", (int)dic["nodesep"]);


                                if (dic.ContainsKey("nodeRankFactor"))
                                    lb.Add("nodeRankFactor", (int)dic["nodeRankFactor"]);

                                if (dic.ContainsKey("nestingRoot"))
                                    lb.Add("nestingRoot", (string)dic["nestingRoot"]);

                                if (dic.ContainsKey("rankdir"))
                                    lb.Add("rankdir", (string)dic["rankdir"]);
                                if (dic.ContainsKey("root"))
                                    lb.Add("root", dic["root"]);
                                if (dic.ContainsKey("dummyChains"))
                                    lb.Add("dummyChains", dic["dummyChains"]);
                            }
                            break;
                        }
                    case "_edgeObjs":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                var dind = new DagreEdgeIndex();
                                var aa = edg.Value as Dictionary<string, object>;
                                _edgeObjs.Add(edg.Key, edg.Value);
                            }
                            break;
                        }
                    case "_nodes":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                JavaScriptLikeObject js = new JavaScriptLikeObject();
                                if (edg.Value is IDictionary<string, object> dic1)
                                {
                                    foreach (var item1 in dic1)
                                    {
                                        js.Add(item1);
                                    }
                                    _nodesRaw.Add(edg.Key, js);

                                }
                                else
                                _nodesRaw.Add(edg.Key, edg.Value);
                            }
                            break;
                        }
                    case "_out":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                _out.Add(edg.Key, edg.Value as Dictionary<string, object>);
                            }
                            break;
                        }
                    case "_in":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                JavaScriptLikeObject js = new JavaScriptLikeObject();
                                if (edg.Value is IDictionary<string, object> dic1)
                                {
                                    foreach (var item1 in dic1)
                                    {
                                        js.Add(item1);
                                    }
                                    _in.Add(edg.Key, js);
                                }
                                else
                                    _in.Add(edg.Key, edg.Value);
                            }
                            break;
                        }

                    case "_edgeLabels":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                _edgeLabels.Add(edg.Key, edg.Value);
                            }
                            break;
                        }
                    case "_children":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                _children.Add(edg.Key, edg.Value);
                            }
                            break;
                        }
                    case "_parent":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                JavaScriptLikeObject js = new JavaScriptLikeObject();
                                if (edg.Value is IDictionary<string, object> dic1)
                                {
                                    foreach (var item1 in dic1)
                                    {
                                        js.Add(item1);
                                    }
                                    _parent.Add(edg.Key, js);
                                }
                                else
                                _parent.Add(edg.Key, edg.Value);


                            }
                            break;
                        }

                    case "_preds":
                    case "_predecessors":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                _predecessors.Add(edg.Key, edg.Value as Dictionary<string, object>);
                            }
                            break;
                        }
                    case "_nodeCount":
                        {
                            _nodeCount = (int)(item.Value);
                            break;
                        }
                    case "_edgeCount":
                        {
                            _edgeCount = (int)(item.Value);
                            break;
                        }
                    case "_sucs":

                    case "_successors":
                        {
                            var dic = item.Value as Dictionary<string, object>;

                            foreach (var edg in dic)
                            {
                                _successors.Add(edg.Key, edg.Value as Dictionary<string, object>);
                            }
                            break;
                        }
                }
            }
            
            ClearNulls();
        }

        public void UpdateAttributeEdgesTo(DagreGraph gr)
        {
            if (gr._edgeCount != _edgeCount) throw new DagreException();
            if (_edgeLabels.Keys.Count != gr._edgeLabels.Keys.Count) throw new DagreException();
            for (int i = 0; i < _edgeLabels.Keys.Count; i++)
            {
                var key1 = _edgeLabels.Keys.ToArray()[i];
                if (!gr._edgeLabels.ContainsKey(key1)) throw new DagreException();
            }
            for (int i = 0; i < _edgeLabels.Keys.Count; i++)
            {
                var key1 = _edgeLabels.Keys.ToArray()[i];
                //if (_nodesRaw.Keys.ToArray()[i] != gr._nodesRaw.Keys.ToArray()[i]) throw new DagreException();
                if (!gr._edgeLabels.ContainsKey(key1)) throw new DagreException();
                dynamic node1 = _edgeLabels[key1];
                dynamic node2 = gr._edgeLabels[key1];

                if (node1 is IDictionary<string, object>)
                {
                    ((dynamic)gr._edgeLabels[key1]).Clear();
                    //if (node1.Keys.Count != node2.Keys.Count) throw new DagreException();

                    foreach (var key in node1.Keys)
                    {
                        dynamic val1 = node1[key];
                        ((dynamic)gr._edgeLabels[key1]).Add(key, val1);
                        continue;

                        dynamic val2 = node2[key];
                        if (val1 is IDictionary<string, object>)
                        {

                        }
                        else
                        {
                            // if (val1 is decimal || val2 is decimal)
                            // {
                            //     if ((float)val1 != (float)val2) throw new DagreException();
                            // }
                            //else if (val1 != val2) throw new DagreException();
                        }
                    }
                }
                else
                {
                    gr._edgeLabels[key1] = node1;
                }
            }
        }

        public void UpdateAttributeNodesTo(DagreGraph gr)
        {
            if (gr._nodeCount != _nodeCount) throw new DagreException();
            if (_nodesRaw.Keys.Count != gr._nodesRaw.Keys.Count) throw new DagreException();
            for (int i = 0; i < _nodesRaw.Keys.Count; i++)
            {
                var key1 = _nodesRaw.Keys.ToArray()[i];
                if (!gr._nodesRaw.ContainsKey(key1)) throw new DagreException();
            }
            for (int i = 0; i < _nodesRaw.Keys.Count; i++)
            {
                var key1 = _nodesRaw.Keys.ToArray()[i];
                //if (_nodesRaw.Keys.ToArray()[i] != gr._nodesRaw.Keys.ToArray()[i]) throw new DagreException();
                if (!gr._nodesRaw.ContainsKey(key1)) throw new DagreException();
                dynamic node1 = _nodesRaw[key1];
                dynamic node2 = gr._nodesRaw[key1];
                gr._nodesRaw[key1] = node1;
                if (node1 is IDictionary<string, object>)
                {
                    continue;
                    //if (node1.Keys.Count != node2.Keys.Count) throw new DagreException();

                    foreach (var key in node1.Keys)
                    {
                        dynamic val1 = node1[key];
                        dynamic val2 = node2[key];
                        if (val1 is IDictionary<string, object>)
                        {

                        }
                        else
                        {
                            // if (val1 is decimal || val2 is decimal)
                            // {
                            //     if ((float)val1 != (float)val2) throw new DagreException();
                            // }
                            //else if (val1 != val2) throw new DagreException();
                        }
                    }
                }
                else
                {
                    if (node1 != node2) throw new DagreException();
                }
            }
        }

        public string[] nodes()
        {
            return nodesRaw();
        }
        public string[] nodesRaw()
        {
            return _nodesRaw.Keys.ToArray();
        }

        public JavaScriptLikeObject _nodesRaw = new JavaScriptLikeObject();

        object _label = new JavaScriptLikeObject();
        public dynamic graph() { return _label; }


        public dynamic nodeRaw(string v)
        {
            if (!_nodesRaw.ContainsKey(v))
            {
                return null;
            }
            return _nodesRaw[v];

        }

        internal bool isMultigraph()
        {
            return _isMultigraph;
        }

        public dynamic node(object v)
        {
            return nodeRaw((string)v);
        }

        public List<DagreEdgeIndex> _edgesIndexes = new List<DagreEdgeIndex>();

        /*public DagreEdge edge(string v)
        {
            
            return _edges.First(z => z.key == v);
        }
    */
        public static string edgeArgsToId(bool isDirectred, object _v, object _w, object name)
        {
            var v = _v + "";
            var w = _w + "";
            if (!isDirectred && string.CompareOrdinal(v, w) > 0)//v>w
            {
                var tmp = v;
                v = w;
                w = tmp;

            }
            return v + EDGE_KEY_DELIM + w + EDGE_KEY_DELIM + (name == null ? DEFAULT_EDGE_NAME : name);
        }

        
        public static string EDGE_KEY_DELIM = "\x01";//\x01
        public static string DEFAULT_EDGE_NAME = "\x00";//\x00
                
        public DagreLabel edge(DagreEdgeIndex v)
        {
            return edge(v.v, v.w, v.name);

        }
        public dynamic edge(string v, string w, string name = null)
        {
            var e = edgeArgsToId(_isDirected, v, w, name);
            if (_edgeLabels.ContainsKey(e as string)) return null;
            return _edgeLabels[e as string];
            //return _edges[v];
        }

        //public bool PreserveOrder = false;
        internal string[] neighbors(string v)
        {
            var preds = predecessors(v);
            if (preds != null)
            {
                string[] ret = null;
                /*if (!PreserveOrder)
                {
                    ret = successors(v).OrderBy(z => z).ToArray();
                    var dgts = ret.Where(z => z.All(char.IsDigit)).ToArray();
                    Array.Sort(dgts, (x, y) => int.Parse(x) - int.Parse(y));
                    var remains = ret.Except(dgts).ToArray();
                    Array.Sort(remains, (x, y) => string.CompareOrdinal(x, y));
                    ret = preds.Union(dgts.Union(remains)).ToArray();
                }
                else*/
                {
                    ret = preds.Union(successors(v)).ToArray();
                }
                //ret = ret.Reverse().ToArray();
                return ret;
            }
            return null;
        }

        public string[] predecessors(string v)
        {
            if (_predecessors.ContainsKey(v))
            {
                string[] ret = null;
                //if (!PreserveOrder)
                {
                    ret = (((dynamic)_predecessors[v]).Keys as ICollection<string>).ToArray();
                    /*var dgts = ret.Where(z => z.All(char.IsDigit)).ToArray();
                    Array.Sort(dgts, (x, y) => int.Parse(x) - int.Parse(y));
                    var remains = ret.Except(dgts).ToArray();
                    Array.Sort(remains, (x, y) => string.CompareOrdinal(x, y));
                    ret = dgts.Union(remains).ToArray();*/
                    return ret;
                }

                //var predsV = _predecessors[v];
                //return predsV.Keys.ToArray();


            }
            return null;
        }

        public Dictionary<string, object> _edgeLabels = new Dictionary<string, object>();
        public dynamic[] edges()
        {
            return edgesRaw();
            //return _edgeObjs.Values.Select(z => z as DagreEdgeIndex).ToArray();
        }

        public dynamic[] edgesRaw()
        {
            return _edgeObjs.Values.ToArray();
        }
        internal DagreGraph removeEdge(object args)
        {
            return removeEdge(new object[] { args });
        }
        internal DagreGraph removeEdge(object[] args)
        {
            string key = "";
            if (args.Length == 1)
                key = edgeObjToIdRaw(_isDirected, args[0]);
            else if (args.Length == 2)
                key = edgeArgsToIdRaw(_isDirected, args[0], args[1], null);
            else
                key = edgeArgsToIdRaw(_isDirected, args[0], args[1], args[2]);
            if (!_edgeObjs.ContainsKey(key)) return this;
            dynamic edge = _edgeObjs[key];
            if (edge != null)
            {
                var v = edge["v"];
                var w = edge["w"];
                _edgeLabels.Remove(key);
                _edgeObjs.Remove(key);
                Action<object, object> decrementOrRemoveEntry = (map, _k) =>
                 {
                     var k = (string)_k;
                     dynamic d = map;
                     var val = d[k];
                     d[k]--;
                     //if (!--map[k])
                     if (d[k] == 0)
                     {
                         d.Remove(k);
                     }
                 };
                decrementOrRemoveEntry(this._predecessors[(string)w], v);
                decrementOrRemoveEntry(this._successors[(string)v], w);
                ((dynamic)_in[(string)w]).Remove(key);
                ((dynamic)_out[(string)v]).Remove(key);
                this._edgeCount--;
            }
            return this;
        }

        public object setEdge(object[] args)
        {            
            object value = null;
            dynamic arg0 = args[0];
            object v = null;
            object w = null;
            object name = null;
            bool valueSpecified = false;
            if (arg0 != null && !(arg0 is string) && (arg0).ContainsKey("v"))
            {
                v = arg0["v"];
                w = arg0["w"];
                if (arg0.ContainsKey("name"))
                    name = arg0["name"];
                if (args.Length == 2)
                {
                    value = args[1];
                    valueSpecified = true;
                }
            }
            else
            {
                v = args[0];
                w = args[1];
                if (args.Length > 3)
                    name = args[3];
                if (args.Length > 2)
                {
                    value = args[2];
                    valueSpecified = true;
                }
            }
            v = "" + v;
            w = "" + w;
            if (name != null)
            {
                name = "" + name;
            }
            var e = edgeArgsToId(this._isDirected, v, w, name);
            if (this._edgeLabels.ContainsKey(e))
            {
                if (valueSpecified)
                {
                    this._edgeLabels[e] = value;
                }
                return this;
            }
            if (name != null && !_isMultigraph)
            {
                throw new DagreException("Cannot set a named edge when isMultigraph = false");
            }
            // It didn't exist, so we need to create it.
            // First ensure the nodes exist.
            this.setNode(v);
            this.setNode(w);
            this._edgeLabels[e] = valueSpecified ? value : this._defaultEdgeLabelFn(v, w, name);
            v = "" + v;
            w = "" + w;
            if (!this._isDirected && string.CompareOrdinal((string)v, (string)w) > 0)
            {
                var tmp = (string)v;
                v = w;
                w = tmp;
            }
            JavaScriptLikeObject jobj = new JavaScriptLikeObject();
            jobj.AddOrUpdate("v", v);
            jobj.AddOrUpdate("w", w);
            jobj.AddOrUpdate("name", name);
            JavaScriptLikeObject jobj2 = new JavaScriptLikeObject();
            jobj2.AddOrUpdate("v", v);
            jobj2.AddOrUpdate("w", w);

            dynamic edgeObj = name != null ? jobj : jobj2;
            edgeObj.Freeze();
            this._edgeObjs[e] = edgeObj;
            Action<dynamic, dynamic> incrementOrInitEntry = (map, k) =>
            {
                var _map = map as IDictionary<string, object>;
                var _k = k as IDictionary<string, object>;
                if (_map.ContainsKey(k))
                {
                    _map[k]++;
                }
                else
                {
                    _map.Add(k, 1);
                }
            };
            incrementOrInitEntry(this._predecessors[(string)w], v);
            incrementOrInitEntry(this._successors[(string)v], w);
            (((dynamic)_in[(string)w])[(string)e]) = edgeObj;
            (((dynamic)_out[(string)v])[(string)e]) = edgeObj;
            _edgeCount++;
            return this;

        }

        public int _edgeCount;


        internal string[] sources()
        {
            return nodesRaw().Where(v =>
            {
                if (!_in.ContainsKey(v))
                {
                    return false;
                }
                return ((dynamic)_in[v]).Count == 0;
            }).ToArray();
        }

        private void incrementOrInitEntry(Dictionary<string, object> dictionaries, object v)
        {
            if (dictionaries.ContainsKey(v as string))
            {
                var vv = (int)(dictionaries[v as string]);
                dictionaries[v as string] = vv + 1;
            }
            else
            {
                dictionaries.Add(v as string, 1);
            }
        }

        internal bool hasEdgeRaw(object[] args)
        {
            return edgeRaw(args) != null;
        }

        public JavaScriptLikeObject _edgeObjs = new JavaScriptLikeObject();

        private Func<object, object, object, object> _defaultEdgeLabelFn = (x, y, z) => { return new JavaScriptLikeObject(); };

        //int _edgesCount = 0;

        internal bool hasNode(string v)
        {
            return _nodesRaw.ContainsKey(v);

        }

        public string[] successors(string v)
        {
            if (_successors.ContainsKey(v))
            {
                dynamic sucsV = _successors[v];
                return (sucsV.Keys as ICollection<string>).ToArray();
            }
            return null;
        }
        internal string[] children(object v = null)
        {
            if (v == null)
            {
                v = GRAPH_NODE;

            }
            if (_isCompound)
            {
                if (_children.ContainsKey(v as string))
                {
                    var children = _children[v as string] as IDictionary<string, object>;
                    var ret = children.Keys.ToArray();

                    //   if (PreserveOrder)
                    {
                        return ret.ToArray();
                    }
                    var dgts = ret.Where(z => z.All(char.IsDigit)).ToArray();
                    Array.Sort(dgts, (x, y) => int.Parse(x) - int.Parse(y));
                    var remains = ret.Except(dgts).ToArray();
                    Array.Sort(remains, (x, y) => string.CompareOrdinal(x, y));
                    ret = dgts.Union(remains).ToArray();
                    return ret.ToArray();

                }

            }
            else if ((dynamic)v == GRAPH_NODE)
            {
                return nodes();
            }
            else if (hasNode(v as string))
            {
                return new string[] { };
            }
            return new string[] { };
        }



        internal object[] outEdges(string v, string w = null)
        {
            if (_out.ContainsKey(v))
            {
                var outV = (((dynamic)_out[v]).Values as ICollection<object>).ToArray();
                if (outV != null && outV.Any())
                {
                    if (w == null)
                    {
                        return outV;
                    }
                    return outV.Where((dynamic z) => z["w"] == w).ToArray();
                }

            }
            return new object[] { };


        }

        internal IEnumerable<object> nodeEdges(string v, string w = null)
        {
            var inEdges = this.inEdges(v, w);
            if (inEdges != null)
            {
                return inEdges.Union(outEdges(v, w));
            }
            return null;
        }



        /*internal void setEdge(string w, string v, object label, Guid guid)
        {
            throw new NotImplementedException();
        }
        */

        public dynamic edge(object e)
        {
            return edgeRaw(e);
        }
        public string edgeObjToIdRaw(bool isDirectred, dynamic v)
        {
            var tt = v as IDictionary<string, object>;
            var vvv = tt["v"];
            var www = tt["w"];
            string name = null;
            if (tt.ContainsKey("name"))
            {
                name = (string)tt["name"];
            }
            return edgeArgsToIdRaw(isDirectred, vvv, www, name);
        }
        internal dynamic edgeRaw(object v)
        {
            var key = edgeObjToIdRaw(_isDirected, v);
            return _edgeLabels[key];

        }
        internal dynamic edgeRaw(object[] args)
        {
            string key = "";
            if (args.Length == 1)
                key = edgeObjToIdRaw(_isDirected, args[0]);
            else if (args.Length == 2)
                key = edgeArgsToIdRaw(_isDirected, args[0], args[1], null);
            else
                key = edgeArgsToIdRaw(_isDirected, args[0], args[1], args[2]);

            if (!_edgeLabels.ContainsKey(key)) return null;
            return _edgeLabels[key];

        }
        public string edgeArgsToIdRaw(bool isDirected, object v_, object w_, object name)
        {
            var v = "" + v_;
            var w = "" + w_;
            if (!isDirected && string.CompareOrdinal(v, w) > 0)
            {
                var tmp = v;
                v = w;
                w = tmp;
            }
            return v + '\x01' + w + '\x01' + (name == null ? '\x00' : name);
        }
        internal DagreGraph removeNode(string v)
        {
            if (nodesRaw().Contains(v))
            {
                _nodesRaw.Remove(v);
                if (_isCompound)
                {
                    if (_parent.ContainsKey(v))
                    {
                        dynamic t1 = _children[(string)_parent[v]];
                        t1.Remove(v);
                        //delete this._children[this._parent[v]][v];                    
                        _parent.Remove(v);
                    }
                    else if (_children.ContainsKey((string)GRAPH_NODE))
                    {
                        dynamic t1 = _children[(string)GRAPH_NODE];

                        t1.Remove(v);
                    }
                    foreach (var child in children(v))
                    {
                        setParent(child);
                    }
                    _children.Remove(v);
                }
                var keys2 = (((dynamic)_in[v]).Keys as ICollection<string>).ToArray();
                foreach (var e in keys2)
                {
                    this.removeEdge(new object[] { this._edgeObjs[e] });
                }
                _in.Remove(v);
                _predecessors.Remove(v);

                var keys = (((dynamic)_out[v]).Keys as ICollection<string>).ToArray();
                foreach (var e in keys)
                {
                    this.removeEdge(new object[] { this._edgeObjs[e] });
                }
                _out.Remove(v);
                _successors.Remove(v);
                --_nodeCount;
            }
            return this;
        }
        internal void removeNode(DagreNode v)
        {
            throw new NotImplementedException();
        }

        internal int nodeCount()
        {
            return _nodeCount;
        }

        internal object[] inEdges(string v, string u = null)
        {
            dynamic inV = null;
            if (_in.ContainsKey(v))
            {
                inV = (dynamic)_in[v];
                //var edges = inV.Values.ToArray();
                var edges = inV.Values;
                if (u == null)
                {
                    return edges;
                }
                return edgesRaw().Where(edge => ((dynamic)edge)["v"] == u).ToArray();
            }
            return null;

        }

        public Func<object, dynamic> _defaultNodeLabelFn = (t) => { return null; };

        
        public DagreGraph setNode(object v, object o2 = null)
        {
            if (_nodesRaw.ContainsKey(v as string))
            {
                if (o2 != null)
                {
                    _nodesRaw[v as string] = o2;

                }
                return this;
            }
            else
            {
                _nodesRaw.Add(v as string, o2);
            }


            _nodesRaw[v as string] = (o2 != null ? o2 : _defaultNodeLabelFn(v));
            if (_isCompound)
            {
                addOrUpdate(v as string, _parent, GRAPH_NODE);

                if (!_children.ContainsKey(v as string))
                {
                    _children.Add(v as string, null);
                }
                _children[v as string] = new JavaScriptLikeObject();
                if (!_children.ContainsKey(GRAPH_NODE as string))
                {
                    _children.Add(GRAPH_NODE as string, new JavaScriptLikeObject());
                }
                addOrUpdate(v as string, _children[GRAPH_NODE as string], true);

            }

            addOrUpdate(v as string, _in, new JavaScriptLikeObject());
            addOrUpdate(v as string, _predecessors, new JavaScriptLikeObject());
            addOrUpdate(v as string, _out, new JavaScriptLikeObject());
            addOrUpdate(v as string, _successors, new JavaScriptLikeObject());



            _nodeCount++;
            return this;
        }
        public void addOrUpdate(string key, Dictionary<string, Dictionary<string, object>> dic, object obj)
        {
            if (!dic.ContainsKey(key))
            {
                dic.Add(key, null);
            }
            dic[key] = obj as Dictionary<string, object>;
        }
        public static void addOrUpdate(string key, dynamic dic, object obj)
        {
            if (!dic.ContainsKey(key))
            {
                dic.Add(key, null);
            }
            dic[key] = obj;
        }

        int _nodeCount;

        public DagreGraph setGraph(object v)
        {
            _label = v;
            return this;
        }
        internal void setParent2(string v, object parent = null)
        {
            setParent(v, (string)parent);
        }
        internal void setParent(string v, string parent = null)
        {
            if (!_isCompound)
            {
                throw new DagreException("cannot set parent in non-compound graph");
            }
            if (parent == null)
            {
                parent = (string)GRAPH_NODE;
            }
            else
            {
                // Coerce parent to string
                parent += "";
                for (var ancestor = parent; ancestor != null; ancestor = (string)this.parent(ancestor))
                {
                    if (ancestor == v)
                    {
                        throw new DagreException("Setting " + parent + " as parent of " + v + " would create a cycle.");
                    }
                }
                this.setNode(parent);
            }
            setNode(v);
            if (_parent.ContainsKey(v))
            {
                dynamic t1 = _children[_parent[v] as string];
                t1.Remove(v);
                _parent[v] = parent;
            }
            dynamic t2 = _children[parent as string];
            t2[v] = true;

        }



        public JavaScriptLikeObject _children = new JavaScriptLikeObject();
        public JavaScriptLikeObject _predecessors = new JavaScriptLikeObject();
        public JavaScriptLikeObject _successors = new JavaScriptLikeObject();
        public JavaScriptLikeObject _in = new JavaScriptLikeObject();
        public JavaScriptLikeObject _out = new JavaScriptLikeObject();
        //public static object GRAPH_NODE = "undefined";
        public static string GRAPH_NODE = "\x00";

        public JavaScriptLikeObject _parent = new JavaScriptLikeObject();
        internal object parent(string v)
        {
            if (_isCompound)
            {
                if (_parent.ContainsKey(v))
                {
                    dynamic parent = _parent[v];
                    if (parent != GRAPH_NODE)
                        return parent;
                }
            }
            return null;
        }
    }

    public class DagreBase
    {
        internal double? width;
        internal double? height;
        internal double? x;
        internal double? y;

    }
    public class DagreNode : DagreBase
    {
        public string id;
        public string key;
        public List<SelfEdgeInfo> selfEdges = null;
        public int? rank;

        internal string borderTop;
        internal int? minRank;
        internal int? maxRank;
        internal List<object> borderLeft;
        internal List<object> borderRight;
        internal string dummy;
        internal DagreEdgeIndex e;
        internal int? order;
        internal DagreEdge edgeLabel;
        internal string edgeObj;
        internal string _class;
        internal int? low;
        internal int? lim;
        internal object parent;
        internal DagreLabel label;
    }
    public class SelfEdgeInfo
    {
        public object label;
        public object e;
    }
    public class DagreEdge : DagreBase
    {
        public string forwardName;
        public string key;
        public double minlen;
        public int weight;
        public double width;
        public double height;
        public int labeloffset;
        public string label;
        public string arrowhead;
        public string id;
        public string labelpos;
        internal bool reversed;

        internal List<dPoint> points = new List<dPoint>();
        internal double? labelRank;
    }

    public class dPoint : DagreBase
    {

    }
    public class DagreEdgeIndex
    {
        public string v;
        public string w;
        public string name;
    }

    public class DagreLabel : DagreBase
    {
        public string labelpos = "r";
        public int weight = 1;

        public int labeloffset = 10;
        public int minlen = 1;

        internal List<dPoint> points = new List<dPoint>();
        public int edgesep;
        public int nodesep;
        public string rankdir;
        public int ranksep;
        public string ranker;
        public string acyclicer;

        internal int maxRank;
        internal string nestingRoot;
        internal int nodeRankFactor;
        internal List<string> dummyChains;
        internal string root;
        public string forwardName;
        internal bool? reversed;
        internal int? cutvalue;
        internal int? labelRank;
        internal object nesingEdge;
        internal double? marginx;
        internal double? marginy;

        public static bool Compare(dynamic obj1, dynamic obj2)
        {
            if (!obj1.ContainsKey("root") && obj2.ContainsKey("root")) throw new DagreException("wrong");
            if (obj1.ContainsKey("root") && !obj2.ContainsKey("root")) throw new DagreException("wrong");
            //if (obj1.Keys.Count != obj2.Keys.Count) throw new DagreException("wrong");
            for (int i = 0; i < obj1.Keys.Count; i++)
            {
                var tt = obj1 as IDictionary<string, object>;
                var key = tt.Keys.ToArray()[i];

                var v1 = obj1[key];
                var v2 = obj2[key];
                if (v1 is Array)
                {
                    for (int ii = 0; ii < v1.Length; ii++)
                    {
                        if (v1[ii] != v2[ii]) throw new DagreException("wrong");
                    }
                }
                else if (v1 is IDictionary<string, object>)
                {
                    var keys = v1.Keys;
                    foreach (var item in keys)
                    {
                        if (!v2.ContainsKey(item)) throw new DagreException("wrong");
                        var val1 = v1[item];
                        var val2 = v2[item];
                        if (val1 != val2) throw new DagreException("wrong");

                    }
                }
                else if (v1 != v2) throw new DagreException("wrong");
            }
            //if (obj1["ranker"] != obj2["ranker"]) return false;
            //if (obj1["nestingRoot"] != obj2["nestingRoot"]) return false;
            //if (obj1["nestingRoot"] != obj2["nestingRoot"]) return false;
            return true;
        }
        internal bool Compare(DagreLabel label)
        {
            if (ranker != label.ranker) return false;
            if (ranksep != label.ranksep) return false;
            if (nestingRoot != label.nestingRoot) return false;

            return true;
        }
    }


    public class DagreException : Exception
    {

        public DagreException() { }
        public DagreException(string str) : base(str)
        {

        }
    }
}
