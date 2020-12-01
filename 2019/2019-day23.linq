<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

List<IntCodeMachine> machines = new();
for (int i = 0; i < 50; i++) {
	IntCodeMachine machine = new();
	machine.TakeInput(i);
	machines.Add(machine);
}

Queue<IntCodeMachine> work = new(machines);
HashSet<long> seennaty = new();
long natx = long.MinValue, naty = long.MinValue;

for (bool done = false; !done; ) {
	var machine = work.Dequeue();
	
	machine.RunToNextBlockedInput();
	
	while (machine.Output.Count >= 3) {
		long targetAddress = machine.Output.Dequeue();
		long x = machine.Output.Dequeue();
		long y = machine.Output.Dequeue();

		if (0 <= targetAddress && targetAddress < 50) {
			var target = machines[(int)targetAddress];
			target.TakeInput(x, y);
			work.Enqueue(target);
		}
		else if (targetAddress == 255) {
			if (naty == long.MinValue) y.Dump("Part 1");
			(natx, naty) = (x, y);
		}
	}
	
	if (work.Count == 0) {
		foreach (var m in machines) {
			m.TakeInput(-1);
			work.Enqueue(m);
		}
		if (!seennaty.Add(naty)) {
			naty.Dump("Part 2");
			return;
		}
		machines[0].TakeInput(natx, naty);
	}
}
