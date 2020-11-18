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
    IntCodeMachine machine = new() { 
        //OutputAction = _ => {} 
    };
    machine.TakeInput(i);
    machines.Add(machine);
}

Queue<IntCodeMachine> work = new(machines);

while (true) {
    var machine = work.Dequeue();
    
    machine.RunToNextBlockedInput();
    
    while (machine.Output.Count > 0) {
        long targetAddress = machine.Output.Dequeue();
        long x = machine.Output.Dequeue();
        long y = machine.Output.Dequeue();

        if (0 <= targetAddress && targetAddress < 50) {
            // new {targetAddress, x, y}.Dump();
        
            var target = machines[(int)targetAddress];
            target.TakeInput(x, y);
            work.Enqueue(target);
        }
        else if (targetAddress == 255) {
            y.Dump("Part 1");
            return;
        }
    }
    
    if (work.Count == 0) {
        foreach (var m in machines) {
            m.TakeInput(-1);
            work.Enqueue(m);
        }
    }
}

machines.Select((m, i) => new { 
    IP = (int)m.Uncapsulate().IP, 
    Upcoming = m.Memory.Skip((int)m.Uncapsulate().IP).Take(10) 
})
.Dump();