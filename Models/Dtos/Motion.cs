public record Motion(
    string id,
    MotionType type,
    string infoSlide,
    string text,
    MotionTheme theme,
    string? tournament,
    string? round
)
{
    internal readonly string Id;
}
