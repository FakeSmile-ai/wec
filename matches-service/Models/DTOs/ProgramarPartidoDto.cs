namespace MatchesService.Models.DTOs;

public record ProgramarPartidoDto(
    int HomeTeamId,
    int AwayTeamId,
    DateTime DateMatch,
    int? QuarterDurationSeconds
);
