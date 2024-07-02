public record Participant(
  string id,
  string tournament,
  string name,
  string society,
  string subscribedAt,
  bool hasPfp,
  string? duoId = null,
  string? emoji = null,
  ParticipantCategory? category = null,
  TournamentRole[]? roles = null
);