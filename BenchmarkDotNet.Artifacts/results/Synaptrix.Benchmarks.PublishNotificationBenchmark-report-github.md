```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.22631.6783/23H2/2023Update/SunValley3)
13th Gen Intel Core i7-13800H 2.50GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3


```
| Method                           | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |-----------:|------:|-------:|----------:|------------:|
| Synaptrix_PublishNotification    | 65.4674 ns | 1.001 | 0.0178 |     224 B |        1.00 |
| SynaptrixGen_PublishNotification |  0.3902 ns | 0.006 |      - |         - |        0.00 |
| Martin_PublishNotification       |  6.2519 ns | 0.096 |      - |         - |        0.00 |
