<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

var input = ReadString().Replace("\r","");

string[] required = "byr iyr eyr hgt hcl ecl pid".Split(' ');

input.Split("\n\n")
	.Select(pp => pp.Split(" \n".ToCharArray()))
	.Count(pp => required.All(r => pp.Any(f => f.StartsWith(r + ":"))))
	.Dump("Part 1");

string[] requirements = {
	@"byr:(19[2-9]\d|200[012])\b",
	@"iyr:(201\d|2020)\b",
	@"eyr:(202\d|2030)\b",
	@"hgt:((59|6\d|7[0-6])in|(1[5-8]\d|19[0-3])cm)\b",
	@"hcl:#[a-f0-9]{6}\b",
	@"ecl:(amb|blu|brn|gry|grn|hzl|oth)\b",
	@"pid:\d{9}\b",
};

input.Split("\n\n").Count(v => requirements.All(r => Regex.IsMatch(v, r))).Dump("Part 2");

