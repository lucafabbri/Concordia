```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.22631.6783/23H2/2023Update/SunValley3)
13th Gen Intel Core i7-13800H 2.50GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3


```
| Method                   | Mean      | Ratio | Allocated | Alloc Ratio |
|------------------------- |----------:|------:|----------:|------------:|
| Synaptrix_SendCommand    | 44.324 ns |  1.00 |         - |          NA |
| SynaptrixGen_SendCommand |  8.229 ns |  0.19 |         - |          NA |
| Martin_SendCommand       |  4.174 ns |  0.09 |         - |          NA |
