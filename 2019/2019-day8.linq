<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

var input = ReadString().Trim().BatchBy(25 * 6);

var layer = input.MinBy(layer => layer.Count('0'));
Console.WriteLine(layer.Count('1') * layer.Count('2'));

new Board(
	input.Transpose()
		.Select(p => p.First(v => v < '2'))
		.Select(c => " *"[c - '0'])
		.BatchBy(25)
		.Select(_ => string.Concat(_))
).Dump();