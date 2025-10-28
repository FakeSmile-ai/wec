package com.scoreboard.teams.client;

import com.scoreboard.teams.dto.PlayerDto;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;
import org.springframework.web.reactive.function.client.WebClient;

import java.time.Duration;
import java.util.List;

@Component
public class PlayersClient {

  private final WebClient web;

  // Usa env var players.service.base-url o un valor por defecto
  public PlayersClient(
      @Value("${players.service.base-url:http://host.docker.internal:8000/api}") String baseUrl,
      WebClient.Builder builder
  ) {
    this.web = builder.baseUrl(baseUrl).build();
  }

  // === Firmas que espera tu Controller/Service ===
  public List<PlayerDto> getPlayersByTeam(Long teamId) {
    return web.get()
        .uri("/players/team/{id}", teamId)
        .retrieve()
        .bodyToFlux(PlayerDto.class)
        .collectList()
        .block(Duration.ofSeconds(5));
  }

  public List<PlayerDto> getPlayersByTeamName(String teamName) {
    return web.get()
        .uri("/players/team-name/{name}", teamName)
        .retrieve()
        .bodyToFlux(PlayerDto.class)
        .collectList()
        .block(Duration.ofSeconds(5));
  }
}
