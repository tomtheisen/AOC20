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
    var container = new DumpContainer().Dump("Machine " + i);
    IntCodeMachine machine = new() { Output = _ => {} };
    int inputFallbacks = 0;
    machine.InputFallback = () => {
        container.Content = new { machine.StepsExecuted, InputFallbacks = ++inputFallbacks };
        if (machine.Input.TryTake(out long item, 100)) return item;
        return -1;
    };
    machine.TakeInput(i);
    machines.Add(machine);
}

List<Task> tasks = new();

int mi = 0;
CancellationTokenSource cts = new();
var token = cts.Token;

machines.ForEach(machine => {
    int thisAddress = mi++;
    
    void OperateMachine() {
        while (!token.IsCancellationRequested) {
            long targetAddress = machine.RunToNextOutputOrThrow(token);
            long x = machine.RunToNextOutputOrThrow(token);
            long y = machine.RunToNextOutputOrThrow(token);
            
            if (0 <= targetAddress && targetAddress < 50) machines[(int)targetAddress].TakeInput(x, y);
            else if (targetAddress == 255) {
                y.Dump("Part 1");
                return;
            }
        }
    }
    
    tasks.Add(Task.Run(OperateMachine, token));
});
    
Task.WaitAny(tasks.ToArray());
cts.Cancel();
