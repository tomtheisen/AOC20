<Query Kind="Statements" />

#load ".\AOC2020"

{
	int result = 0;
	foreach (var e in ReadInts()) result += e / 3 - 2;
	Console.WriteLine(result);
}

{
	int result = 0;
	foreach (var e in ReadInts()) {
		for (int fuel = e; fuel > 0; ) result += fuel = fuel / 3 - 2;
	}
	Console.WriteLine(result);
}
