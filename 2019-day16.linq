<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

string input = ReadLines()[0];


string current = input;
for (int i = 0; i < 100; i++) {
	string next = "";
	for (int j = 0; j < input.Length; j++) {
		int sum = 0;
		for (int k = 0; k < input.Length; k++) {
			int multiplier = ((k + 1) / (j + 1) % 4) switch { 1=>1, 3=>-1, _=>0 };
			sum += multiplier * (current[k] - 48);
		}
		next += Abs(sum % 10);
	}
	current = next;
}

current[..8].Dump();
