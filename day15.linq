<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var input = ReadLines()[0].Split(',').Select(v => int.Parse(v)).ToList();

Dictionary<int, int> lastSeen = new();
for (int i = 0; i < input.Count - 1; i++) {
	lastSeen[input[i]] = i + 1;
}

for (int i = input.Count, said = input[^1]; i < 30_000_000; i++) {
	int nextSaid = lastSeen.TryGetValue(said, out int previouslySaid) ? i - previouslySaid : 0;
	lastSeen[said] = i;
	said = nextSaid;
	if (i == 2020 - 1) said.DumpClip("Part 1");
	if (i == 30_000_000 - 1) said.DumpClip("Part 2");
}