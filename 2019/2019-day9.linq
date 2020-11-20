<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

IntCodeMachine m = new() { OutputAction = WriteLine };
m.TakeInput(1);
m.Run();

m.Reset();
m.TakeInput(2);
m.Run();