<Query Kind="Statements" />

#load ".\AOC2020"

void Run(int[] mem) {
	int ip = 0;
	bool running = true;
	while (running) { 
		switch (mem[ip]) {
			case 1:
				mem[mem[ip + 3]] = mem[mem[ip + 1]] + mem[mem[ip + 2]];
				ip += 4;
				break;
			case 2:
				mem[mem[ip + 3]] = mem[mem[ip + 1]] * mem[mem[ip + 2]];
				ip += 4;
				break;
			case 99:
				running = false;
				break;
		}
	}
}

var input = ReadString().Split(',').Select(int.Parse).ToArray();
{
	var mem = input.ToArray();
	mem[1] = 12;
	mem[2] = 2;
	Run(mem);
	
	Console.WriteLine(mem[0]);
}

{
	for (int noun = 0; noun < 100; noun++) {
		for (int verb = 0; verb < 100; verb++) {
			var mem = input.ToArray();
			mem[1] = noun;
			mem[2] = verb;
			Run(mem);
			
			if (mem[0] == 19690720) Console.WriteLine(noun * 100 + verb);
		}
	}
}