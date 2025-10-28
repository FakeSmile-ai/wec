// src/middlewares/validationMiddleware.js

const Joi = require('joi');

const createPlayerSchema = Joi.object({
  name: Joi.string().min(3).max(100).required().messages({
    'string.base': 'El nombre debe ser texto',
    'any.required': 'El nombre es un campo requerido',
  }),
  age: Joi.number().integer().min(1).required().messages({
    'number.base': 'La edad debe ser un número',
    'any.required': 'La edad es un campo requerido',
  }),
  position: Joi.string().max(100).optional().allow(null, ''),
  team_id: Joi.number().integer().optional().allow(null).messages({
    'number.base': 'team_id debe ser un número entero',
  }),
});

const updatePlayerSchema = Joi.object({
    name: Joi.string().min(3).max(100).optional(),
    age: Joi.number().integer().min(1).optional(),
    position: Joi.string().max(100).optional().allow(null, ''),
    team_id: Joi.number().integer().optional().allow(null)
}).min(1).messages({
    'object.min': 'Debes proporcionar al menos un campo para actualizar'
});

const validateRequest = (schema) => {
  return (req, res, next) => {
    const { error } = schema.validate(req.body, {
        abortEarly: false,
        convert: true
    });

    if (error) {
      const errors = error.details.map(detail => detail.message);
      return res.status(400).json({
        status: 'error',
        message: 'Datos de entrada inválidos',
        errors: errors,
      });
    }

    next();
  };
};

module.exports = {
  validateCreatePlayer: validateRequest(createPlayerSchema),
  validateUpdatePlayer: validateRequest(updatePlayerSchema),
};