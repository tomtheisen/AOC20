<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

void Main() {
	var recipes = new Dictionary<string, (int quant, (string name, int quant)[] components)>();
	var stock = new Dictionary<string, long>{["ORE"] = 0};
	long oreMined = 0;
	
	foreach(var line in ReadLines()) {
		var components = Regex.Matches(line, @"(\d+) (\w+)")
			.Select(r => (name: r.Groups[2].Value, quant: int.Parse(r.Groups[1].Value)))
			.ToArray();
		
		recipes[components[^1].name] = (components[^1].quant, components[..^1]);
		stock[components[^1].name] = 0;
	}
	
	void CraftOrMine(string name, int quantity) {
		if (stock[name] >= quantity) return;

		if (name == "ORE") {
			oreMined += quantity - stock[name];
			stock[name] = quantity;
			return;
		}
		
		while (stock[name] < quantity) {
			while (recipes[name].components.Any(c => stock[c.name] < c.quant)) {
				foreach (var (n, q) in recipes[name].components) {
					CraftOrMine(n, q);
				}
			}
			
			foreach (var (n, q) in recipes[name].components) {
				stock[n] -= q;
			}
			stock[name] += recipes[name].quant;
		}
	}
	
	bool Craft(string name, int quantity) {
		if (stock[name] >= quantity) return true;

		while (stock[name] < quantity) {
			while (recipes[name].components.FirstOrDefault(c => stock[c.name] < c.quant) is (string n, int q)) {
				if (n == "ORE") return false;
				if (!Craft(n, q)) return false;
			}
			
			foreach (var (n, q) in recipes[name].components) {
				stock[n] -= q;
			}
			stock[name] += recipes[name].quant;
		}
		return true;
	}
	
	CraftOrMine("FUEL", 1);
	oreMined.Dump();
	
	foreach (var name in stock.Keys.ToList()) stock[name] = 0;
 	//stock["ORE"] = 1_000_000_000_000;
 	stock["ORE"] = 1_000_000_000;
	Craft("FUEL", int.MaxValue);
	stock["FUEL"].Dump();
}

// You can define other methods, fields, classes and namespaces here
