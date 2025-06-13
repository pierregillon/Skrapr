namespace Skrapr;

public record WebSiteUrl
{
    public string Value { get; }

    public WebSiteUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Url cannot be null or whitespace.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
           )
        {
            throw new ArgumentException("The url must be an absolute http or https url.", nameof(url));
        }

        Value = url;
    }
    
    public override string ToString() => Value;
}