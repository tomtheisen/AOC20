<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

string input = ReadLines()[0];


var current = input.Select(i => i - 48).ToArray();
var next = new int[input.Length];
for (int i = 0; i < 100; i++) {
	for (int j = 0; j < input.Length; j++) {
		if (j > input.Length / 2) {
			next[j] = (next[j - 1] - current[j - 1] + 10) % 10;
		}
		else {
			int sum = 0;
			for (int k = j; k < input.Length; k++) {
				int multiplier = ((k + 1) / (j + 1) % 4) switch { 1=>1, 3=>-1, _=>0 };
				sum += multiplier * current[k];
			}
			next[j] = Abs(sum % 10);
		}
	}
	(current, next) = (next, current);
	
	//string.Concat(current).Dump();
}

string.Concat(current[..8]).Dump();
