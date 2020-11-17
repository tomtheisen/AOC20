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
    var allShuf = shuffles.Aggregate((a, b) => a.AndThen(b, deck.Size));
    var part1Shuffled = deck.Shuffle(allShuf);
    
    long p1idx = part1Shuffled.FindCard(2019).Dump("Part 1");
    part1Shuffled.CardAt(p1idx).Dump("2019?");

    
    var bigdeck = new Deck(119315717514047, 0, 1);
    Shuffle duplicated = shuffles.Aggregate((a, b) => a.AndThen(b, bigdeck.Size));
    long targetTimes = 101741582076661;
    
    for (long count = 1; count < targetTimes; targetTimes *= 2) {
        if ((targetTimes & targetTimes) > 0) bigdeck = bigdeck.Shuffle(duplicated);
        duplicated = duplicated.AndThen(duplicated, bigdeck.Size);
    }
    
    bigdeck.Dump()
        .CardAt(2020).Dump();
    
    
    
}

record Shuffle(long Stride, long Offset) {
	public Shuffle Normalize(long size) => new Shuffle(
		(Stride % size + size) % size,
		(Offset % size + size) % size
	);
    
    public Shuffle AndThen(Shuffle other, long size) => new Shuffle(
		this.Stride * other.Stride,
		this.Offset * other.Stride + other.Offset
	).Normalize(size);
    
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
	
	public long FindCard(long card) => (card * Stride + ZeroIndex) % Size;
    
    public long CardAt(long position) {
        checked {
            long up = Stride, uptimes = 1, down = Stride - Size, downtimes = 1;
            
            while (up != 1) {
                long newstep = up + down;
                if (newstep > 0) (up, uptimes) = (newstep, uptimes + downtimes);
                else (down, downtimes) = (newstep, uptimes + downtimes);
            }
            
            //new { up, down, uptimes, downtimes }.Dump();
            
            var result = (new BigInteger(position - ZeroIndex + Size) % Size * uptimes + ZeroIndex) % Size;
            return (long)result;
        }
    }
    
    public Deck Shuffle(Shuffle shuffle) => new Deck(this.Size, 
		this.ZeroIndex * shuffle.Stride - shuffle.Offset, 
		this.Stride * shuffle.Stride
	).Normalize();
}
