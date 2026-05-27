.PHONY: build
build:
	dotnet build -c Debug
	dotnet build -c Release

.PHONY: test
test:
	dotnet test -c Debug

.PHONY: bench
bench:
	dotnet run --project benchmarks/Waffle.Core.Benchmark -c Release -- -f '*WaffleSyntaxBenchmark*'

.PHONY: t4
t4:
	t4 --class=Waffle.Core.Benchmark.T4Preprocessed --out=benchmarks/Waffle.Core.Benchmark/T4.cs -- benchmarks/Waffle.Core.Benchmark/T4.tt
