package com.scoreboard.teams.service;

import com.scoreboard.teams.client.PlayersClient;
import com.scoreboard.teams.dto.PlayerDto;
import com.scoreboard.teams.model.Team;
import com.scoreboard.teams.repository.TeamRepository;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class TeamService {

  private final TeamRepository repo;
  private final PlayersClient playersClient; // ya existe en tu árbol de código

  public TeamService(TeamRepository repo, PlayersClient playersClient) {
    this.repo = repo;
    this.playersClient = playersClient;
  }

  // === listado con búsqueda opcional ===
  public Page<Team> findAll(String search, int page, int size) {
    if (page < 0) page = 0;
    if (size <= 0 || size > 100) size = 10;
    String term = (search == null || search.isBlank()) ? null : "%" + search.trim().toLowerCase() + "%";
    return repo.search(term, PageRequest.of(page, size));
  }

  public Team findById(Long id) {
    return repo.findById(id).orElseThrow(() -> new RuntimeException("Team not found"));
  }

  public Team save(Team t) {
    return repo.save(t);
  }

  public void delete(Long id) {
    repo.deleteById(id);
  }

  // === métodos que usa el controller para hablar con el microservicio de jugadores ===
  public List<PlayerDto> getPlayersByTeam(Long teamId) {
    // si quieres, valida que exista el team:
    // repo.findById(teamId).orElseThrow(() -> new RuntimeException("Team not found"));
    return playersClient.getPlayersByTeam(teamId);
  }

  public List<PlayerDto> getPlayersByTeamName(String teamName) {
    return playersClient.getPlayersByTeamName(teamName);
  }
}
