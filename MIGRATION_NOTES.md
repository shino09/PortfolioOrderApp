# Resumen de migracion

La aplicacion PortfolioOrderApp fue migrada desde ASP.NET MVC 5 sobre .NET Framework 4.7 a ASP.NET Core 8

El objetivo fue mantener el mismo funcionamiento utilizando las nuevas herramientas de ASP.NET Core

## Estructura

La aplicacion se separo en varias capas

- Domain: contiene las entidades principales
- Application: contiene la logica del negocio
- Infrastructure: contiene el acceso a memoria, archivos y sesiones
- Web: contiene los controllers, vistas y configuracion
- Tests: contiene las pruebas unitarias

Esta separacion permite que la logica del negocio no dependa directamente de ASP.NET y facilita las pruebas

## Principales cambios

- Se reemplazo la sesion de MVC por la sesion de ASP.NET Core
- Se reemplazo HttpPostedFileBase por IFormFile para manejar archivos
- Se reemplazo Server.MapPath por IWebHostEnvironment
- Se reemplazo el modulo de trazabilidad por un middleware
- Se agrego inyeccion de dependencias usando DI
- La configuracion paso de Web.config a appsettings.json
- Se agrego manejo de errores mediante UseExceptionHandler
- Se agregaron pruebas unitarias con xUnit

## Funcionalidades

La aplicacion permite

- Consultar instrumentos
- Crear una orden utilizando la sesion
- Registrar una auditoria cuando se confirma una orden
- Subir comprobantes de deposito en formato JPG, PNG o PDF
- Validar que los archivos no superen los 5 MB
- Registrar el deposito inicialmente como Pendiente de validacion
- Medir el tiempo de ejecucion de cada solicitud

## Mejoras realizadas

En la version anterior se encontraron algunos problemas

- Los servicios se creaban directamente dentro de los controllers
- Existia estado compartido mediante variables estaticas
- Habia codigo dependiente directamente de HttpContext
- Los archivos no tenian una validacion adecuada
- Los archivos se almacenaban en una carpeta publica
- Existia configuracion que no se utilizaba
- Habia codigo de desarrollo que no deberia estar en produccion
- Algunos POST no utilizaban proteccion contra ataques CSRF
- Algunos modelos no tenian validaciones adecuadas

Estos problemas fueron corregidos o mejorados durante la migracion

## Pruebas

Se crearon 7 pruebas unitarias utilizando xUnit para comprobar el funcionamiento de OrderTicketService

Se utilizaron implementaciones de prueba para simular el almacenamiento y la auditoria

## Como ejecutar

Para compilar

dotnet build

Para ejecutar la aplicacion

dotnet run --project PortfolioOrderApp.Web

Para ejecutar las pruebas

dotnet test PortfolioOrderApp.Tests