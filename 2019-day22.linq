<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

void Main() {
	var shuffles = ReadLines().Select(Shuffle.Parse).ToArray();

    var deck = new Deck(10007, 0, 1);
    var part1Shuffled = deck.Shuffle(shuffles.Aggregate((a, b) => a.AndThen(b, deck.Size)));
    
    BigInteger p1idx = part1Shuffled.FindCard(2019).Dump("Part 1");

    var bigdeck = new Deck(119315717514047, 0, 1);
    Shuffle fullShuffle = shuffles.Aggregate((a, b) => a.AndThen(b, bigdeck.Size));
    BigInteger targetTimes = 101741582076661;
    
    bigdeck.ShuffleTimes(fullShuffle, targetTimes).CardAt(2020).Dump();
}

record Shuffle(BigInteger Stride, BigInteger Offset) {
	public Shuffle Normalize(BigInteger size) => new Shuffle(
		(Stride % size + size) % size,
		(Offset % size + size) % size
	);
    
    public Shuffle AndThen(Shuffle other, BigInteger size) => checked(new Shuffle(
		this.Stride * other.Stride,
		this.Offset * other.Stride + other.Offset
	).Normalize(size));
    
    public static Shuffle Parse(string str) => 
    	str.StartsWith("deal with increment ") ? new Shuffle(BigInteger.Parse(str[20..]), 0) 
    	: str.StartsWith("cut ") ? new Shuffle(1, BigInteger.Parse(str[4..]))
    	: str == "deal into new stack" ? new Shuffle(-1, 1)
    	: throw new ArgumentOutOfRangeException(nameof(str));
}

record Deck(BigInteger Size, BigInteger ZeroIndex, BigInteger Stride) {
	public Deck Normalize() => this with {
		ZeroIndex = (ZeroIndex % Size + Size) % Size,
		Stride = (Stride % Size + Size) % Size,
	};
	
	public BigInteger FindCard(BigInteger card) => (BigInteger) checked((card * Stride + ZeroIndex) % Size);
    
    public BigInteger CardAt(BigInteger position) {
        checked {
            BigInteger up = Stride, uptimes = 1, down = Stride - Size, downtimes = 1;
            
            while (up != 1) {
                BigInteger newstep = up + down;
                if (newstep > 0) (up, uptimes) = (newstep, uptimes + downtimes);
                else (down, downtimes) = (newstep, uptimes + downtimes);
            }
            
            BigInteger distanceFromZero = (position - ZeroIndex + Size) % Size;
            var result = distanceFromZero * uptimes % Size;
            return (BigInteger)result;
        }
    }
    
    public Deck Shuffle(Shuffle shuffle) => new Deck(this.Size, 
		this.ZeroIndex * shuffle.Stride - shuffle.Offset, 
		this.Stride * shuffle.Stride
	).Normalize();
    
    public Deck ShuffleTimes(Shuffle shuffle, BigInteger times) {
        var deck = this;
        Shuffle duplicated = shuffle;
        for (BigInteger count = 1; count <= times; count *= 2) {
            if ((count & times) > 0) deck = deck.Shuffle(duplicated);
            duplicated = duplicated.AndThen(duplicated, deck.Size);
        }
        return deck;
    }
}
