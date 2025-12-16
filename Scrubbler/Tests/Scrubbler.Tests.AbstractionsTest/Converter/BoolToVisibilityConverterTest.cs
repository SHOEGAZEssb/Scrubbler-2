using Microsoft.UI.Xaml;
using Scrubbler.Abstractions.Converter;

namespace Scrubbler.Tests.AbstractionsTest.Converter;

[TestFixture]
internal class BoolToVisibilityConverterTest
{
    [Test]
    public void ConvertTest()
    {
        var conv = new BoolToVisibilityConverter();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(conv.Convert(true, typeof(Visibility), new object(), string.Empty), Is.EqualTo(Visibility.Visible));
            Assert.That(conv.Convert(false, typeof(Visibility), new object(), string.Empty), Is.EqualTo(Visibility.Collapsed));
        }
    }

    [Test]
    public void ConvertInvertedTest()
    {
        var conv = new BoolToVisibilityConverter()
        {
            Invert = true
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(conv.Convert(true, typeof(Visibility), new object(), string.Empty), Is.EqualTo(Visibility.Collapsed));
            Assert.That(conv.Convert(false, typeof(Visibility), new object(), string.Empty), Is.EqualTo(Visibility.Visible));
        }
    }

    [Test]
    public void ConvertBackTest()
    {
        var conv = new BoolToVisibilityConverter();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(conv.ConvertBack(Visibility.Visible, typeof(bool), new object(), string.Empty), Is.True);
            Assert.That(conv.ConvertBack(Visibility.Collapsed, typeof(bool), new object(), string.Empty), Is.False);
        }
    }

    [Test]
    public void ConvertBackInvertedTest()
    {
        var conv = new BoolToVisibilityConverter()
        {
            Invert = true
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(conv.ConvertBack(Visibility.Visible, typeof(bool), new object(), string.Empty), Is.False);
            Assert.That(conv.ConvertBack(Visibility.Collapsed, typeof(bool), new object(), string.Empty), Is.True);
        }
    }
}
