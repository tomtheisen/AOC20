<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

static long MulAddMod(long m1, long m2, long add, long mod) {
	if (m2 < 0) return -MulAddMod(m1, -m2, -add, mod);
	long result = add;
	for (m1 %= mod; m2 > 0; m2 /= 2, m1 = m1 * 2 % mod) checked {
		if (m2 % 2 == 1) result = (result + m1) % mod;
	}
	return (result + mod) % mod;
}

void Main() {
	var shuffles = ReadLines().Select(Shuffle.Parse).ToArray();

	var deck = new Deck(10007, 0, 1);
	var part1Shuffled = deck.Shuffle(shuffles.Aggregate((a, b) => a.AndThen(b, deck.Size)));
	long p1idx = part1Shuffled.FindCard(2019).Dump("Part 1");

	var bigdeck = new Deck(119315717514047, 0, 1);
	Shuffle fullShuffle = shuffles.Aggregate((a, b) => a.AndThen(b, bigdeck.Size));
	long targetTimes = 101741582076661;
	
	bigdeck.ShuffleTimes(fullShuffle, targetTimes).CardAt(2020).Dump("Part 2");
}

record Shuffle(long Stride, long Offset) {
	public Shuffle Normalize(long size) => new Shuffle(
		(Stride % size + size) % size,
		(Offset % size + size) % size);

	public override string ToString() => $"Shuffle: (Stride: {Stride}, Offset: {Offset})";

	public Shuffle AndThen(Shuffle other, long size) => checked(
		new Shuffle(
			MulAddMod(this.Stride, other.Stride, 0, size),
			MulAddMod(this.Offset, other.Stride, other.Offset, size)
		).Normalize(size));
	
	public static Shuffle Parse(string str) => 
		str.StartsWith("deal with increment ") ? new Shuffle(long.Parse(str[20..]), 0) 
		: str.StartsWith("cut ") ? new Shuffle(1, long.Parse(str[4..]))
		: str == "deal into new stack" ? new Shuffle(-1, 1)
		: throw new ArgumentOutOfRangeException(nameof(str));
}

record Deck(long Size, long ZeroIndex, long Stride) {
	public Deck Normalize() => this with {
		ZeroIndex = (ZeroIndex % Size + Size) % Size,
		Stride = (Stride % Size + Size) % Size,
	};

	public override string ToString() => $"Deck: (Size: {Size}, 0-index: {ZeroIndex}, Stride: {Stride})";

	public long FindCard(long card) => MulAddMod(card, Stride, ZeroIndex, Size);
	
	public long CardAt(long position) {
		checked {
			long up = Stride, uptimes = 1, down = Stride - Size, downtimes = 1;
			
			while (up != 1) {
				long newstep = up + down;
				if (newstep > 0) (up, uptimes) = (newstep, uptimes + downtimes);
				else (down, downtimes) = (newstep, uptimes + downtimes);
			}
			
			var distanceFromZero = (position - ZeroIndex + Size) % Size;
			return MulAddMod(distanceFromZero, uptimes, 0, Size);
		}
	}
	
	public Deck Shuffle(Shuffle shuffle) => new Deck(this.Size, 
		MulAddMod(this.ZeroIndex, shuffle.Stride, -shuffle.Offset, this.Size), 
		MulAddMod(this.Stride, shuffle.Stride, 0, this.Size)
	).Normalize();
	
	public Deck ShuffleTimes(Shuffle shuffle, long times) {
		var deck = this;
		Shuffle duplicated = shuffle;
		for (long count = 1; count <= times; count *= 2) checked {
			if ((count & times) > 0) deck = deck.Shuffle(duplicated);
			duplicated = duplicated.AndThen(duplicated, deck.Size);
		}
		return deck;
	}
}
