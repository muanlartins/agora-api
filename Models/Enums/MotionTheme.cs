using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MotionTheme {
  economy,
  ri,
  environment,
  ludic,
  philosophy,
  socialMovements,
  politics,
  technology,
  religion,
  popCulture
}