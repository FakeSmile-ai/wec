// src/services/playerService.js

const { pool } = require('../config/database');

exports.getAllPlayers = async () => {
  const [rows] = await pool.query('SELECT * FROM players');
  return rows;
};

exports.getPlayerById = async (id) => {
  const [rows] = await pool.query('SELECT * FROM players WHERE id = ?', [id]);
  return rows[0];
};

exports.getPlayersByTeamId = async (teamId) => {
  const [rows] = await pool.query('SELECT * FROM players WHERE team_id = ?', [teamId]);
  return rows;
};

exports.createPlayer = async (player) => {
  const { name, age, position, team_id } = player;
  const [result] = await pool.query(
    'INSERT INTO players (name, age, position, team_id) VALUES (?, ?, ?, ?)',
    [name, age, position || null, team_id || null]
  );
  return { id: result.insertId, ...player };
};

exports.updatePlayer = async (id, player) => {
    const existingPlayer = await this.getPlayerById(id);
    if (!existingPlayer) return null;
  
    // Fusiona los datos existentes con los nuevos para no perder información
    const playerToUpdate = { ...existingPlayer, ...player };
  
    const { name, age, position, team_id } = playerToUpdate;
    const [result] = await pool.query(
      'UPDATE players SET name = ?, age = ?, position = ?, team_id = ? WHERE id = ?',
      [name, age, position, team_id, id]
    );
  
    if (result.affectedRows === 0) {
      return null;
    }
    return playerToUpdate;
};

exports.deletePlayer = async (id) => {
  const [result] = await pool.query('DELETE FROM players WHERE id = ?', [id]);
  return result.affectedRows > 0;
};