// <copyright file="ElasticSearchReporterProvider.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using AiUo.Extensions.AppMetric.Formatting.ElasticSearch.Client;
using App.Metrics.Abstractions.Filtering;
using App.Metrics.Abstractions.Reporting;
using App.Metrics.Internal;
using App.Metrics.Reporting;
using Microsoft.Extensions.Logging;
using ElasticSearch_BulkPayload = AiUo.Extensions.AppMetric.Reporting.ElasticSearch.BulkPayload;
using ElasticSearch_BulkPayloadBuilder = AiUo.Extensions.AppMetric.Reporting.ElasticSearch.BulkPayloadBuilder;

namespace AiUo.Extensions.AppMetric.Formatting.ElasticSearch;

public class ElasticSearchReporterProvider : IReporterProvider
{
    private readonly ElasticSearchReporterSettings _settings;

    public ElasticSearchReporterProvider(ElasticSearchReporterSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Filter = new NoOpMetricsFilter();
    }

    public ElasticSearchReporterProvider(ElasticSearchReporterSettings settings, IFilterMetrics fitler)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Filter = fitler ?? new NoOpMetricsFilter();
    }

    public IFilterMetrics Filter { get; }

    public IMetricReporter CreateMetricReporter(string name, ILoggerFactory loggerFactory)
    {
        var elasticSearchBulkClient = new ElasticSearchBulkClient(
            loggerFactory,
            _settings.ElasticSearchSettings,
            _settings.HttpPolicy);

        var payloadBuilder = new ElasticSearch_BulkPayloadBuilder(
            _settings.ElasticSearchSettings.Index,
            _settings.MetricNameFormatter,
            _settings.MetricTagValueFormatter,
            _settings.DataKeys);

        return new ReportRunner<ElasticSearch_BulkPayload>(
            async p =>
            {
                var result = await elasticSearchBulkClient.WriteAsync(p.Payload());
                return result;
            },
            payloadBuilder,
            _settings.ReportInterval,
            name,
            loggerFactory);
    }
}