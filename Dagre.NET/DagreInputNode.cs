using System.Collections.Generic;

namespace Dagre
{
    public class DagreInputNode
    {
        public DagreInputNode Group;
        public List<DagreInputNode> Childs = new List<DagreInputNode>();
        public List<DagreInputNode> Parents = new List<DagreInputNode>();
        public object Tag;
        public float Width = 300;
        public float Height = 100;
        public float X;
        public float Y;
    }
}
