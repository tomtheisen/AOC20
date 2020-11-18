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

ManualResetEventSlim mres = new();

object sync = new();

for (int i = 0; i < 50; i++) {
    IntCodeMachine machine = new() { Output = _ => {} };
    //var container = new DumpContainer().Dump("Machine " + i);
    int inputFallbacks = 0;
    machine.InputFallback = () => {
        //lock (sync) container.Content = new { 
        //    machine.IP,
        //    machine.StepsExecuted, 
        //    InputFallbacks = ++inputFallbacks };
        if (mres.IsSet) throw new("abort");
        lock (machine) {
            if (machine.Input.TryTake(out long item, 100)) return item;
        }
        return -1;
    };
    machine.TakeInput(i);
    machines.Add(machine);
}

List<Thread> threads = new();

int mi = 0;

machines.ForEach(machine => {
    int thisAddress = mi++;
    
    void OperateMachine() {
        try {	        
            while (!mres.IsSet) {
                long targetAddress = machine.RunToNextOutputOrThrow();
                long x = machine.RunToNextOutputOrThrow();
                long y = machine.RunToNextOutputOrThrow();
                
                if (0 <= targetAddress && targetAddress < 50) lock (machine) {
                    machines[(int)targetAddress].TakeInput(x, y);
                    Console.WriteLine($"{thisAddress} -> {targetAddress}: ({x}, {y})");
                }
                else if (targetAddress == 255) {
                    y.Dump("Part 1");
                    mres.Set();
                    return;
                }
            }
        }
        catch (Exception ex) when (ex.Message == "abort") {
            return;
        }
    }
    
    Thread thread = new(OperateMachine);
    thread.Start();
    threads.Add(thread);
});

mres.Wait();

