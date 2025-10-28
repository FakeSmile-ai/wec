namespace MatchesService.Models.DTOs;

/// <summary>
/// DTO used when finishing a match — includes scores, fouls, and related data.
/// </summary>
public record ScoreEventItem(int TeamId, int? PlayerId, int Points, DateTime DateRegister);
public record FoulItem(int TeamId, int? PlayerId, DateTime DateRegister);

public record FinishMatchDto(
    int HomeScore,
    int AwayScore,
    int HomeFouls,
    int AwayFouls,
    List<ScoreEventItem>? ScoreEvents,
    List<FoulItem>? Fouls
);
