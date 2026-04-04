```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.22631.6783/23H2/2023Update/SunValley3)
13th Gen Intel Core i7-13800H 2.50GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.200
  [Host]   : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  ShortRun : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                           | Mean      | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------------- |----------:|------:|-------:|----------:|------------:|
| MediatR_PublishNotification      | 87.151 ns |  1.00 | 0.0350 |     440 B |        1.00 |
| Concordia_PublishNotification    | 74.028 ns |  0.85 | 0.0178 |     224 B |        0.51 |
| ConcordiaGen_PublishNotification |  1.624 ns |  0.02 |      - |         - |        0.00 |
| Martin_PublishNotification       |  7.902 ns |  0.09 |      - |         - |        0.00 |
