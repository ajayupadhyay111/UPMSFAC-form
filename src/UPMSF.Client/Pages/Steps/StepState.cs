namespace UPMSF.Client.Pages.Steps;

/// <summary>How a wizard section should render: not yet reached, currently editable, or saved+locked.</summary>
public enum StepState
{
    Future = 0,
    Active = 1,
    Locked = 2
}
