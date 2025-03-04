// <copyright file="ElasticSearchReporterSettings.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using AiUo.Extensions.AppMetric.Formatting.ElasticSearch.Client;
using App.Metrics.Abstractions.Reporting;

using App.Metrics.Reporting;
using Constants = AiUo.Extensions.AppMetric.Reporting.ElasticSearch.Constants;

namespace AiUo.Extensions.AppMetric.Formatting.ElasticSearch;

public class ElasticSearchReporterSettings : IReporterSettings
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ElasticSearchReporterSettings" /> class.
    /// </summary>
    public ElasticSearchReporterSettings()
    {
        DataKeys = new MetricValueDataKeys(
            Constants.ElasticsearchDefaults.CustomHistogramDataKeys,
            Constants.ElasticsearchDefaults.CustomMeterDataKeys);

        ElasticSearchSettings = new ElasticSearchSettings(new Uri("http://localhost:9200"), "metrics");
        HttpPolicy = new HttpPolicy
        {
            FailuresBeforeBackoff = Client.Constants.DefaultFailuresBeforeBackoff,
            BackoffPeriod = Client.Constants.DefaultBackoffPeriod,
            Timeout = Client.Constants.DefaultTimeout
        };
        ReportInterval = TimeSpan.FromSeconds(5);
        MetricNameFormatter = Constants.ElasticsearchDefaults.MetricNameFormatter;
        MetricTagValueFormatter = Constants.ElasticsearchDefaults.MetricTagValueFormatter;
    }

    /// <inheritdoc />
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
    // ReSharper disable MemberCanBePrivate.Global
    public MetricValueDataKeys DataKeys { get; set; }
    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore AutoPropertyCanBeMadeGetOnly.Global

    /// <summary>
    ///     Gets or sets the ElasticSearch database settings.
    /// </summary>
    /// <value>
    ///     The ElasticSearch database settings.
    /// </value>
    public ElasticSearchSettings ElasticSearchSettings { get; set; }

    /// <summary>
    ///     Gets or sets the HTTP policy settings which allows circuit breaker configuration to be adjusted
    /// </summary>
    /// <value>
    ///     The HTTP policy.
    /// </value>
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
    // ReSharper disable MemberCanBePrivate.Global
    public HttpPolicy HttpPolicy { get; set; }
    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore AutoPropertyCanBeMadeGetOnly.Global

    /// <summary>
    ///     Gets or sets the metric name formatter func which takes the metric context and name and returns a formatted string
    ///     which will be reported to ElasticSearch as the measurement
    /// </summary>
    /// <value>
    ///     The metric name formatter.
    /// </value>
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
    public Func<string, string, string> MetricNameFormatter { get; set; }
    // ReSharper restore AutoPropertyCanBeMadeGetOnly.Global
    // ReSharper restore MemberCanBePrivate.Global

    /// <summary>
    ///     Gets or sets the metric tag value formatter func which takes the metric name and returns a formatted string
    ///     which will be used when reporting tag values to elasticsearch
    /// </summary>
    /// <value>
    ///     The metric tag value formatter.
    /// </value>
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
    // ReSharper disable MemberCanBePrivate.Global
    public Func<string, string> MetricTagValueFormatter { get; set; }
    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore AutoPropertyCanBeMadeGetOnly.Global

    /// <summary>
    ///     Gets or sets the report interval for which to flush metrics to ElasticSearch.
    /// </summary>
    /// <value>
    ///     The report interval.
    /// </value>
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
    // ReSharper disable MemberCanBePrivate.Global
    public TimeSpan ReportInterval { get; set; }
    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore AutoPropertyCanBeMadeGetOnly.Global
}