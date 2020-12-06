<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var input = ReadString();

var groups = input.Replace("\r","").Split("\n\n");

int total1 = 0, total2 = 0;

foreach (var g in groups) {
    total1 += string.Concat(g.Split()).Distinct().Count();
    total2 += g.Split().Aggregate ((x, y) => string.Concat(x.Intersect(y))).Length;
}

total1.Dump("Part 1");
total2.Dump("Part 2");
