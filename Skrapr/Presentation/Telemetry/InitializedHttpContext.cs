namespace Skrapr.Presentation.Telemetry;

public record InitializedHttpContext(
    HttpContext Context,
    Stream OriginalRequestStream,
    Stream OverridenRequestStream
) : IAsyncDisposable
{
    public static async Task<InitializedHttpContext> Initialize(HttpContext context)
    {
        var originalRequestStream = context.Request.Body;

        var overridenRequestBodyStream = new MemoryStream();
        await context.Request.Body.CopyToAsync(overridenRequestBodyStream);
        context.Request.Body = overridenRequestBodyStream;

        return new InitializedHttpContext(
            context,
            originalRequestStream,
            overridenRequestBodyStream
        );
    }

    public async Task<string?> ReadRequestBody()
    {
        OverridenRequestStream.Seek(0, SeekOrigin.Begin);
        var requestBody = await new StreamReader(OverridenRequestStream).ReadToEndAsync();
        OverridenRequestStream.Seek(0, SeekOrigin.Begin);
        return requestBody;
    }

    public async Task RestoreStreams()
    {
        await OverridenRequestStream.DisposeAsync();
        Context.Request.Body = OriginalRequestStream;
    }

    public async ValueTask DisposeAsync() => await RestoreStreams();
}
