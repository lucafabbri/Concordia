```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.22631.6783/23H2/2023Update/SunValley3)
13th Gen Intel Core i7-13800H 2.50GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3


```
| Method                   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------- |----------:|------:|-------:|----------:|------------:|
| MediatR_SendCommand      | 40.456 ns |  1.00 | 0.0102 |     128 B |        1.00 |
| Synaptrix_SendCommand    | 20.729 ns |  0.51 |      - |         - |        0.00 |
| SynaptrixGen_SendCommand |  4.201 ns |  0.10 |      - |         - |        0.00 |
| Martin_SendCommand       |  4.508 ns |  0.11 |      - |         - |        0.00 |
