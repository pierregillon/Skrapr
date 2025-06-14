using System.Diagnostics;

namespace Skrapr.Infra.Telemetry;

public class Instrumentation : IDisposable
{
    internal const string ActivitySourceName = "Skrapr";

    public Instrumentation()
    {
        var version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
        ActivitySource = new ActivitySource(ActivitySourceName, version);
    }

    public ActivitySource ActivitySource { get; }

    public void Dispose() =>
        ActivitySource.Dispose();
}
