public record Debate(
  string id,
  string date,
  string time,
  DebateStyle style,
  DebateVenue venue,
  MotionType motionType,
  MotionTheme motionTheme,
  string motion,
  string[]? infoSlides,
  Member[]? debaters,
  int[]? sps,
  Member chair,
  Member[]? wings
);
