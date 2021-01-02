<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\AOC2020"

var input = Regex.Split(ReadString(), @"\r?\n\r?\n")
	.Select(r => Regex.Split(r, @"\r?\n"))
	.ToList();

var validations = input[0]
	.Select(r => Regex.Matches(r, @"\d+").Select(m => int.Parse(m.Value)).ToArray())
	.ToList();

bool Satisfies(int[] validation, int num) =>
	num >= validation[0] && num <= validation[1] 
	|| num >= validation[2] && num <= validation[3];
	
bool IsValid(int num) => validations.Any(v => Satisfies(v, num));

int part1 = 0;
// indexed by ticket number index
var possibleAssignments = new int[input[0].Length];
Array.Fill(possibleAssignments, (1 << possibleAssignments.Length) - 1);

foreach (var ticket in input[2][1..]) {
	if (string.IsNullOrWhiteSpace(ticket)) continue;
	bool validTicket = true;
	int[] ticketnums = ticket.Split(',').Select(s => int.Parse(s)).ToArray();
	foreach (var num in ticketnums) {
		if (!IsValid(num)) {
			part1 += num;
			validTicket = false;
		}
	}
	if (validTicket) {
		for (int i = 0; i < ticketnums.Length; i++) {
			for (int j = 0; j < validations.Count; j++) {
				if (!Satisfies(validations[j], ticketnums[i])) {
					possibleAssignments[i] &= ~(1 << j);
				}
			}
		}
	}
}
part1.DumpClip("Part 1");

// indexed by ticket number index
var propIndices = new int[input[0].Length];
var orderedTuples = possibleAssignments
	.Select((a, i) => new { PossibleProperties = a, TicketNumberIndex = i })
	.OrderBy(t => BitOperations.PopCount((uint)t.PossibleProperties))
	.ToList();
	
int assigned = 0;
foreach (var record in orderedTuples) propIndices[record.TicketNumberIndex] 
	= (int)Log2(assigned ^ (assigned = record.PossibleProperties));

var myTicket = input[1][1]
	.Split(',')
	.Select(a => int.Parse(a))
	.ToArray();

long part2 = 1;
for (int i = 0; i < myTicket.Length; i++) checked {
	string prop = input[0][propIndices[i]];
	if (prop.StartsWith("departure")) part2 *= myTicket[i];
}

part2.DumpClip("Part 2");
