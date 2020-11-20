<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

var asteroids = new Board(ReadString()).FindAll("#").ToList();

asteroids.Max(a => asteroids
	.Except(new [] {a})
	.Select(b => b - a)
	.Select(d => Atan2(d.DX, d.DY))
	.Distinct()
	.Count()
).Dump();

var pos = asteroids.MaxBy(a => asteroids
	.Except(new [] {a})
	.Select(b => b - a)
	.Select(d => Atan2(d.DX, d.DY))
	.Distinct()
	.Count()
);

asteroids.Remove(pos);

var target = asteroids
	.GroupBy(a => {
		var d = pos - a;
		return Atan2(-d.DX, -d.DY);
	})
	.OrderBy(a => -a.Key)
	.Select(g => g.OrderBy(pos.Manhattan))
	.Transpose()
	.SelectMany(g => g)
	.ElementAt(199);

Console.WriteLine(target.X * 100 + target.Y);