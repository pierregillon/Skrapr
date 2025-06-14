using System.Diagnostics;

namespace Skrapr.Infra.Telemetry;

public static class ActivityExtensions
{
    public static void AddTagIfMissing(this Activity activity, string key, string? value)
    {
        if (activity.Tags.All(x => x.Key != key))
        {
            activity.AddTag(key, value);
        }
    }
}
