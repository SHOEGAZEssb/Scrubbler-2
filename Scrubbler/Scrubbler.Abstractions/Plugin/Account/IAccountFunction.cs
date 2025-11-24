using System;
using System.Collections.Generic;
using System.Text;

namespace Scrubbler.Abstractions.Plugin.Account;

public interface IAccountFunction
{
    string? Username { get; }
}
