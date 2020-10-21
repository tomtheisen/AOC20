<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

void Main() {
}

public class IntCodeMachine {
	private int[] InitialMemory;
	public int[] Memory { get; set; }
	
	public Queue<int> Input { get; } = new Queue<int>();
	public Action<int>? Output { get; set; }
	
	private int IP = 0;
	private bool Running = false;
	
	public IntCodeMachine() 
		: this(ReadString().Split(',').Select(int.Parse).ToArray()) {}
		
	public IntCodeMachine(int[] initialMemory) {
		InitialMemory = initialMemory;
		Memory = Array.Empty<int>();
		Reset();
	}
	
	public void Reset() {
		Memory = InitialMemory[..];
		IP = 0;
		Running = false;
		Input.Clear();
	}
	
	private ref int Operand(int n) {
		bool immediate = Memory[IP] / n switch { 1 => 100, 2 => 1000, 3 => 10000 } % 10 > 0;
		ref int result = ref Memory[immediate ? IP + n : Memory[IP + n]];
		return ref result;
	}
	
	public void Run() {
		IP = 0;
		Running = true;
		while (Running) { 
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
					Operand(1) = GetInput();
					IP += 2;
					break;
				case 4:
					DoOutput(Operand(1));
					IP += 2;
					break;
				case 5:
					if (Operand(1) != 0) IP = Operand(2);
					else IP += 3;
					break;
				case 6:
					if (Operand(1) == 0) IP = Operand(2);
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
				case 99:
					Running = false;
					break;
			}
		}
	}
	
	private int GetInput() => Input.Dequeue();
	
	private void DoOutput(int n) => (Output ?? WriteLine)(n);
}
