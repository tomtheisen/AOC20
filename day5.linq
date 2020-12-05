<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var input = ReadLines();

int SeatId(string pass) => 
	Convert.ToInt32(pass
		.Replace('F','0').Replace('B','1')
		.Replace('L', '0').Replace('R', '1'), 2);

input.Max(SeatId).Dump("Part 1");

var seats = input.Select(SeatId).ToHashSet();

Enumerable.Range(0, 1024)
	.Single(e => !seats.Contains(e) && seats.Contains(e - 1) && seats.Contains(e + 1))
	.Dump("Part 2");
