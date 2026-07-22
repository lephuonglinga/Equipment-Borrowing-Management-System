using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Options;
using Microsoft.Extensions.Options;

namespace EquipmentBorrowingManagementSystem.Web.Services;

public class EbmsApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptions<ApiOptions> _apiOptions;

    public EbmsApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IOptions<ApiOptions> apiOptions)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _apiOptions = apiOptions;
        _httpClient.BaseAddress = new Uri(_apiOptions.Value.BaseUrl.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<T?> GetAsync<T>(string path, bool requireAuth = true, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, path, requireAuth);
        return await SendAsync<T>(request, requireAuth, cancellationToken);
    }

    public async Task<T?> PostAsync<T>(string path, object? body = null, bool requireAuth = true, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, path, requireAuth, body);
        return await SendAsync<T>(request, requireAuth, cancellationToken);
    }

    public async Task PostAsync(string path, object? body = null, bool requireAuth = true, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, path, requireAuth, body);
        await SendAsync<object?>(request, requireAuth, cancellationToken, allowEmpty: true);
    }

    public async Task<T?> PutAsync<T>(string path, object body, bool requireAuth = true, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Put, path, requireAuth, body);
        return await SendAsync<T>(request, requireAuth, cancellationToken);
    }

    public async Task PutAsync(string path, object body, bool requireAuth = true, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Put, path, requireAuth, body);
        await SendAsync<object?>(request, requireAuth, cancellationToken, allowEmpty: true);
    }

    public async Task<T?> PatchAsync<T>(string path, object body, bool requireAuth = true, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Patch, path, requireAuth, body);
        return await SendAsync<T>(request, requireAuth, cancellationToken);
    }

    public async Task PatchAsync(string path, object body, bool requireAuth = true, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Patch, path, requireAuth, body);
        await SendAsync<object?>(request, requireAuth, cancellationToken, allowEmpty: true);
    }

    public async Task DeleteAsync(string path, bool requireAuth = true, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, path, requireAuth);
        await SendAsync<object?>(request, requireAuth, cancellationToken, allowEmpty: true);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, bool requireAuth, object? body = null)
    {
        var request = new HttpRequestMessage(method, path.TrimStart('/'));

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        AttachAuthHeader(request, requireAuth);
        return request;
    }

    private async Task<T?> SendAsync<T>(
        HttpRequestMessage request,
        bool requireAuth,
        CancellationToken cancellationToken,
        bool allowEmpty = false,
        bool retried = false)
    {
        var response = await SendWithRefreshAsync(request, requireAuth, retried, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            if (allowEmpty || response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return default;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }

        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new ApiException((int)response.StatusCode, TryParseMessage(errorBody));
    }

    private async Task<HttpResponseMessage> SendWithRefreshAsync(
        HttpRequestMessage request,
        bool requireAuth,
        bool retried,
        CancellationToken cancellationToken)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        var auth = session?.GetAuthSession();

        if (requireAuth && auth is not null && auth.IsAccessTokenExpired && !string.IsNullOrWhiteSpace(auth.RefreshToken) && !retried)
        {
            await RefreshTokenAsync(auth, session!, cancellationToken);
            AttachAuthHeader(request, requireAuth);
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (requireAuth && response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !retried)
        {
            auth = session?.GetAuthSession();
            if (auth is not null && !string.IsNullOrWhiteSpace(auth.RefreshToken))
            {
                await RefreshTokenAsync(auth, session!, cancellationToken);

                using var retryRequest = await CloneRequestAsync(request);
                AttachAuthHeader(retryRequest, requireAuth);
                response = await _httpClient.SendAsync(retryRequest, cancellationToken);
            }
        }

        return response;
    }

    private async Task RefreshTokenAsync(AuthSession auth, ISession session, CancellationToken cancellationToken)
    {
        using var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new RefreshTokenRequest { RefreshToken = auth.RefreshToken }, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };

        var response = await _httpClient.SendAsync(refreshRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            session.ClearAuthSession();
            throw new ApiException(401, "Phiên đăng nhập đã hết hạn.");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var refreshed = JsonSerializer.Deserialize<AuthSession>(content, JsonOptions)
            ?? throw new ApiException(401, "Phiên đăng nhập đã hết hạn.");

        session.SetAuthSession(refreshed);
    }

    private void AttachAuthHeader(HttpRequestMessage request, bool requireAuth)
    {
        request.Headers.Authorization = null;
        if (!requireAuth)
        {
            return;
        }

        var auth = _httpContextAccessor.HttpContext?.Session.GetAuthSession();
        if (auth is not null && !string.IsNullOrWhiteSpace(auth.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        if (request.Content is not null)
        {
            var body = await request.Content.ReadAsStringAsync();
            clone.Content = new StringContent(body, Encoding.UTF8, request.Content.Headers.ContentType?.MediaType ?? "application/json");
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }

    private static string? TryParseMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            var error = JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptions);
            return error?.Message;
        }
        catch
        {
            return null;
        }
    }
}
