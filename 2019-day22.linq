<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

Shuffle ParseShuffle(string str) =>
	str.StartsWith("deal with increment ") ? new Shuffle(long.Parse(str[20..]), 0) 
	: str.StartsWith("cut ") ? new Shuffle(1, long.Parse(str[4..]))
	: str == "deal into new stack" ? new Shuffle(-1, 1)
	: throw new ArgumentOutOfRangeException(nameof(str));

Deck Apply(Deck deck, Shuffle shuffle) => new Deck(deck.Size, 
		deck.ZeroIndex * shuffle.Stride - shuffle.Offset, 
		deck.Stride * shuffle.Stride
	).Normalize();
	
Shuffle Compose(Shuffle s1, Shuffle s2) => new Shuffle(
		s1.Stride * s2.Stride,
		s1.Stride * s2.Offset + s2.Offset
	);

void Main() {
	var shuffles = ReadLines().Select(ParseShuffle).ToArray();
	
	var deck = new Deck(10007, 0, 1);
	foreach (var shuf in shuffles) deck = Apply(deck, shuf);
	deck.FindCard(2019).Dump("Part 1");
	
	deck = new Deck(119315717514047, 0, 1);
}

record Shuffle(long Stride, long Offset) {
	public Shuffle Normalize(long size) => new Shuffle(
		(Stride % size + size) % size,
		(Offset % size + size) % size
	);
}

record Deck(long Size, long ZeroIndex, long Stride) {
	public Deck Normalize() => this with {
		ZeroIndex = (ZeroIndex % Size + Size) % Size,
		Stride = (Stride % Size + Size) % Size,
	};
	
	public long FindCard(long card) => (card * Stride + ZeroIndex) % Size;
}
