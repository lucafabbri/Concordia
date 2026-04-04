```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.22631.6783/23H2/2023Update/SunValley3)
13th Gen Intel Core i7-13800H 2.50GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.200
  [Host]   : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  ShortRun : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                   | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------- |----------:|------:|-------:|----------:|------------:|
| MediatR_SendCommand      | 50.543 ns |  1.00 | 0.0102 |     128 B |        1.00 |
| Concordia_SendCommand    | 22.222 ns |  0.44 |      - |         - |        0.00 |
| ConcordiaGen_SendCommand |  1.733 ns |  0.03 |      - |         - |        0.00 |
| Martin_SendCommand       |  5.771 ns |  0.11 |      - |         - |        0.00 |
