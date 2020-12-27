<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var input = ReadLines();

{
	var mem = new Dictionary<long, long>();
	
	long maskzeroes = 0, maskones = 0;
	
	foreach (var instr in input) {
		if (instr.StartsWith("mask = ")) {
			maskzeroes = maskones = 0;
			for (int i = 0; i < 36; i++) {
				switch (instr[^(i + 1)]) {
					case '0': maskzeroes |= 1L << i; break;
					case '1': maskones   |= 1L << i; break;
				}
			}
		}
		else {
			var parts = Regex.Matches(instr, @"\d+")
				.Select(e => long.Parse(e.Value))
				.ToList();
			long masked = parts[1] & ~maskzeroes | maskones;
			mem[parts[0]] = masked;
		}
	}
	mem.Values.Sum().DumpClip("Part 1");
}

{
	var mem = new Dictionary<long, long>();
	
	long maskones = 0;
	List<int> maskfloats = new();

	foreach (var instr in input) {
		if (instr.StartsWith("mask = ")) {
			maskones = 0;
			maskfloats.Clear();
			for (int i = 0; i < 36; i++) {
				switch (instr[^(i + 1)]) {
					case 'X': maskfloats.Add(i); break;
					case '1': maskones |= 1L << i; break;
				}
			}
		}
		else {
			var parts = Regex.Matches(instr, @"\d+")
				.Select(e => long.Parse(e.Value))
				.ToList();
				
			var addresses = new List<long> { parts[0] | maskones };
			foreach (int f in maskfloats) {
				addresses.AddRange(addresses
					.Select(addr => addr ^ 1L << f)
					.ToList());
			}
			foreach (var addr in addresses) {
				mem[addr] = parts[1];
			}
		}
	}
	mem.Values.Sum().DumpClip("Part 2");
}
