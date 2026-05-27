// (c) DeNA Co., Ltd.

namespace Waffle.Core.Benchmark.Test;

public class SameOutputTest
{
    [Test]
    public void SameOutput()
    {
        var benchmark = new WaffleSyntaxBenchmark();
        var t4 = benchmark.WithT4();
        var t4Preprocessed = benchmark.WithT4Preprocessed();
        var scriban = benchmark.WithScriban();
        var waffle = benchmark.WithWaffle();
        var sb = benchmark.WithStringBuilder();
        Assert.That(t4, Is.EqualTo(sb));
        Assert.That(t4Preprocessed, Is.EqualTo(sb));
        Assert.That(scriban, Is.EqualTo(sb));
        Assert.That(waffle, Is.EqualTo(sb));
    }
}
