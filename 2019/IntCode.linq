<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

#load ".\AOC2020"

void Main() {
	var m = new IntCodeMachine(new long[] {1102,34915192,34915192,7,4,7,99,0});
	m.Run();
}

public class IntCodeMachine : IObservable<long> {
	private long[] InitialMemory;
	public long[] Memory { get; set; }
	
	public BlockingCollection<long> Input { get; } = new();
	public Queue<long> Output { get; } = new();
	public Action<long>? OutputAction { get; set; }
	public Func<long>? InputOverride { get; set; }
	public Func<long>? InputFallback { get; set; }
	public int StepsExecuted { get; private set; }
	
	private int IP = 0;
	private int RelativeBase = 0;
	private bool Running = false;
	private readonly List<IObserver<long>> Observers = new();
	
	public IntCodeMachine() 
		: this(ReadString().Split(',').Select(long.Parse).ToArray()) {}
		
	public IntCodeMachine(long[] initialMemory) {
		InitialMemory = initialMemory;
		Memory = Array.Empty<long>();
		Reset();
	}
	
	public IntCodeMachine Clone() {
		var result = new IntCodeMachine(InitialMemory) {
			Memory = Memory[..],
			OutputAction = OutputAction,
			InputOverride = InputOverride,
			IP = IP,
			RelativeBase = RelativeBase,
			Running = Running,
		};
		result.Observers.AddRange(Observers);
		return result;
	}
	
	public void Reset() {
		Memory = InitialMemory[..];
		StepsExecuted = RelativeBase = IP = 0;
		Running = false;
		while (Input.TryTake(out _));
	}
	
	private ref long Operand(int n) {
		long mode = Memory[IP] / n switch { 1 => 100, 2 => 1000, 3 => 10000, _ => throw new ArgumentOutOfRangeException(nameof(n)) } % 10;
		int ptr = (int)(mode switch { 
			0 => Memory[IP + n], 
			1 => IP + n, 
			2 => Memory[IP + n] + RelativeBase,
			_ => throw new ArgumentOutOfRangeException(nameof(n)),
		});
		var memory = this.Memory;
		if (ptr >= Memory.Length) Array.Resize(ref memory, ptr + 1);
		this.Memory = memory;
		ref long result = ref Memory[ptr];
		return ref result;
	}
	
	public long? Step(bool allowBlockOnInput = true) {
		StepsExecuted += 1;
		switch (Memory[IP] % 100) {
			case 1:
				Operand(3) = Operand(1) + Operand(2);
				IP += 4;
				break;
			case 2:
				Operand(3) = Operand(1) * Operand(2);
				IP += 4;
				break;
			case 3:
				if (Input.Count == 0 && !allowBlockOnInput) break;
				Operand(1) = GetInput();
				IP += 2;
				break;
			case 4:
				var output = Operand(1);
				DoOutput(output);
				IP += 2;
				return output;
			case 5:
				if (Operand(1) != 0) IP = (int)Operand(2);
				else IP += 3;
				break;
			case 6:
				if (Operand(1) == 0) IP = (int)Operand(2);
				else IP += 3;
				break;
			case 7:
				Operand(3) = Operand(1) < Operand(2) ? 1 : 0;
				IP += 4;
				break;
			case 8:
				Operand(3) = Operand(1) == Operand(2) ? 1 : 0;
				IP += 4;
				break;
			case 9:
				RelativeBase += (int)Operand(1);
				IP += 2;
				break;
			case 99:
				Running = false;
				break;
		}
		return null;
	}
	
	public long? RunToNextOutput(CancellationToken? token = null) {
		Running = true;
		long? output;
		do output = Step();
		while (Running && output == null && token?.IsCancellationRequested != true);
		return output;
	}
	
	public long RunToNextOutputOrThrow(CancellationToken? token = null) => RunToNextOutput(token) ?? throw new Exception("machine terminated unexpectedly");
	
	public void RunToNextBlockedInput(CancellationToken? token = null) {
		Running = true;
		do {
			//Console.WriteLine("Starting: " + IP);
			int ip = IP;
			Step(allowBlockOnInput: false);
			//new {ip, IP}.Dump();
			if (ip == IP) return;
		} while (Running && token?.IsCancellationRequested != true);
	}
	
	public void Run(CancellationToken? token = null) {
		IP = 0;
		Running = true;
		while (Running && token?.IsCancellationRequested != true) Step();
		Observers.ForEach(o => o.OnCompleted());
	}
	
	public void TakeInput(long input) => Input.Add(input);
	public void TakeInput(params long[] input) => input.ToList().ForEach(TakeInput);
	public void TakeInput(string input) => TakeInput(input.Select(i => (long)i).ToArray());
	
	private long GetInput() {
		if (InputOverride is not null) return InputOverride();
		if (InputFallback is null) return Input.Take();
		if (Input.TryTake(out long result)) return result;
		return InputFallback();
	}
	
	private void DoOutput(long n) {
		(OutputAction ?? Output.Enqueue)(n);
		Observers.ForEach(o => o.OnNext(n));
	}

	private class Subscription : IDisposable {
		private IntCodeMachine Machine;
		private IObserver<long> Observer;
		
		public Subscription(IntCodeMachine machine, IObserver<long> observer) {
			this.Machine = machine;
			this.Observer = observer;
		}
		
		public void Dispose() => Machine.Observers.Remove(Observer);
	}

	public IDisposable Subscribe(IObserver<long> observer) {
		Observers.Add(observer);
		return new Subscription(this, observer);
	}
}
