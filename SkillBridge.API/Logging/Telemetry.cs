using System.Diagnostics;

namespace SkillBridge.API.Logging;

public static class Telemetry
{
    public const string ActivitySourceName = "SkillBridge.API";
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
