using BenchmarkDotNet.Running;

// Enable Synaptrix source generator for this assembly.
[assembly: Synaptrix.Attributes.DiscoverSynaptrixHandlersAttribute]

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
