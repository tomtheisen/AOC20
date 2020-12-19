<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

const int Size = 25;
var input = ReadLongs();

var q = new Queue<long>(input.Take(Size));
long invalid = 0;

foreach (var e in input.Skip(Size)) {
	var sums = Choose(q, 2).Select(x => x.Sum());
	if (!sums.Contains(e)) {
		(invalid = e).DumpClip("Part 1");
		break;
	}
	q.Dequeue();
	q.Enqueue(e);
}

int i = 0, j = 0;
for (long sum = 0; sum != invalid; ) {
	if (sum < invalid) sum += input[j++];
	else if (sum > invalid) sum -= input[i++];
}
var range = input[i..j];
long weakness = range.Min() + range.Max();
weakness.DumpClip("Part 2");
