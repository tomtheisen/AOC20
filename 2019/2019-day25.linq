<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

StringBuilder sb = new();
IntCodeMachine machine = new() { OutputAction = o => {
	Write((char)o);
	sb.Append((char)o);
}};

machine.TakeInput(@"
south
take spool of cat6
west
take space heater
north
take weather machine
north
west
west
take whirled peas
east
east
south
west
south
east
take candy cane
west
south
take space law space brochure
north
north
east
south
south
take shell
north
east
east
south
take hypercube
south
south
".Replace("\r", ""));
machine.RunToNextBlockedInput();

string[] inv = @"
- weather machine
- shell
- candy cane
- whirled peas
- hypercube
- space law space brochure
- space heater
- spool of cat6".Split("\r\n")
	.Skip(1).Select(c => c[2..])
	.ToArray().Dump();

for (int i = 0; i < 256; i++) {
	var clone = machine.Clone();
	for (int j = 0; j < 8; j++) {
		if ((i >> j & 1) > 0) clone.TakeInput("drop " + inv[j] + "\n");
	}
	clone.TakeInput("east\n");
	clone.RunToNextBlockedInput();
}

Regex.Match(sb.ToString(), @".*\d{3}.*").Dump();