```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.22631.6783/23H2/2023Update/SunValley3)
13th Gen Intel Core i7-13800H 2.50GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3


```
| Method                 | Mean     | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------- |---------:|------:|-------:|----------:|------------:|
| MediatR_SendQuery      | 55.98 ns |  1.00 | 0.0191 |     240 B |        1.00 |
| Synaptrix_SendQuery    | 49.31 ns |  0.88 | 0.0032 |      40 B |        0.17 |
| SynaptrixGen_SendQuery | 25.77 ns |  0.46 | 0.0032 |      40 B |        0.17 |
| Martin_SendQuery       | 16.36 ns |  0.29 | 0.0032 |      40 B |        0.17 |
