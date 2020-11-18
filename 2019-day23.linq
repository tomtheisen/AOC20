<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

long GetMinusOneSlowly() {
    Thread.Sleep(10);
    return -1;
}

List<IntCodeMachine> machines = new();

for (int i = 0; i < 50; i++) {
    IntCodeMachine machine = new() { InputFallback = GetMinusOneSlowly, Output = _ => {} };
    machine.TakeInput(i);
    machines.Add(machine);
}

List<Task> tasks = new();

var sw = Stopwatch.StartNew();
int mi = 0;

CancellationTokenSource cts = new();
var token = cts.Token;

machines.ForEach(machine => {
    int thisAddress = mi++;
    tasks.Add(Task.Run(() => {
        while (!token.IsCancellationRequested) {
            long targetAddress = machine.RunToNextOutputOrThrow();
            long x = machine.RunToNextOutputOrThrow();
            long y = machine.RunToNextOutputOrThrow();
            
            Console.WriteLine($"{sw.Elapsed} {thisAddress:00}: To {targetAddress:00} {x,15}, {y,15}");
            
            if (0 <= targetAddress && targetAddress < 50) machines[(int)targetAddress].TakeInput(x, y);
            else if (targetAddress == 255) {
                y.Dump("Part 1");
                return;
            }
        }
    }, token));
});
    
Task.WaitAny(tasks.ToArray());
cts.Cancel();
