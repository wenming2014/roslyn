``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 10.0.16299
Processor=Intel Core i5-7500 CPU 3.40GHz (Kaby Lake), ProcessorCount=4
Frequency=3328121 Hz, Resolution=300.4698 ns, Timer=TSC
.NET Core SDK=2.1.1-preview-007094
  [Host]     : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT


```
 |   Method | Commit |     Mean |     Error |    StdDev |
 |--------- |------- |---------:|----------:|----------:|
 | **TestEmit** |   **HEAD** | **662.5 ms** |  **8.110 ms** |  **7.586 ms** |
 | **TestEmit** |  **HEAD^** | **669.8 ms** | **11.450 ms** | **10.711 ms** |
