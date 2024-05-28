using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TournamentRole {
  debater,
  judge,
  observer,
  ca,
  dca,
  equity,
  tabby,
  advisor,
  convenor
}