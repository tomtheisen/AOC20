<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\AOC2020"
const int Margin = 7;

var input = ReadLines();

int width = input[0].Length + Margin * 2, height = input[0].Length + Margin * 2, depth = Margin * 2 + 1;

{
	var cube = new bool[width, height, depth];
	for (int i = 0; i < input.Length; i++) {
		for (int j = 0; j < input[i].Length; j++) {
			cube[j + Margin, i + Margin, Margin] = input[j][i] == '#';
		}
	}
	
	for (int i = 0; i < 6; i++) {
		var next = (bool[,,])cube.Clone();
		for (int x = 1; x < width - 1; x++) {
			for (int y = 1; y < height - 1; y++) {
				for (int z = 1; z < depth - 1; z++) {
					int neighbors = 0;
					bool set = cube[x, y, z];
	
					for (int nx = x - 1; nx <= x + 1; nx++) {
						for (int ny = y - 1; ny <= y + 1; ny++) {
							for (int nz = z - 1; nz <= z + 1; nz++) {
								if (x == nx && y == ny && z == nz) continue;
								neighbors += cube[nx, ny, nz] ? 1 : 0;
							}
						}
					}
					
					next[x, y, z] = neighbors == 3 || set && neighbors == 2;
				}
			}
		}
		
		cube = next;
	}
	
	cube.Cast<bool>().Count(true).Dump("Part 1");
}

{
	var hypercube = new bool[width, height, depth, depth];
	for (int i = 0; i < input.Length; i++) {
		for (int j = 0; j < input[i].Length; j++) {
			hypercube[j + Margin, i + Margin, Margin, Margin] = input[j][i] == '#';
		}
	}
	
	for (int i = 0; i < 6; i++) {
		var next = (bool[,,,])hypercube.Clone();
		for (int x = 1; x < width - 1; x++) {
			for (int y = 1; y < height - 1; y++) {
				for (int z = 1; z < depth - 1; z++) {
					for (int w = 1; w < depth - 1; w++) {
						int neighbors = 0;
						bool set = hypercube[x, y, z, w];
		
						for (int nx = x - 1; nx <= x + 1; nx++) {
							for (int ny = y - 1; ny <= y + 1; ny++) {
								for (int nz = z - 1; nz <= z + 1; nz++) {
									for (int nw = w - 1; nw <= w + 1; nw++) {
										if (x == nx && y == ny && z == nz && w == nw) continue;
										neighbors += hypercube[nx, ny, nz, nw] ? 1 : 0;
									}
								}
							}
						}
						
						next[x, y, z, w] = neighbors == 3 || set && neighbors == 2;
					}
				}
			}
		}
		
		hypercube = next;
	}
	
	hypercube.Cast<bool>().Count(true).Dump("Part 2");
}

