using System.Diagnostics;

namespace Skrapr.Infra.Telemetry;

internal class StaticActivityAccessor : IActivityAccessor
{
    public Activity? GetCurrent() => Activity.Current;
}