using System.Net;
using System.Text;
using NBomber.WebToursDemo.NewScripts;

namespace NBomber.WebToursDemo.NewScripts.WebTours;

internal static class HttpSupport
{
    private static readonly Random Random = new();

    public static HttpClient GetOrCreateClient(IScenarioContext context)
    {
        const string key = "http_client";
        if (context.ScenarioInstanceData.TryGetValue(key, out var existing) && existing is HttpClient cached)
            return cached;

        var client = NBomberHttp.CreateDefaultClient();
        context.ScenarioInstanceData[key] = client;
        return client;
    }

    public static async Task ThinkTime(IScenarioContext context)
    {
        var delay = Random.Next(VariablesOfCycles.ThinkTimeMinMs, VariablesOfCycles.ThinkTimeMaxMs + 1);
        await Task.Delay(delay);
    }

    public static HttpRequestMessage CreateGet(string path)
    {
        var url = path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? path
            : $"{Protocols.BaseUrl}{path}";

        return NBomberHttp.CreateRequest("GET", url)
            .WithHeader("Accept", string.Join(",", Protocols.HtmlAccept))
            .WithHeader("Upgrade-Insecure-Requests", "1");
    }

    public static HttpRequestMessage CreatePostForm(string path, string formBody)
    {
        var url = path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? path
            : $"{Protocols.BaseUrl}{path}";

        return NBomberHttp.CreateRequest("POST", url)
            .WithHeader("Accept", string.Join(",", Protocols.HtmlAccept))
            .WithHeader("Upgrade-Insecure-Requests", "1")
            .WithHeader("Content-Type", "application/x-www-form-urlencoded")
            .WithBody(new StringContent(formBody, Encoding.UTF8, "application/x-www-form-urlencoded"));
    }

    public static string EncodeForm(IEnumerable<KeyValuePair<string, string>> pairs) =>
        string.Join("&", pairs.Select(p =>
            $"{WebUtility.UrlEncode(p.Key)}={WebUtility.UrlEncode(p.Value)}"));

    public static HttpResponseMessage GetPayload(Response<HttpResponseMessage> response) =>
        response.Payload.IsSome() ? response.Payload.Value : throw new InvalidOperationException("HTTP response payload is empty");

    public static async Task<string> ReadBody(Response<HttpResponseMessage> response) =>
        await GetPayload(response).Content.ReadAsStringAsync();

    public static bool IsOk(Response<HttpResponseMessage> response) =>
        !response.IsError && response.Payload.IsSome() && (int)GetPayload(response).StatusCode is >= 200 and < 400;

    public static Response<object> ToStepResponse(Response<HttpResponseMessage> response, string? failMessage = null)
    {
        if (!IsOk(response))
        {
            var status = response.IsError
                ? response.StatusCode
                : response.Payload.IsSome()
                    ? ((int)GetPayload(response).StatusCode).ToString()
                    : "0";

            var reason = response.Payload.IsSome() ? GetPayload(response).ReasonPhrase : null;
            return Response.Fail(
                statusCode: status,
                message: failMessage ?? response.Message ?? reason ?? "HTTP error");
        }

        var payload = GetPayload(response);
        var bodyLength = payload.Content.Headers.ContentLength ?? 0;
        return Response.Ok(
            statusCode: ((int)payload.StatusCode).ToString(),
            sizeBytes: bodyLength);
    }

    public static async Task<Response<HttpResponseMessage>> SendGet(HttpClient client, string path) =>
        await NBomberHttp.Send(client, CreateGet(path));

    public static async Task<Response<HttpResponseMessage>> SendPostForm(HttpClient client, string path, string body) =>
        await NBomberHttp.Send(client, CreatePostForm(path, body));
}
