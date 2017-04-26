using System;
using Microsoft.CSharp;
using System.Collections
;using System.CodeDom.Compiler;

static class BuildManager
{
	static int Main(string[] args)
	{
		foreach(string str in args)
			Console.WriteLine("arg: " + str);

		CSharpCodeProvider provider = new CSharpCodeProvider();

		CompilerParameters parameters = new CompilerParameters();

		parameters.GenerateExecutable = true;
		parameters.OutputAssembly = "Test.exe";
		parameters.GenerateInMemory = false;

		var res = provider.CompileAssemblyFromFile(parameters, "Test.cs");

		foreach(CompilerError ce in res.Errors)
			Console.WriteLine(ce.ToString());

		Console.WriteLine("Path: " + res.PathToAssembly);

		return 0;
	}
}
