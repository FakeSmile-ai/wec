namespace MatchesService.Models.DTOs;

/// <summary>
/// DTO used to create a new match using existing team IDs.
/// </summary>
public record NewGameByTeamsDto(
    int HomeTeamId,                 // ID of the home team
    int AwayTeamId,                 // ID of the away team
    int? QuarterDurationSeconds     // Optional quarter duration (in seconds)
);
