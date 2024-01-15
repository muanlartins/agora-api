using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DebatePosition {
  pm,
  lo,
  dpm,
  dlo,
  mg,
  mo,
  gw,
  ow
}