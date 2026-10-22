using System.Globalization;

namespace ProcurementApp.Models;

/// <summary>
/// The clock shown in the phone's status bar. The design's mockups use Apple's placeholder
/// "9:41"; this app shows the real Sri Lankan time instead, so the demo reads as a live app.
///
/// The server renders the first value (so the correct time is on screen immediately) and
/// sitepro.js keeps it ticking in the browser.
/// </summary>
public static class DeviceClock
{
    /// <summary>Windows and IANA both appear depending on the machine, so try each.</summary>
    private static readonly string[] ColomboZoneIds = { "Sri Lanka Standard Time", "Asia/Colombo" };

    /// <summary>Sri Lanka has observed no daylight saving since 2006; it is a flat UTC+05:30.</summary>
    private static readonly TimeSpan ColomboOffset = TimeSpan.FromMinutes(330);

    /// <summary>Current Colombo time as the status bar shows it, e.g. "9:41".</summary>
    public static string ColomboTime() =>
        ColomboNow().ToString("h:mm", CultureInfo.InvariantCulture);

    private static DateTimeOffset ColomboNow()
    {
        foreach (var id in ColomboZoneIds)
        {
            try
            {
                var zone = TimeZoneInfo.FindSystemTimeZoneById(id);
                return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone);
            }
            catch (TimeZoneNotFoundException) { /* try the next spelling */ }
            catch (InvalidTimeZoneException) { /* corrupt zone data — fall through */ }
        }

        // Neither id resolved (trimmed ICU data, unusual container): use the fixed offset.
        return DateTimeOffset.UtcNow.ToOffset(ColomboOffset);
    }
}
