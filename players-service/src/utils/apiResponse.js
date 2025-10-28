// src/utils/apiResponse.js
const logger = require('../config/logger');

const successResponse = (res, message, data, statusCode = 200) => {
  return res.status(statusCode).json({
    status: 'success',
    message,
    data,
  });
};

const errorResponse = (res, message, statusCode = 500, errors = []) => {
  return res.status(statusCode).json({
    status: 'error',
    message,
    errors,
  });
};

const notFound = (req, res, next) => {
  const error = new Error(`Ruta no encontrada - ${req.originalUrl}`);
  res.status(404);
  next(error);
};

const errorHandler = (err, req, res, next) => {
  const statusCode = res.statusCode === 200 ? 500 : res.statusCode;
  
  // Usamos Winston para registrar el error con todos sus detalles
  logger.error(err.message, { 
    statusCode,
    stack: err.stack,
    url: req.originalUrl,
    method: req.method,
    ip: req.ip,
  });

  errorResponse(
    res,
    process.env.NODE_ENV === 'production' ? 'Ocurrió un error en el servidor' : err.message,
    statusCode
  );
};

module.exports = {
  successResponse,
  errorResponse,
  notFound,
  errorHandler,
};