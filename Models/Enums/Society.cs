using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Society {
  independent,
  gdo,
  sdufrj,
  sdfdv,
  schools,
  hermeneutica,
  sddufsc,
  senatus,
  sdufes,
  sds,
  uspd,
  sdp,
}