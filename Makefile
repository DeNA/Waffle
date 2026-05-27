.PHONY: build
build:
ifdef WSL_DISTRO_NAME
	dotnet.exe build -c Debug
	dotnet.exe build -c Release
else
	dotnet build -c Debug
	dotnet build -c Release
endif

.PHONY: test
test:
ifdef WSL_DISTRO_NAME
	dotnet.exe test -c Debug
else
	dotnet test -c Debug
endif

.PHONY: bench
bench:
ifdef WSL_DISTRO_NAME
	dotnet.exe run --project benchmarks/Waffle.Core.Benchmark -c Release -- -f '*WaffleSyntaxBenchmark*'
else
	dotnet run --project benchmarks/Waffle.Core.Benchmark -c Release -- -f '*WaffleSyntaxBenchmark*'
endif

.PHONY: t4
t4:
ifdef WSL_DISTRO_NAME
	t4.exe --class=Waffle.Core.Benchmark.T4Preprocessed --out=benchmarks/Waffle.Core.Benchmark/T4.cs -- benchmarks/Waffle.Core.Benchmark/T4.tt
else
	t4 --class=Waffle.Core.Benchmark.T4Preprocessed --out=benchmarks/Waffle.Core.Benchmark/T4.cs -- benchmarks/Waffle.Core.Benchmark/T4.tt
endif