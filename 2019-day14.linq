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
		
		if (stock[name] < quantity) {
			int batches = (int)((quantity - stock[name] + recipes[name].quant - 1) / recipes[name].quant);
			
			while (recipes[name].components.Any(c => stock[c.name] < c.quant * batches)) {
				foreach (var (n, q) in recipes[name].components) {
					CraftOrMine(n, q * batches);
				}
			}
			
			foreach (var (n, q) in recipes[name].components) {
				stock[n] -= q * batches;
			}
			stock[name] += recipes[name].quant * batches;
		}
	}
	
	bool Craft(string name, long quantity) {
		if (stock[name] >= quantity) return true;

		if (stock[name] < quantity) {
			long batches = (quantity - stock[name] + recipes[name].quant - 1) / recipes[name].quant;
			bool hadtomake;
			do {
				hadtomake = false;
				foreach (var (cname, cquant) in recipes[name].components) {
					if (stock[cname] < cquant * batches) {
						if (cname == "ORE") return false;
						if (!Craft(cname, cquant * batches)) return false;
						hadtomake = true;
					}
				}
			} while (hadtomake);
			
			foreach (var (n, q) in recipes[name].components) {
				stock[n] -= q * batches;
			}
			stock[name] += recipes[name].quant * batches;
		}
		return true;
	}
	
	CraftOrMine("FUEL", 1);
	oreMined.Dump();
	
	foreach (var name in stock.Keys.ToList()) stock[name] = 0;
 	stock["ORE"] = 1_000_000_000_000;
	var oreContainer = new DumpContainer().Dump("ORE");
	var fuelContainer = new DumpContainer().Dump("FUEL");
	
	long verifiedFuel;
	Dictionary<string, long> verifiedStock;
	long target = 1;
	while(Craft("FUEL", target *= 2)) {
		oreContainer.Content = stock["ORE"];
		fuelContainer.Content = verifiedFuel = stock["FUEL"];
		verifiedStock = stock.Clone();
	}
	stock["FUEL"].Dump();
}
