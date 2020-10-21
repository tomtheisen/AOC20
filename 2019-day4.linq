<Query Kind="Statements">
  <Namespace>static System.Math</Namespace>
</Query>

#load ".\AOC2020"

//264360-746325
{
	int result = 0;
	for (int i = 264360; i < 746325; i++) {
		var s = i.ToString();
		if (s[0] <= s[1] && s[1] <= s[2] && s[2] <= s[3] && s[3] <= s[4] && s[4] <= s[5] 
			&& (s[0] == s[1] || s[1] == s[2] || s[2] == s[3] || s[3] == s[4] || s[4] == s[5]))
				++result;
	}
	
	Console.WriteLine(result);
}

{
	int result = 0;
	for (int i = 264360; i < 746325; i++) {
		var s = i.ToString();
		if (s[0] <= s[1] && s[1] <= s[2] && s[2] <= s[3] && s[3] <= s[4] && s[4] <= s[5] 
			&& (
				s[0] == s[1] && s[1] < s[2] 
				|| s[1] == s[2] && s[0] < s[1] && s[2] < s[3]
				|| s[2] == s[3] && s[1] < s[2] && s[3] < s[4] 
				|| s[3] == s[4] && s[2] < s[3] && s[4] < s[5]
				|| s[4] == s[5] && s[3] < s[4]
			))
				++result;
	}
	
	Console.WriteLine(result);
}

