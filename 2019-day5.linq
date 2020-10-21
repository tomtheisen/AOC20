<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

var machine = new IntCodeMachine();

{
	machine.Input.Enqueue(1);
	machine.Run();
}

{
	machine.Reset();
	machine.Input.Enqueue(5);
	machine.Run();
}
