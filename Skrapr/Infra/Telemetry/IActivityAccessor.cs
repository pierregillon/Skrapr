using System.Diagnostics;

namespace Skrapr.Infra.Telemetry;

public interface IActivityAccessor
{
    Activity? GetCurrent();
}