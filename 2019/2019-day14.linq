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
	foreach(var line in ReadLines()) {
		var components = Regex.Matches(line, @"(\d+) (\w+)")
			.Select(r => (name: r.Groups[2].Value, quant: int.Parse(r.Groups[1].Value)))
			.ToArray();
		
		recipes[components[^1].name] = (components[^1].quant, components[..^1]);
		stock[components[^1].name] = 0;
	}
	
	bool Craft(string name, long quantity, Action<long>? oreMine = null) {
		if (stock[name] >= quantity) return true;

		if (name == "ORE") {
			if (oreMine is object) {
				oreMine(quantity - stock[name]);
				stock[name] = quantity;
				return true;
			}
			return false;
		}

		if (stock[name] < quantity) {
			long batches = (quantity - stock[name] + recipes[name].quant - 1) / recipes[name].quant;
			bool hadtomake;
			do {
				hadtomake = false;
				foreach (var (cname, cquant) in recipes[name].components) {
					if (stock[cname] < cquant * batches) {
						if (!Craft(cname, cquant * batches, oreMine)) return false;
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
	
	long oreMined = 0;
	Craft("FUEL", 1, ore => oreMined += ore);
	oreMined.Dump();
	
	foreach (var name in stock.Keys.ToList()) stock[name] = 0;
 	stock["ORE"] = 1_000_000_000_000;
	
	var verifiedStock = stock.Clone();
	long verifiedFuel = 0, target = 1;
	while(Craft("FUEL", target *= 2)) verifiedStock = stock.Clone();

	for(long tooMuch = target; tooMuch > verifiedFuel + 1; ) {
		target = tooMuch + verifiedFuel >> 1;
		stock = verifiedStock.Clone();
		if (Craft("FUEL", target)) (verifiedFuel, verifiedStock) = (stock["FUEL"], stock.Clone());
		else tooMuch = target;
	}
	
	verifiedFuel.Dump();
}
