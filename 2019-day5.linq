<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

var machine = new IntCodeMachine();

{
	machine.TakeInput(1);
	machine.Run();
}

{
	machine.Reset();
	machine.TakeInput(5);
	machine.Run();
}
