Migracion ASP.NET MVC 5 (.NET Framework 4.7) a ASP.NET Core 8.

Muestra el catalogo, arma la orden por sesion, escribe la auditoria al confirmar, acepta depositos JPEG/PNG/PDF de hasta 5 MB que quedan como "Pendiente de validacion", y mide el tiempo de cada request.

Arquitectura Domain: las entidades (Instrument, OrderLine, DepositRequest). Application: los casos de uso y las interfaces. No sabe nada de HTTP. Infrastructure: las implementaciones reales (memoria, archivos, sesion). Web: controllers, vistas, la inyeccion de dependencias y el pipeline. Tests: las pruebas unitarias con xUnit. Las dependencias van de afuera hacia adentro: Web e Infrastructure usan Application, y Application usa Domain.

Que se cambio Sesion: de HttpContext.Current.Session a ISession con JSON. Archivos: de HttpPostedFileBase a IFormFile, y de Server.MapPath a IWebHostEnvironment. Trazabilidad: el ComplianceTraceModule paso a ser un middleware con Stopwatch. Inyeccion de dependencias: saque los new ServicioX() de los controllers, ahora todo se inyecta y se registra en Program.cs. Configuracion: pase de Web.config a appsettings.json.

Tests pruebas unitarias de OrderTicketService con dobles de prueba (FakeOrderTicketStore y RecordingComplianceAudit).
