using System.Diagnostics;

namespace Skrapr.Presentation.Telemetry;

internal class StaticActivityAccessor : IActivityAccessor
{
    public Activity? GetCurrent() => Activity.Current;
}