namespace UPMSF.Server.Services;

/// <summary>Thrown when the authenticated principal is missing/invalid claims.
/// Mapped to HTTP 401 by the global exception handler.</summary>
public class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string message = "Unauthorized.") : base(message) { }
}
