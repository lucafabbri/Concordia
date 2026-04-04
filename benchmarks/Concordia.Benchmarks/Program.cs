using BenchmarkDotNet.Running;

// Enable Concordia source generator for this assembly.
[assembly: Concordia.Attributes.DiscoverConcordiaHandlersAttribute]

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
