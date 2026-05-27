// (c) DeNA Co., Ltd.

namespace Waffle.Core.Test;

public class WaffleSyntaxTest_AlignmentFormat
{
    [Test]
    public void AlignmentAndFormat_NoSpecification_ValueIsUnchanged()
    {
        var v = "hoge";
        Assert.That(Render($"{v}"), Is.EqualTo("hoge"));
    }

    [Test]
    public void AlignmentAndFormat_PositiveAlignment_RightAligned()
    {
        var v = "right";
        Assert.That(Render($"{v,7}"), Is.EqualTo($"{v,7}"));
    }

    [Test]
    public void AlignmentAndFormat_NegativeAlignment_LeftAligned()
    {
        var v = "left";
        Assert.That(Render($"{v,-7}"), Is.EqualTo($"{v,-7}"));
    }

    [Test]
    public void AlignmentAndFormat_WithFormat_FormatIsApplied()
    {
        var v = 1234;
        Assert.That(Render($"{v:D5}"), Is.EqualTo($"{v:D5}"));
    }

    [Test]
    public void AlignmentAndFormat_WithCustomFormat_LtGt_BothAreApplied()
    {
        var v = 1234;
        Assert.That(
            Render($" \u00a0  {v,7:D5<>}   \u00a0  {v,10:<D8>}  \u00a0  {v,8:<>D6} \u00a0  "),
            Is.EqualTo($"{v,7:D5}{v,10:D8}{v,8:D6}"));
    }

    [Test]
    public void AlignmentAndFormat_WithCustomFormat_LtLtGtGt_BothAreApplied()
    {
        var v = 1234;
        Assert.That(Render($"""
            
                 
            
            {v,7:D5<<>>}
            
                  
            {v,10:<<D8>>}

                 
            {v,8:<<>>D6}
            
                 
                 
            """), Is.EqualTo($"{v,7:D5}{v,10:D8}{v,8:D6}"));
    }
}
