﻿using System.Text.Json.Serialization;

namespace AppleAppStoreConnect;

[JsonConverter(typeof(RelationshipJsonConverter))]
public class Relationship
{
	public Relationship() { }

	[JsonPropertyName("data")]
	public List<RelationshipData>? Data { get; set; }

	[JsonPropertyName("meta")]
	public Meta Meta { get; set; } = new();

	[JsonPropertyName("links")]
	public Links Links { get; set; } = new();
}