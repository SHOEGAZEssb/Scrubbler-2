using System;
using System.Collections.Generic;
using System.Text;

namespace Scrubbler.Abstractions.Services;

public interface IWindowHandleProvider
{
    IntPtr GetWindowHandle();
}

