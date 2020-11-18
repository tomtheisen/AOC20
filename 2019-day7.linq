<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

{
	long power, result = int.MinValue;
	IntCodeMachine machine = new() { OutputAction = o => power = o};
	
	foreach (var perm in Permutations(0,1,2,3,4)) {
		power = 0;
		foreach (int phase in perm) {
			machine.Reset();
			machine.TakeInput(phase, power);
			machine.Run();
		}
		result = Max(power, result);
	}
	WriteLine(result);
}

{
	var machines = new IntCodeMachine[5];
	for (int i = 0; i < 5; i++) {
		machines[i] = new IntCodeMachine();
		if (i > 0) machines[i - 1].OutputAction = machines[i].TakeInput;
	}
	machines[^1].OutputAction = machines[0].TakeInput;
	
	long result = int.MinValue;
	foreach (var perm in Permutations(5,6,7,8,9)) {
		for (int i = 0; i < 5; i++) {
			machines[i].Reset();
			machines[i].TakeInput(perm[i]);
		}
		machines[0].TakeInput(0);
		var tasks = machines.Select(m => Task.Factory.StartNew(() => m.Run())).ToArray();
		Task.WaitAll(tasks);
		result = Max(result, machines[0].Input.TryTake(out long t) ? t : throw new Exception());
	}
	WriteLine(result);
}
