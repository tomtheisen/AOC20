<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var input = ReadLines()[0].Split(',').Select(v => int.Parse(v)).ToList();

var lastSeen = new int[30_000_000];
for (int i = 0; i < input.Count - 1; i++) lastSeen[input[i]] = i + 1;

int said = input[^1];
for (int i = input.Count; i < 30_000_000; i++) {
	int previouslySaid = lastSeen[said];
	int nextSaid = previouslySaid > 0 ? i - previouslySaid : 0;
	lastSeen[said] = i;
	said = nextSaid;
	if (i == 2020 - 1) said.DumpClip("Part 1");
}
said.DumpClip("Part 2");
