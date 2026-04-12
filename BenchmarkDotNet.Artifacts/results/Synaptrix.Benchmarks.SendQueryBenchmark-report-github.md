```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.22631.6783/23H2/2023Update/SunValley3)
13th Gen Intel Core i7-13800H 2.50GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3


```
| Method                 | Mean     | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------- |---------:|------:|-------:|----------:|------------:|
| Synaptrix_SendQuery    | 50.74 ns |  1.00 | 0.0032 |      40 B |        1.00 |
| SynaptrixGen_SendQuery | 26.77 ns |  0.53 | 0.0032 |      40 B |        1.00 |
| Martin_SendQuery       | 16.88 ns |  0.33 | 0.0032 |      40 B |        1.00 |
