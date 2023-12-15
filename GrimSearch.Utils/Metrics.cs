using System.Diagnostics.Metrics;

public static class Metrics
{
    static Meter _meter = new("GrimSearch", "1.0.0");
    static Metrics()
    {
        IndexBuildTime = _meter.CreateHistogram<double>("indexBuildTime", "ms");
        SearchTime = _meter.CreateHistogram<double>("searchTime", "ms");

    }
    public static Meter GetMeter()
    {
        return _meter;
    }

    public static Histogram<double> IndexBuildTime { get; private set; }
    public static Histogram<double> SearchTime { get; private set; }

}