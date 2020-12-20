<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var input = ReadInts().ToHashSet();

/*
Part 1
L{emoZ+c|M3++:-o:G:*
*/

int target = input.Max() + 3;
input.Add(target);

long[] sub = new long[target + 1];
sub[0] = 1;

for (int i = 1; i <= target; i++) checked {
	if (!input.Contains(i)) continue;
	sub[i] = 
		+ sub.ElementAtOrDefault(i - 1)
		+ sub.ElementAtOrDefault(i - 2)
		+ sub.ElementAtOrDefault(i - 3);
}

sub[^1].DumpClip("Part 2");
