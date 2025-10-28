// src/routes/playerRoutes.js

const express = require('express');
const router = express.Router();
const playerController = require('../controllers/playerController');
const { validateCreatePlayer, validateUpdatePlayer } = require('../middlewares/validationMiddleware');

// --- Rutas para Players ---
router.get('/players', playerController.getAllPlayers);
router.get('/players/:id', playerController.getPlayerById);
router.post('/players', validateCreatePlayer, playerController.createPlayer);
router.put('/players/:id', validateUpdatePlayer, playerController.updatePlayer);
router.delete('/players/:id', playerController.deletePlayer);

// --- Ruta para obtener jugadores de un equipo ---
router.get('/teams/:teamId/players', playerController.getPlayersByTeam);

module.exports = router;