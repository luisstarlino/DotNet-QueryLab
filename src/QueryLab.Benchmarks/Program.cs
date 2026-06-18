using BenchmarkDotNet.Running;
using QueryLab.Benchmarks.Benchmarks;

BenchmarkRunner.Run(
[
    BenchmarkConverter.TypeToBenchmarks(typeof(FiltragemBenchmark)),
    BenchmarkConverter.TypeToBenchmarks(typeof(PaginacaoBenchmark)),
    BenchmarkConverter.TypeToBenchmarks(typeof(ProjecaoBenchmark)),
]);
