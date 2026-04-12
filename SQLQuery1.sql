SELECT Id, NombreUsuario, Rol, Estado, IntentosIngreso, FechaCreacion 
FROM Usuarios 
WHERE NombreUsuario = 'admin'


SELECT *
FROM Usuarios
WHERE NombreUsuario = 'admin'


-- Ver cuántos usuarios tienen EstadoUsuarioId nulo
SELECT Id, NombreUsuario, EstadoUsuarioId, Descripcion
FROM Usuarios
WHERE EstadoUsuarioId IS NULL;