// <copyright file="Constants.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

namespace AiUo.Extensions.AppMetric.Formatting.ElasticSearch.Client;

internal static class Constants
{
    public static readonly TimeSpan DefaultBackoffPeriod = TimeSpan.FromSeconds(30);
    public static readonly int DefaultFailuresBeforeBackoff = 3;
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
}