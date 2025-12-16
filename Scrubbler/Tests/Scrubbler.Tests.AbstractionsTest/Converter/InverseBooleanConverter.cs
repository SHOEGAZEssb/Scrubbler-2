using Scrubbler.Abstractions.Converter;

namespace Scrubbler.Tests.AbstractionsTest.Converter;

[TestFixture]
internal class InverseBooleanConverterTest
{
    [Test]
    public void ConvertBooleanTest()
    {
        var conv = new InverseBooleanConverter();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(conv.Convert(true, typeof(bool), new object(), string.Empty), Is.False);
            Assert.That(conv.Convert(false, typeof(bool), new object(), string.Empty), Is.True);
        }
    }

    [Test]
    public void ConvertNonBooleanReturnsOriginalValue()
    {
        var conv = new InverseBooleanConverter();
        var input = "not a bool";

        var result = conv.Convert(input, typeof(object), new object(), string.Empty);

        Assert.That(result, Is.SameAs(input));
    }

    [Test]
    public void ConvertBackBooleanTest()
    {
        var conv = new InverseBooleanConverter();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(conv.ConvertBack(true, typeof(bool), new object(), string.Empty), Is.False);
            Assert.That(conv.ConvertBack(false, typeof(bool), new object(), string.Empty), Is.True);
        }
    }

    [Test]
    public void ConvertBackNonBooleanReturnsOriginalValue()
    {
        var conv = new InverseBooleanConverter();
        var input = 42;

        var result = conv.ConvertBack(input, typeof(object), new object(), string.Empty);

        Assert.That(result, Is.EqualTo(input));
    }
}
