using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace UPMSF.Client.Services;

/// <summary>Attaches the stored JWT as a Bearer token to every API request.</summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ILocalStorageService _storage;
    public AuthHeaderHandler(ILocalStorageService storage) => _storage = storage;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _storage.GetItemAsStringAsync(AppAuthStateProvider.TokenKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
