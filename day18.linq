<Query Kind="Statements">
  <NuGetReference>Sprache</NuGetReference>
  <Namespace>Sprache</Namespace>
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\AOC2020"

var input = ReadLines();

{
	Parser<long> expression = null;
	var parenthesized = Parse.Ref(() => expression).Contained(Parse.Char('('), Parse.Char(')')).Token();
	var literal = Parse.Digit.AtLeastOnce().Token().Text().Select(long.Parse);
	var term = literal.Or(parenthesized);
	var @operator = Parse.Char('+').Or(Parse.Char('*'));
	expression = Parse.ChainOperator(@operator, term,
		(op, a, b) => checked(op switch { '+' => a + b, '*' => a * b }));
	input.Sum(expression.End().Parse).DumpClip("Part 1");
}

{
	Parser<long> expression = null;
	var parenthesized = Parse.Ref(() => expression).Contained(Parse.Char('('), Parse.Char(')')).Token();
	var literal = Parse.Digit.AtLeastOnce().Token().Text().Select(long.Parse);
	var term = literal.Or(parenthesized);
	var sum = Parse.ChainOperator(Parse.Char('+'), term, (_, a, b) => checked(a + b));
	expression = Parse.ChainOperator(Parse.Char('*'), sum, (_, a, b) => checked(a * b));
	input.Sum(expression.End().Parse).DumpClip("Part 2");
}
