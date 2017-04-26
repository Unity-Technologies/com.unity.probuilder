using System;
using System.Collections;

static class Test
{
	static int Main(string[] args)
	{
		foreach(string str in args)
			Console.WriteLine("arg: " + str);
		return 0;
	}
}
