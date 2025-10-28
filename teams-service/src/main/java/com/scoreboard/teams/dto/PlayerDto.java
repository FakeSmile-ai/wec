package com.scoreboard.teams.dto;

import lombok.*;

@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class PlayerDto {
    private Long id;
    private String name;
    private String position;
    private Integer number;
    private String nationality;
    private String teamName;

    // Campos opcionales (si Laravel los devuelve)
    private String email;
    private Integer age;
    private String team;
}
