using System.Diagnostics;

namespace Skrapr.Presentation.Telemetry;

public interface IActivityAccessor
{
    Activity? GetCurrent();
}