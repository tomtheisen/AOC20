<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
</Query>

#load ".\AOC2020"
#load ".\IntCode"

var machine = new IntCodeMachine();

{
	machine.Memory[1] = 12;
	machine.Memory[2] = 2;
	machine.Run();
	
	WriteLine(machine.Memory[0]);
}

{
	for (int noun = 0; noun < 100; noun++) {
		for (int verb = 0; verb < 100; verb++) {
			machine.Reset();
			machine.Memory[1] = noun;
			machine.Memory[2] = verb;
			machine.Run();
			
			if (machine.Memory[0] == 19690720) WriteLine(noun * 100 + verb);
		}
	}
}