<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

string FFT(int[] signal, int times, int start, int length) {
	var current = signal.ToArray();
	var next = new int[signal.Length];
	next[^1] = current[^1];
	for (int i = 0; i < times; i++) {
		for (int j = signal.Length - 2; j >= start; j--) {
			if (j > signal.Length / 2) {
				next[j] = (next[j + 1] + current[j]) % 10;
			}
			else {
				int sum = 0;
				for (int k = j; k < signal.Length; k++) {
					int multiplier = ((k + 1) / (j + 1) % 4) switch { 1=>1, 3=>-1, _=>0 };
					sum += multiplier * current[k];
				}
				next[j] = Abs(sum % 10);
			}
		}
		(current, next) = (next, current);
	}
	return string.Concat(current.Skip(start).Take(length));
}

int[] input = ReadDigits();

FFT(input, 100, 0, 8).Dump();

var big = Enumerable.Repeat(input, 10_000).SelectMany(e => e).ToArray();
int start = int.Parse(ReadLines()[0][..7]);
FFT(big, 100, start, 8).Dump();
