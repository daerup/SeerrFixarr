namespace SeerrFixarr.Api.Overseerr;

public record UserLocalSettings
{
  public string Username { get; set; }  = null!;
  public string Locale { get; set; } = "";

  public void Deconstruct(out string username, out string locale)
  {
    username = Username;
    locale = Locale;
  }
}