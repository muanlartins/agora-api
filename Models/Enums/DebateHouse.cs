using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DebateHouse {
  og,
  oo,
  cg,
  co
}