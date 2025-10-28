package com.scoreboard.teams;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.domain.EntityScan;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;

@SpringBootApplication
@EntityScan(basePackages = "com.scoreboard.teams.model")          // <-- tus @Entity
@EnableJpaRepositories(basePackages = "com.scoreboard.teams.repository") // <-- tus repos
public class TeamsServiceApplication {
    public static void main(String[] args) {
        SpringApplication.run(TeamsServiceApplication.class, args);
        System.out.println("✅ Teams Service iniciado en http://localhost:8082");
    }
}
