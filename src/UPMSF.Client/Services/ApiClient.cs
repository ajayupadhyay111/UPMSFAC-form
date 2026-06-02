using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UPMSF.Shared;

namespace UPMSF.Client.Services;

public record ApiResult(bool Ok, string? Error = null)
{
    public static ApiResult Success() => new(true);
    public static ApiResult Fail(string e) => new(false, e);
}

public record ApiResult<T>(bool Ok, T? Value = default, string? Error = null)
{
    public static ApiResult<T> Success(T v) => new(true, v);
    public static ApiResult<T> Fail(string e) => new(false, default, e);
}

public class ApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public ApiClient(HttpClient http) => _http = http;

    // ---------- lookups ----------
    public async Task<List<LookupItem>> GetDistrictsAsync() =>
        await _http.GetFromJsonAsync<List<LookupItem>>("api/lookups/districts") ?? new();
    public async Task<List<LookupItem>> GetCouncilsAsync() =>
        await _http.GetFromJsonAsync<List<LookupItem>>("api/lookups/councils") ?? new();
    public async Task<List<CourseDto>> GetCoursesAsync(int councilId, CourseType type) =>
        await _http.GetFromJsonAsync<List<CourseDto>>($"api/lookups/courses?councilId={councilId}&courseType={(int)type}") ?? new();

    // ---------- auth ----------
    public Task<ApiResult<RegisterResponse>> RegisterAsync(RegisterRequest req) =>
        PostAsync<RegisterRequest, RegisterResponse>("api/auth/register", req);
    public Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest req) =>
        PostAsync<LoginRequest, LoginResponse>("api/auth/login", req);
    public async Task<ProfileDto?> GetProfileAsync() =>
        await _http.GetFromJsonAsync<ProfileDto>("api/auth/me");

    // ---------- applications ----------
    public async Task<List<ApplicationSummaryDto>> GetApplicationsAsync() =>
        await _http.GetFromJsonAsync<List<ApplicationSummaryDto>>("api/applications") ?? new();
    public async Task<ApplicationDetailDto?> GetApplicationAsync(int id) =>
        await _http.GetFromJsonAsync<ApplicationDetailDto>($"api/applications/{id}");
    public Task<ApiResult<CreateApplicationResponse>> CreateApplicationAsync(CourseDetailsDto dto) =>
        PostAsync<CourseDetailsDto, CreateApplicationResponse>("api/applications", dto);
    public Task<ApiResult> SavePart1Async(int id, FormPart1Dto dto) =>
        PutAsync($"api/applications/{id}/part1", dto);
    public Task<ApiResult> SavePart2Async(int id, FormPart2Dto dto) =>
        PutAsync($"api/applications/{id}/part2", dto);
    public Task<ApiResult> ProceedDocumentsAsync(int id) =>
        PostNoBodyAsync($"api/applications/{id}/documents/proceed");
    public Task<ApiResult<PaymentDto>> PayAsync(int id) =>
        PostNoBodyAsync<PaymentDto>($"api/applications/{id}/payment");
    public Task<ApiResult> SubmitAsync(int id) =>
        PostNoBodyAsync($"api/applications/{id}/submit");

    /// <summary>Downloads a document's bytes (with auth) so the client can open/view it.</summary>
    public async Task<(byte[] Data, string ContentType)?> DownloadDocumentAsync(int id, DocumentType type)
    {
        var resp = await _http.GetAsync($"api/applications/{id}/documents/{type}/content");
        if (!resp.IsSuccessStatusCode) return null;
        var data = await resp.Content.ReadAsByteArrayAsync();
        var ct = resp.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        return (data, ct);
    }

    public async Task<ApiResult<DocumentDto>> UploadDocumentAsync(int id, DocumentType type, Stream content, string fileName, long size)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(content);
        form.Add(fileContent, "file", fileName);
        var resp = await _http.PostAsync($"api/applications/{id}/documents/{type}", form);
        return await ReadAsync<DocumentDto>(resp);
    }

    // ---------- helpers ----------
    private async Task<ApiResult<TOut>> PostAsync<TIn, TOut>(string url, TIn body)
    {
        var resp = await _http.PostAsJsonAsync(url, body);
        return await ReadAsync<TOut>(resp);
    }
    private async Task<ApiResult> PutAsync<TIn>(string url, TIn body)
    {
        var resp = await _http.PutAsJsonAsync(url, body);
        return await ReadAsync(resp);
    }
    private async Task<ApiResult> PostNoBodyAsync(string url)
    {
        var resp = await _http.PostAsync(url, null);
        return await ReadAsync(resp);
    }
    private async Task<ApiResult<TOut>> PostNoBodyAsync<TOut>(string url)
    {
        var resp = await _http.PostAsync(url, null);
        return await ReadAsync<TOut>(resp);
    }

    private static async Task<ApiResult<T>> ReadAsync<T>(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode)
        {
            var v = await resp.Content.ReadFromJsonAsync<T>(Json);
            return ApiResult<T>.Success(v!);
        }
        return ApiResult<T>.Fail(await ExtractError(resp));
    }
    private static async Task<ApiResult> ReadAsync(HttpResponseMessage resp) =>
        resp.IsSuccessStatusCode ? ApiResult.Success() : ApiResult.Fail(await ExtractError(resp));

    private static async Task<string> ExtractError(HttpResponseMessage resp)
    {
        try
        {
            var doc = await resp.Content.ReadFromJsonAsync<JsonElement>();
            if (doc.ValueKind == JsonValueKind.Object && doc.TryGetProperty("message", out var m))
                return m.GetString() ?? resp.ReasonPhrase ?? "Error";
        }
        catch { /* not json */ }
        return resp.StatusCode == HttpStatusCode.Unauthorized
            ? "Session expired. Please login again."
            : resp.ReasonPhrase ?? "Request failed.";
    }
}
