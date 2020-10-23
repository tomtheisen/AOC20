<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

List<Moon> GetInput() => ReadLines()
		.Select(line => Regex.Matches(line, @"-?\d+")
						.Select(m => int.Parse(m.Value))
						.ToArray())
		.Select(t => new Moon { X = t[0], Y = t[1], Z = t[2] })
		.ToList();

void Main() {
	var moons = GetInput();
	
	void SimulateTick() {
		foreach (var moon in moons) {
			moon.DX += moons.Sum(other => Sign(other.X - moon.X));
			moon.DY += moons.Sum(other => Sign(other.Y - moon.Y));
			moon.DZ += moons.Sum(other => Sign(other.Z - moon.Z));
		}
		foreach (var moon in moons) moon.Step();
	}
	
	for (int i = 0; i < 1000; i++) SimulateTick();
	moons.Sum(m => m.Energy()).Dump();
	
	moons = GetInput();
	long xloop = -1, yloop = -1, zloop = -1;
	for (long step = 1; xloop < 0 || yloop < 0 || zloop < 0; ++step) {
		SimulateTick();
		if (moons.All(m => m.DX == 0) && xloop < 0) xloop = step;
		if (moons.All(m => m.DY == 0) && yloop < 0) yloop = step;
		if (moons.All(m => m.DZ == 0) && zloop < 0) zloop = step;
	}
	Console.WriteLine(xloop * yloop * zloop);
}


class Moon {
	public int X;
	public int Y;
	public int Z;
	public int DX;
	public int DY;
	public int DZ;
	
	public void Step() {
		X += DX;
		Y += DY;
		Z += DZ;
	}
	
	public int Energy() {
		return (Abs(X) + Abs(Y) + Abs(Z)) * (Abs(DX) + Abs(DY) + Abs(DZ));
	}
}
