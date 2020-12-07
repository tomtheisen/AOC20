<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var input = ReadLines();

var rules = new Dictionary<string, List<(int count, string name)>>();

foreach (var line in input) {
	var parts = line.Split(" bags contain ");
	var contents = Regex.Matches(parts[1], @"(\d+) (\w+ \w+) bags?\b")
		.Select(r => (int.Parse(r.Groups[1].Value), r.Groups[2].Value))
		.ToList();
	rules.Add(parts[0], contents);
}

var containers = rules
	.SelectMany(r => r.Value.Select(v => (r.Key, v.name)))
	.ToLookup(t => t.name, t => t.Key);

var outers = new HashSet<string>();

void Visit(string bag) {
	if (!outers.Add(bag)) return;
	foreach (var outer in containers[bag]) Visit(outer);
}
Visit("shiny gold");

(outers.Count - 1).DumpClip("Part 1");

int CountInnerBags(string bag) 
	=> rules[bag].Sum(rule => rule.count * (1 + CountInnerBags(rule.name)));

CountInnerBags("shiny gold").DumpClip("Part 2");
