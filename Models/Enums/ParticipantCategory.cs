using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ParticipantCategory {
  novice,
  open,
  dino
}