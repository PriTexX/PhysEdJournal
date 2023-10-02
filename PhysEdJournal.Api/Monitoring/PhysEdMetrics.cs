using Prometheus;

namespace PhysEdJournal.Api.Monitoring;

public static class PhysEdMetrics
{
    private const string PREFIX = "physedjournal";
    
    public static readonly Counter VisitsCounter = Metrics.CreateCounter($"{PREFIX}_visits_total", "Number of visits for teacher", new[] {"guid", "name", "day_of_week"});
}