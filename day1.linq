<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Windows</Namespace>
</Query>

#load ".\AOC2020"

// L{em2m`_}L`pq[^S{|+2020=}jc": Π(`|u) = `:*"

var input = ReadLongs();

(
	from x in input
	from y in input
	where x + y == 2020
	select x * y
).First().DumpClip("Part 1");

(
	from x in input
	from y in input
	from z in input
	where x + y + z == 2020
	select x * y * z
).First().DumpClip("Part 2");
