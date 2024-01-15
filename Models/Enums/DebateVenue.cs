using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DebateVenue {
  remote, 
  inPerson
}