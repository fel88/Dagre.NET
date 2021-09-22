namespace Dagre
{
    public class coordinateSystem
    {
        public static void undo(DagreGraph g)
        {
            var rankDir = g.graph()["rankdir"].ToLower();
            if (rankDir == "bt" || rankDir == "rl")
            {
                reverseY(g);
            }

            if (rankDir == "lr" || rankDir == "rl")
            {
                swapXY(g);
                swapWidthHeight(g);
            }
        }
        public static void reverseYOne(dynamic attrs)
        {
            attrs["y"] = -attrs["y"];
        }

        public static void reverseY(DagreGraph g)
        {
            foreach (var v in g.nodes())
            {
                reverseYOne(g.node(v));
            }
            foreach (var e in g.edges())
            {
                var edge = g.edge(e);
                foreach (var item in edge["points"])
                {
                    reverseYOne(item);
                }

                if (edge.y != null)
                {
                    reverseYOne(edge);
                }
            }
        }

        public static void swapXYOne(DagreBase attrs)
        {
            var x = attrs.x;
            attrs.x = attrs.y;
            attrs.y = x;
        }

        public static void swapXY(DagreGraph g)
        {
            foreach (var v in g.nodes())
            {
                swapXYOne(g.node(v));
            }

            foreach (var e in g.edges())
            {
                var edge = g.edge(e);
                foreach (var item in edge.points)
                {
                    swapXYOne(item);
                }

                if (edge.x != null)
                {
                    swapXYOne(edge);
                }
            }


        }
        public static void adjust(DagreGraph g)
        {
            var rankDir = g.graph()["rankdir"].ToLower();
            if (rankDir == "lr" || rankDir == "rl")
            {
                swapWidthHeight(g);
            }
        }


        public static void swapWidthHeightOne(dynamic attrs)
        {
            var w = attrs["width"];
            attrs["width"] = attrs["height"];
            attrs["height"] = w;
        }

        public static void swapWidthHeight(dynamic g)
        {
            foreach (var v in g.nodes())
            {
                swapWidthHeightOne(g.node(v));
            }
            foreach (var e in g.edges())
            {
                swapWidthHeightOne(g.edge(e));
            }

        }

    }
}
