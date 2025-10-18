namespace FinPlan.Web.Config;

public sealed class AdSettings
{
    /// <summary>
    /// Google AdSense publisher client identifier (e.g. ca-pub-XXXXXXXXXXXXXX).
    /// When blank, ads are suppressed globally.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Toggle to force AdSense requests into test mode (data-adtest="on").
    /// Keep enabled in non-production environments.
    /// </summary>
    public bool UseTestAds { get; set; } = true;

    /// <summary>
    /// Optional default ad slot identifier applied when a component consumer does not provide one.
    /// </summary>
    public string? DefaultSlot { get; set; }
}
