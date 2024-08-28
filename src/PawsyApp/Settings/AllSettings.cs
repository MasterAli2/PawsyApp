using PawsyApp.GuildStorage;
using System.Collections.Concurrent;
using System.Text.Json;

namespace PawsyApp.Settings;

public partial class AllSettings
{
    internal static ConcurrentDictionary<ulong, GuildSettings> GuildSettingsStorage = [];
    internal static readonly JsonSerializerOptions options = new() { WriteIndented = true };
    internal static void SaveAll()
    {
        foreach (var item in GuildSettingsStorage.Keys)
        {
            GuildSettingsStorage[item].Save();
        }

    }
}