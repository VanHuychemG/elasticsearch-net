﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Nest;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.ManagedElasticsearch.Clusters;
using Tests.Framework.MockData;
using static Nest.Infer;

namespace Tests.Aggregations.Bucket.IpRange
{
	[SkipVersion("5.0.0-alpha2", "broken in this release. error reason: Expected numeric type on field [leadDeveloper.iPAddress], but got [ip]")]
	public class IpRangeAggregationUsageTests : AggregationUsageTestBase
	{
		public IpRangeAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override object AggregationJson => new
		{
			ip_ranges = new
			{
				ip_range = new
				{
					field = "leadDeveloper.ipAddress",
					ranges = new object[]
					{
						new {to = "10.0.0.5"},
						new {from = "10.0.0.5"}
					}
				}
			}
		};

		protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
			.IpRange("ip_ranges", ip => ip
				.Field(p => p.LeadDeveloper.IpAddress)
				.Ranges(
					r => r.To("10.0.0.5"),
					r => r.From("10.0.0.5")
				)
			);

		protected override AggregationDictionary InitializerAggs =>
			new IpRangeAggregation("ip_ranges")
			{
				Field = Field((Project p) => p.LeadDeveloper.IpAddress),
				Ranges = new List<Nest.IpRange>
				{
					new Nest.IpRange {To = "10.0.0.5"},
					new Nest.IpRange {From = "10.0.0.5"}
				}
			};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();
			var ipRanges = response.Aggregations.IpRange("ip_ranges");
			ipRanges.Should().NotBeNull();
			ipRanges.Buckets.Should().NotBeNull();
			ipRanges.Buckets.Count.Should().BeGreaterThan(0);
			foreach (var range in ipRanges.Buckets)
				range.DocCount.Should().BeGreaterThan(0);
		}
	}
}
