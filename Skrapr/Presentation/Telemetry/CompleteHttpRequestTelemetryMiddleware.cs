namespace Skrapr.Presentation.Telemetry;

internal class CompleteHttpRequestTelemetryMiddleware(IActivityAccessor activityAccessor) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var activity = activityAccessor.GetCurrent();

        if (activity is null)
        {
            await next(context);
            return;
        }
        
        if (context.Request.Query.TryGetValue("sessionId", out var sessionId))
        {
            activity.SetTag("sessionId", sessionId.ToString());
        }

        foreach (var routeValue in context.Request.RouteValues)
        {
            if (routeValue.Key != "action" && routeValue.Key != "controller")
            {
                activity.SetTag($"route_key_{routeValue.Key}", routeValue.Value?.ToString());
            }
        }

        await using var initializedContext = await InitializedHttpContext.Initialize(context);

        if (context.Request.Method != HttpMethod.Get.ToString())
        {
            var requestBody = await initializedContext.ReadRequestBody();
            if (requestBody is not null)
            {
                activity.AddTagIfMissing("RequestBody", requestBody.Truncate(1000));
            }
        }

        await next(context);
    }
}
