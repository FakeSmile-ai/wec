// src/config/logger.js

const fs = require('fs');
const path = require('path');
const { createLogger, format, transports } = require('winston');

const env = process.env.NODE_ENV || 'development';
const logDirectory = path.join(__dirname, '../../logs');

// Crear el directorio de logs si no existe
if (!fs.existsSync(logDirectory)) {
  fs.mkdirSync(logDirectory, { recursive: true });
}

// Formato para los logs en archivo (JSON)
const jsonFormat = format.combine(
  format.timestamp(),
  format.errors({ stack: true }),
  format.splat(),
  format.json()
);

// Formato para los logs en la consola (más legible)
const consoleFormat = format.combine(
  format.colorize(),
  format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss' }),
  format.printf(({ level, message, timestamp, ...metadata }) => {
    let meta = '';
    if (metadata && Object.keys(metadata).length) {
      // Si el stack de un error existe, lo mostramos de forma más clara
      if (metadata.stack) {
        meta = `\n${metadata.stack}`;
      } else {
        meta = ` ${JSON.stringify(metadata)}`;
      }
    }
    return `[${timestamp}] ${level}: ${message}${meta}`;
  })
);

const logger = createLogger({
  level: env === 'production' ? 'info' : 'debug',
  format: consoleFormat, // Por defecto, usamos el formato de consola
  transports: [
    new transports.File({
      filename: path.join(logDirectory, 'error.log'),
      level: 'error',
      format: jsonFormat, // Los archivos siempre en JSON
    }),
    new transports.File({
      filename: path.join(logDirectory, 'combined.log'),
      format: jsonFormat,
    }),
  ],
  exceptionHandlers: [
    new transports.File({ filename: path.join(logDirectory, 'exceptions.log'), format: jsonFormat }),
  ],
  rejectionHandlers: [
    new transports.File({ filename: path.join(logDirectory, 'rejections.log'), format: jsonFormat }),
  ],
});

// Si no estamos en producción, también mostramos todos los logs en la consola
if (env !== 'production') {
  logger.add(
    new transports.Console({
      format: consoleFormat,
    })
  );
}

module.exports = logger;