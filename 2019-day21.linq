<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

var machine = new IntCodeMachine { 
    Output = n => Console.Write(n < 127 ? "" + (char)n : n + "\n")
};

// part 1
machine.TakeInput("OR A J\n");
machine.TakeInput("AND B J\n");
machine.TakeInput("AND C J\n");
machine.TakeInput("NOT J J\n");
machine.TakeInput("AND D J\n");
machine.TakeInput("WALK\n");
machine.Run();

// part 2
machine.Reset();
machine.TakeInput("OR A J\n");
machine.TakeInput("AND B J\n");
machine.TakeInput("AND C J\n");
machine.TakeInput("NOT J J\n");
machine.TakeInput("AND D J\n");
machine.TakeInput("RUN\n");
machine.Run();
