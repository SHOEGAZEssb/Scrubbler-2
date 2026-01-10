using Scrubbler.Abstractions.Services;

namespace Scrubbler.Host.Services;

internal sealed class WindowHandleProvider() : IWindowHandleProvider
{
    private Window? _window;

    public void SetWindow(Window window)
    {
        _window = window;
    }

    public IntPtr GetWindowHandle()
    {
        if (_window == null)
            throw new InvalidOperationException("Window not set");

        return WinRT.Interop.WindowNative.GetWindowHandle(_window);
    }
}
