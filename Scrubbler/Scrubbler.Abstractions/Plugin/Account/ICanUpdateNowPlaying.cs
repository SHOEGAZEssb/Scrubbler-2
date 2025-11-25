using System;
using System.Collections.Generic;
using System.Text;

namespace Scrubbler.Abstractions.Plugin.Account;

public interface ICanUpdateNowPlaying : IAccountFunction
{
    Task<string?> UpdateNowPlaying(string artistName, string trackName, string? albumName);
}
