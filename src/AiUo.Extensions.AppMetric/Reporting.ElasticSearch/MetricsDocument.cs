// <copyright file="MetricsDocument.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace AiUo.Extensions.AppMetric.Reporting.ElasticSearch;

public class MetricsDocument
{
    [JsonProperty("name")]
    public string MeasurementName { get; set; }

    [JsonIgnore]
    public string MeasurementType { get; internal set; }

    [JsonProperty("fields")]
    public IDictionary<string, object> Fields { get; set; }

    [JsonProperty("tags")]
    public IDictionary<string, string> Tags { get; set; }

    [JsonProperty("@timestamp")]
    public DateTime WrittenOn { get; set; } = DateTime.UtcNow;
}