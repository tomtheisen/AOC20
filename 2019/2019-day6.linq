<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

var orbits = new Dictionary<string, string>();

foreach (var line in ReadLines()) {
	var parts = line.Split(')');
	orbits[parts[1]] = parts[0];
}

{
	int CountOrbits(string body) 
		=> orbits.TryGetValue(body, out var center) ? 1 + CountOrbits(center) : 0;
	WriteLine(orbits.Keys.Sum(CountOrbits));
}

{
	List<string> PathTo(string body) {
		var result = new List<string>{ body };
		
		for (string curr = body; orbits.TryGetValue(curr, out curr); ) {
			result.Insert(0, curr);
		}
		
		return result;
	}
	
	List<string> you = PathTo("YOU"), san = PathTo("SAN");
	
	while (you[0] == san[0]) {
		you.RemoveAt(0);
		san.RemoveAt(0);
	}
	WriteLine(you.Count + san.Count - 2);
}
	