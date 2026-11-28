# Taller Mecánico - Avance AP1

## Cómo correrlo
1. Abrir `ProyectoFinalDSW1.sln` en Visual Studio.
2. Herramientas > Administrador de paquetes NuGet > Consola del Administrador de paquetes:
   `Add-Migration Taller`
3. Ejecutar el proyecto con el perfil **https** (F5).
   Al iniciar, el sistema aplica las migraciones, crea los procedimientos de `Data/Sp` y los usuarios iniciales.

## Usuarios iniciales (cambiar la contraseña al entrar)
| Rol | Correo | Contraseña |
|---|---|---|
| Admin | admin@taller.com | Admin#2026 |
| Mecánico | mecanico@taller.com | Mecanico#2026 |
