using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using PawsyApp.PawsyCore.Modules.Settings;

namespace FilterMatcher.Settings;

internal class FilterMatcherSettings() : ISettings
{
    [JsonInclude]
    internal ConcurrentDictionary<long, RuleBundle> RuleList { get; set; } = [];
    [JsonInclude]
    internal ulong LoggingChannelID { get; set; } = 0;
    [JsonInclude]
    internal ulong NextRuleID { get; set; } = 0;
}
