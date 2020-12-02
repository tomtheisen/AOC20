<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

// stax
/*
LcZ{
  GsH#
  x2(
  {eF^:b+
F"Part 1"PP
{
  Gs1T{evm@
  x2@#2%
  +
F"Part 2"PP

}
.- |t':-jXN
*/

var input = ReadLines();

int compliant1 = 0, compliant2 = 0;
foreach (var line in input) {
	var parts = line.Split("- :".ToCharArray());
	int min = int.Parse(parts[0]), max = int.Parse(parts[1]);
	char c = parts[2][0];
	string password = parts[4];
	int count = password.Count(t => t == c);
	
	if (count >= min && count <= max) ++compliant1;
	if (password[min-1] == c ^ password[max-1] ==c) ++compliant2;
}
compliant1.Dump("Part 1");
compliant2.Dump("Part 2");
