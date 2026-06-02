using Microsoft.AspNetCore.Components;
using UPMSF.Shared;

namespace UPMSF.Client.Services;

/// <summary>
/// Decides where a logged-in applicant should land, based on how far they have
/// progressed (stored server-side as Application.CurrentStep / Status):
///   • a draft application in progress  -> resume that application's wizard
///   • no application yet               -> start the first application (course step)
///   • all applications submitted       -> dashboard
/// </summary>
public class FlowService
{
    private readonly ApiClient _api;
    private readonly NavigationManager _nav;
    public FlowService(ApiClient api, NavigationManager nav) { _api = api; _nav = nav; }

    public async Task ResumeAsync()
    {
        var apps = await _api.GetApplicationsAsync();
        var draft = apps.FirstOrDefault(a => a.Status != ApplicationStatus.Submitted);
        if (draft is not null)
            _nav.NavigateTo($"application/{draft.Id}");
        else if (apps.Count == 0)
            _nav.NavigateTo("application/new");
        else
            _nav.NavigateTo("dashboard");
    }
}
