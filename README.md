# Dagre.NET

C# port of https://github.com/dagrejs/dagre

Available as NuGet Package: https://www.nuget.org/packages/Dagre.NET/1.0.0.6

## Usage
Simple test:
```javascript
DagreInputGraph dg = new DagreInputGraph();
var nd1 = dg.AddNode();
var nd2 = dg.AddNode();
dg.AddEdge(nd1, nd2);
dg.Layout(); 
Console.WriteLine($"node1 : {nd1.X} {nd1.Y}");
Console.WriteLine($"node2 : {nd2.X} {nd2.Y}");

```

Simple test #2:
```javascript
DagreInputGraph dg = new DagreInputGraph();
var nd1 = dg.AddNode(new { Name = "input" }, 100, 20);
var nd2 = dg.AddNode(new { Name = "node1" }, 150, 30);
var nd3 = dg.AddNode(new { Name = "node2" }, 150, 30);
var nd4 = dg.AddNode(new { Name = "output" }, 100, 20);
dg.AddEdge(nd1, nd2, 2);
dg.AddEdge(nd2, nd3);
dg.AddEdge(nd3, nd4, 2);
dg.Layout();

Console.WriteLine($"{((dynamic)nd1.Tag).Name} : {nd1.X} {nd1.Y}");
Console.WriteLine($"{((dynamic)nd2.Tag).Name} : {nd2.X} {nd2.Y}");
Console.WriteLine($"{((dynamic)nd3.Tag).Name} : {nd3.X} {nd3.Y}");
Console.WriteLine($"{((dynamic)nd4.Tag).Name} : {nd4.X} {nd4.Y}");
```

Sample image (googlenet-3) taken from Dendrite (https://github.com/fel88/Dendrite)

<img src="imgs/1.png"/>
