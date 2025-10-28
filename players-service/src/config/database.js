// src/config/database.js

const mysql = require('mysql2/promise');
const logger = require('./logger');

const dbConfig = {
  host: process.env.DB_HOST,      // Ahora definido en docker-compose
  user: process.env.DB_USER,      // Ahora definido en docker-compose
  password: process.env.DB_PASSWORD,  // Ahora definido en docker-compose
  database: process.env.DB_NAME,  // Ahora definido en docker-compose
  ssl: false, 
  waitForConnections: true,
  connectionLimit: 10,
  queueLimit: 0,
  connectTimeout: 10000,
};

const pool = mysql.createPool(dbConfig);

const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

async function connectWithRetry(retries = 5) {
  for (let i = 0; i < retries; i++) {
    try {
      const connection = await pool.getConnection();
      logger.info('✅ ¡Conexión a la base de datos exitosa!');
      connection.release();
      return;
    } catch (error) {
      logger.warn(`❌ Intento de conexión ${i + 1}/${retries} fallido. Reintentando en 5 segundos...`);
      await sleep(5000);
      if (i === retries - 1) {
        throw new Error(`No se pudo conectar a la base de datos después de ${retries} intentos. Error: ${error.message}`);
      }
    }
  }
}

module.exports = {
  pool,
  connectWithRetry,
};