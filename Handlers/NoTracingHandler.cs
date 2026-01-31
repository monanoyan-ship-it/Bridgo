namespace Bridgo.Handlers;

/// <summary>
/// Distributed tracing header'larini (traceparent, tracestate) kaldiran handler
/// Bazi harici API'ler bu header'lari kabul etmiyor
/// </summary>
public class NoTracingHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Tracing header'larini kaldir
        request.Headers.Remove("traceparent");
        request.Headers.Remove("tracestate");

        return base.SendAsync(request, cancellationToken);
    }
}
