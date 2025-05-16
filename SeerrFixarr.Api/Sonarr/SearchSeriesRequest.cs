namespace SeerrFixarr.Api.Sonarr;

public record SearchSeriesRequest
{
    public string Name => "SeriesSearch";
    public int SeriesId	 { get; init; }
    public static implicit operator SearchSeriesRequest(int seriesId) => new() { SeriesId	 = seriesId };
}