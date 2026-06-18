namespace QueryLab.Api.Infrastructure;

public record QueryResult<T>(T Data, QueryMetrics Metrics);
