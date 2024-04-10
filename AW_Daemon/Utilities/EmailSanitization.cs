namespace AW_Daemon.Utilities;

/// <summary>
/// I am using EF for my api, this is just here for double-checking safety, etc. You should never rely on this for actual
/// sanatization, that would be absolutely insane in 202X.
/// </summary>
public class EmailSanitization
{
    public static string SanitizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return string.Empty;

        email = email.Trim().ToLowerInvariant().Replace("'", "''");

        return email;
    }

}