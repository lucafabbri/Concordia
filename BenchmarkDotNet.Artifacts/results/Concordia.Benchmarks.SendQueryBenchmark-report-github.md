```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.22631.6783/23H2/2023Update/SunValley3)
13th Gen Intel Core i7-13800H 2.50GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.200
  [Host]   : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  ShortRun : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                 | Mean     | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------- |---------:|------:|-------:|----------:|------------:|
| MediatR_SendQuery      | 66.84 ns |  1.00 | 0.0191 |     240 B |        1.00 |
| Concordia_SendQuery    | 64.12 ns |  0.96 | 0.0088 |     112 B |        0.47 |
| ConcordiaGen_SendQuery | 32.76 ns |  0.49 | 0.0089 |     112 B |        0.47 |
| Martin_SendQuery       | 27.71 ns |  0.42 | 0.0032 |      40 B |        0.17 |
