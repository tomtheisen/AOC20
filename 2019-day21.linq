<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\IntCode"
#load ".\AOC2020"

var machine = new IntCodeMachine { 
    Output = n => Console.Write(n < 127 ? "" + (char)n : n + "\n")
};

// part 1
machine.TakeInput("OR A J\n");
machine.TakeInput("AND B J\n");
machine.TakeInput("AND C J\n");
machine.TakeInput("NOT J J\n");
machine.TakeInput("AND D J\n");
machine.TakeInput("WALK\n");
machine.Run();

// part 2
machine.Reset();

machine.TakeInput("NOT G J\n");
machine.TakeInput("AND H J\n");

machine.TakeInput("NOT F T\n");
machine.TakeInput("OR T J\n");
machine.TakeInput("OR E J\n");

machine.TakeInput("NOT C T\n");
machine.TakeInput("AND T J\n");
machine.TakeInput("AND B J\n");

machine.TakeInput("NOT B T\n");
machine.TakeInput("OR T J\n");

machine.TakeInput("NOT A T\n");
machine.TakeInput("OR T J\n");

machine.TakeInput("AND D J\n");

machine.TakeInput("RUN\n");
machine.Run();

/*
.	jump
#
	#.
		#..
			#...	stay
			#..#	jump
		#.#
			#.#.	stay
			#.##	jump
	##
		##.
		   	##..	stay
		   	##.#
		   		##.#.
		   		     	##.#..	jump
		   		     	##.#.#	
		   		     	      	##.#.#.
		   		     	      	       	##.#.#.. stay
		   		     	      	       	##.#.#.#
		   		     	      	       		##.#.#.#.	??? 
		   		     	      	       		##.#.#.##	jump
		   		     	      	##.#.##	stay
		   		##.##	jump
		###	stay

NOT A
OR (
	NOT B
	OR 
	B AND NOT C AND (
		E
		OR
		NOT F
		OR (
			NOT G
			AND
			H
		)
	)
)
AND D
*/
