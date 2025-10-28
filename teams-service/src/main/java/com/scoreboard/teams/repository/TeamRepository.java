package com.scoreboard.teams.repository;

import com.scoreboard.teams.model.Team;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

public interface TeamRepository extends JpaRepository<Team, Long> {

  @Query("""
    select t from Team t
    where (:term is null)
       or lower(t.name)  like :term
       or lower(t.city)  like :term
       or lower(t.coach) like :term
  """)
  Page<Team> search(@Param("term") String term, Pageable pageable);
}
