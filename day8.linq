<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var input = ReadLines();

{
    int ip = 0, acc = 0;
    var seen = new HashSet<int>();
    
    while (seen.Add(ip)) {
        var (cmd, arg) = (input[ip][..3], int.Parse(input[ip][4..]));
        switch (cmd) {
            case "acc":
                acc += arg;
                ip += 1;
                break;
            case "jmp":
                ip += arg;
                break;
            case "nop":
                ip += 1;
                break;
        }
    }
    acc.Dump("Part 1");
}

for (int i = 0; i < input.Length; i++) {
    if (input[i].StartsWith("acc")) continue;
    
    var guess = input.ToArray();
    guess[i] = (guess[i].StartsWith("nop") ? "jmp" : "nop") + guess[i][3..];
    
    int ip = 0, acc = 0;
    var seen = new HashSet<int>();
    
    while (0 <= ip && ip < guess.Length && seen.Add(ip)) {
        var (cmd, arg) = (guess[ip][..3], int.Parse(guess[ip][4..]));
        switch (cmd) {
            case "acc":
                acc += arg;
                ip += 1;
                break;
            case "jmp":
                ip += arg;
                break;
            case "nop":
                ip += 1;
                break;
        }
    }
    if (ip == guess.Length) {
        acc.Dump("Part 2");
        break;
    }
}

