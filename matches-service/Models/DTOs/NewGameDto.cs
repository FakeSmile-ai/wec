namespace MatchesService.Models.DTOs;

/// <summary>
/// DTO used to create a new match with new team names.
/// </summary>
public record NewGameDto(
    string HomeName,                // Home team name
    string AwayName,                // Away team name
    int? QuarterDurationSeconds     // Optional quarter duration (in seconds)
);
