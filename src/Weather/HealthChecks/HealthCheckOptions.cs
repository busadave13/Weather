namespace Weather.HealthChecks;

/// <summary>
/// Configuration options for health checks.
/// </summary>
public class HealthCheckOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "HealthChecks";

    /// <summary>
    /// Maximum number of requests before the Ready health check becomes unhealthy.
    /// Set to 0 or negative to disable the threshold.
    /// When exceeded, Kubernetes stops routing new traffic to the pod.
    /// </summary>
    public long RequestCountThreshold { get; set; } = 0;

    /// <summary>
    /// Additional number of requests after Ready fails before Live also fails.
    /// This gives in-flight requests time to complete before the pod is terminated.
    /// When request count reaches (RequestCountThreshold + LiveGracePeriodRequests),
    /// the Live probe fails and Kubernetes kills the pod.
    /// </summary>
    public long LiveGracePeriodRequests { get; set; } = 50;
}
