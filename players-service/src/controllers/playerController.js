// src/controllers/playerController.js

const playerService = require('../services/playerService');
const { successResponse, errorResponse } = require('../utils/apiResponse');
const logger = require('../config/logger');

// ✅ Obtener todos los jugadores (adaptado para Angular)
exports.getAllPlayers = async (req, res, next) => {
  try {
    const players = await playerService.getAllPlayers();

    // Devuelve formato compatible con Angular { items, totalCount }
    res.status(200).json({
      items: players,
      totalCount: players.length
    });
  } catch (error) {
    logger.error('Error al obtener jugadores:', error);
    next(error);
  }
};

// ✅ Obtener jugador por ID
exports.getPlayerById = async (req, res, next) => {
  try {
    const { id } = req.params;
    const player = await playerService.getPlayerById(id);
    if (!player) {
      return errorResponse(res, 'Jugador no encontrado', 404);
    }
    successResponse(res, 'Jugador obtenido con éxito', player);
  } catch (error) {
    next(error);
  }
};

// ✅ Obtener jugadores por ID de equipo
exports.getPlayersByTeam = async (req, res, next) => {
  try {
    const { teamId } = req.params;
    logger.info(`Buscando jugadores para el equipo con ID: ${teamId}`);
    const players = await playerService.getPlayersByTeamId(teamId);

    // También devuelve formato { items, totalCount }
    res.status(200).json({
      items: players,
      totalCount: players.length
    });
  } catch (error) {
    next(error);
  }
};

// ✅ Crear nuevo jugador
exports.createPlayer = async (req, res, next) => {
  try {
    const newPlayer = await playerService.createPlayer(req.body);
    successResponse(res, 'Jugador creado con éxito', newPlayer, 201);
  } catch (error) {
    next(error);
  }
};

// ✅ Actualizar jugador
exports.updatePlayer = async (req, res, next) => {
  try {
    const { id } = req.params;
    const updatedPlayer = await playerService.updatePlayer(id, req.body);
    if (!updatedPlayer) {
      return errorResponse(res, 'Jugador no encontrado para actualizar', 404);
    }
    successResponse(res, 'Jugador actualizado con éxito', updatedPlayer);
  } catch (error) {
    next(error);
  }
};

// ✅ Eliminar jugador
exports.deletePlayer = async (req, res, next) => {
  try {
    const { id } = req.params;
    const result = await playerService.deletePlayer(id);
    if (!result) {
      return errorResponse(res, 'Jugador no encontrado para eliminar', 404);
    }
    successResponse(res, 'Jugador eliminado con éxito', null);
  } catch (error) {
    next(error);
  }
};
