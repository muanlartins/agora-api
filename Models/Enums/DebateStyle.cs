using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DebateStyle {
  bp,
  fundamentals,
  australian,
  other
}