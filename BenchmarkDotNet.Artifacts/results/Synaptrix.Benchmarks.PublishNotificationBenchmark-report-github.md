```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.22631.6783/23H2/2023Update/SunValley3)
13th Gen Intel Core i7-13800H 2.50GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3


```
| Method                           | Mean       | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |-----------:|------:|-------:|----------:|------------:|
| MediatR_PublishNotification      | 68.1210 ns | 1.002 | 0.0350 |     440 B |        1.00 |
| Synaptrix_PublishNotification    | 65.3865 ns | 0.962 | 0.0178 |     224 B |        0.51 |
| SynaptrixGen_PublishNotification |  0.4611 ns | 0.007 |      - |         - |        0.00 |
| Martin_PublishNotification       |  5.9429 ns | 0.087 |      - |         - |        0.00 |
