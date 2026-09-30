# INFORME TÉCNICO ENTREGABLE: ARQUITECTURA DE CAPAS
## Sistema de Gestión de Biblioteca (`BibliotecaDB`)
**Curso:** Desarrollo de Aplicaciones Empresariales Avanzadas  
**Laboratorio:** N°07 - Arquitectura en 4 Capas (.NET C# / WPF / ADO.NET / SQL Server)

---

## 1. RESUMEN DE LA ARQUITECTURA ESTRUCTURAL

La solución ha sido construida dividiendo responsabilidades estrictamente en 4 proyectos independientes:

```
                  ┌──────────────────────────────┐
                  │        Biblioteca.WPF        │
                  │   (Capa de Presentación)     │
                  └──────────────┬───────────────┘
                                 │
                                 ▼
                  ┌──────────────────────────────┐
                  │      Biblioteca.Negocio      │
                  │     (Capa de Logica/Reglas)  │
                  └──────────────┬───────────────┘
                                 │
                                 ▼
                  ┌──────────────────────────────┐
                  │       Biblioteca.Datos       │
                  │   (Acceso a Datos ADO.NET)   │
                  └──────────────┬───────────────┘
                                 │
                                 ▼ (Entidades referencia transversal)
                  ┌──────────────────────────────┐
                  │     Biblioteca.Entidades     │
                  │      (Clases POCO / DTO)     │
                  └──────────────────────────────┘
```

### Reglas de Dependencias Aplicadas:
1. **`Biblioteca.Entidades`**: Clase de biblioteca POCO pura. No referencia a ningún otro proyecto.
2. **`Biblioteca.Datos`**: Acceso a SQL Server vía ADO.NET (`SqlCommand`, `SqlParameter`, `SqlTransaction`). Referencia únicamente a `Biblioteca.Entidades`.
3. **`Biblioteca.Negocio`**: Contiene la lógica del negocio, validaciones y la excepción propia `ReglaNegocioException`. Referencia a `Biblioteca.Datos` y `Biblioteca.Entidades`.
4. **`Biblioteca.WPF`**: Interfaz gráfica de usuario. Referencia únicamente a `Biblioteca.Negocio` y `Biblioteca.Entidades`. **ESTRICTAMENTE SIN REFERENCIAS A `Biblioteca.Datos` NI `System.Data.SqlClient`**.

---

## 2. JUSTIFICACIÓN DE REGLAS POR CAPA

### A. Capa de Entidades (`Biblioteca.Entidades`)
- **Responsabilidad:** Representar el modelo de datos mediante objetos POCO (Plain Old C# Objects) sin comportamiento de negocio ni persistencia.
- **Clases:** `Autor`, `Libro`, `Socio`, `Prestamo`, `DetallePrestamo`, `ReportePrestamoDto`.

### B. Capa de Datos (`Biblioteca.Datos`)
- **Responsabilidad:** Ejecutar operaciones CRUD en SQL Server llamando a **Procedimientos Almacenados (SP)** mediante `SqlParameter` para prevenir Inyección SQL.
- **¿Por qué NO van las reglas de negocio aquí?**
  - Si colocáramos validaciones de negocio en la Capa de Datos (como verificar si un socio supera los 3 libros o calcular multas por días de mora), acoplaríamos el almacenamiento con las políticas de la empresa. Si las reglas cambian (por ejemplo, permitir 5 libros o cambiar la multa a S/ 2.00), tendríamos que modificar la capa de acceso a datos o los scripts SQL, violando el Principio de Responsabilidad Única (SRP).
  - La capa de Datos solo se limita a ejecutar comandos SQL de forma segura y devolver colecciones `List<T>` o entidades POCO, garantizando la transaccionalidad (`SqlTransaction`) en operaciones complejas.

### C. Capa de Negocio (`Biblioteca.Negocio`)
- **Responsabilidad:** Enforzar las políticas del dominio antes de autorizar o persistir cualquier cambio.
- **Reglas Implementadas:**
  1. **Límite de 3 Libros Pendientes:** Antes de registrar un préstamo en `PrestamoNegocio`, se valida que `librosPendientesActuales + cantidadSolicitada <= 3`. De lo contrario, se lanza `ReglaNegocioException`.
  2. **Impedir Baja Lógica de Libros o Socios con Préstamos Pendientes:** `LibroNegocio` y `SocioNegocio` verifican mediante `TienePrestamosPendientesAsync` y bloquean la baja lógica.
  3. **No permitir duplicados de ISBN ni DNI:** Validado en `LibroNegocio` y `SocioNegocio` antes de invocar la capa de datos.
  4. **Cálculo de Multas:** `PrestamoNegocio.CalcularMulta` evalúa los días entre `FechaLimite` y `FechaDevolucion`. Multiplica exactamente los días de morosidad por **S/ 1.50**.
  5. **Cambio de Estado de Préstamo:** Al devolver el último libro del detalle, el préstamo se actualiza a estado **"Devuelto"**.
- **¿Por qué NO van estas reglas en la Capa de Presentación (WPF)?**
  - La interfaz de usuario (WPF) solo se encarga de recolectar eventos del usuario y mostrar datos/mensajes. Si colocáramos la lógica en WPF (ej. dentro de los eventos `Button_Click`), duplicaríamos código si mañana creáramos una API Web o una aplicación móvil, dejando la aplicación vulnerable a inconsistencias.
  - Al aislar las reglas en `Biblioteca.Negocio`, garantizamos que la UI solo capture `ReglaNegocioException` y muestre el mensaje en pantalla.

### D. Capa de Presentación (`Biblioteca.WPF`)
- **Responsabilidad:** Renderizado visual moderno, captura de entradas del usuario y llamadas asíncronas (`async/await`) sin congelar el hilo principal de la interfaz (`UI Thread`).
- **Formularios/Vistas:**
  1. `LibrosView`: Mantenimiento con Nombre de Autor y buscador dinámico.
  2. `SociosView`: Mantenimiento con validación visual de DNI.
  3. `PrestamosView`: Selección de socio, contador dinámico de disponibilidad y carrito de préstamo multi-libro.
  4. `DevolucionesView`: Cálculo instantáneo de días de morosidad y multa cobrada (S/ 1.50/día).
  5. `ReportesView`: Filtro por rango de fechas ejecutando el `INNER JOIN` de 4 tablas.

---

## 3. CUMPLIMIENTO DE REQUISITOS TÉCNICOS

| N° | Requisito Evaluado | Estado | Detalles de Implementación |
|---|---|---|---|
| 1 | BD `BibliotecaDB` creada con 5 tablas y FKs | ✔️ Cumplido | Ejecutado en `bibliotecaDB.sql` |
| 2 | `Activo BIT DEFAULT 1` | ✔️ Cumplido | Presente en Autores, Libros y Socios |
| 3 | `ISBN` y `DNI` UNIQUE | ✔️ Cumplido | Restricciones `UNIQUE` en SQL Server |
| 4 | Datos de prueba requeridos | ✔️ Cumplido | 8 autores, 20 libros, 10 socios, 5 préstamos |
| 5 | Socio con 3 libros pendientes | ✔️ Cumplido | Ana Pérez (SocioId=1) posee 3 detalles pendientes |
| 6 | Solución con 4 proyectos | ✔️ Cumplido | Entidades, Datos, Negocio, WPF |
| 7 | Referencias entre capas | ✔️ Cumplido | Respetando la jerarquía estricta |
| 8 | WPF sin referencias a Datos ni SqlClient | ✔️ Cumplido | WPF solo referencia a Negocio y Entidades |
| 9 | Entidades solo con propiedades (POCO) | ✔️ Cumplido | Clases limpias sin código de acceso |
| 10 | Datos con `SqlParameter` | ✔️ Cumplido | 0% concatenación de cadenas SQL |
| 11 | Datos devuelve `List<T>` | ✔️ Cumplido | Nunca se expone `SqlDataReader` ni `DataTable` |
| 12 | `ReglaNegocioException` propia | ✔️ Cumplido | Implementada en `Biblioteca.Negocio.Excepciones` |
| 13 | Eliminación Lógica | ✔️ Cumplido | `Activo = 0` vía Stored Procedures |
| 14 | Validar duplicados de ISBN/DNI | ✔️ Cumplido | Validado en la Capa de Negocio |
| 15 | Bloqueo de baja con préstamos pendientes | ✔️ Cumplido | Lógica de validación previa en Negocio |
| 16 | Máximo 3 libros pendientes por socio | ✔️ Cumplido | Validado en `PrestamoNegocio` |
| 17 | Validar ejemplares > 0 y Activo | ✔️ Cumplido | Verificado en `PrestamoNegocio` |
| 18 | `SqlTransaction` en Préstamo y Devolución | ✔️ Cumplido | Inserción cabecera + detalles + stock en 1 transacción |
| 19 | Multa S/ 1.50 por día de morosidad | ✔️ Cumplido | Calculada en `PrestamoNegocio.CalcularMulta` |
| 20 | Cambio de Estado a "Devuelto" | ✔️ Cumplido | Automatizado cuando pendientes == 0 |
| 21 | Formulario Libros con Nombre de Autor | ✔️ Cumplido | Consulta `INNER JOIN` mapeada en WPF |
| 22 | Formulario Socios | ✔️ Cumplido | Mantenimiento completo |
| 23 | Búsquedas por filtro | ✔️ Cumplido | Búsqueda dinámica en Libros y Socios |
| 24 | Formulario Préstamo Multi-libro | ✔️ Cumplido | Selección de socio + lista de libros seleccionados |
| 25 | Formulario Devolución con Multa | ✔️ Cumplido | Banner visual con desglose de días y multa |
| 26 | Reporte 4 Tablas | ✔️ Cumplido | `sp_Prestamos_ReportePorFechas` con `INNER JOIN` |
| 27 | Cadena de conexión en `App.config` | ✔️ Cumplido | `ConfigurationManager.ConnectionStrings["BibliotecaDB"]` |
| 28 | `async/await` de punta a punta | ✔️ Cumplido | Sin llamadas bloqueantes `.Result` ni `.Wait()` |
| 29 | Eventos `private async void Button_Click` | ✔️ Cumplido | Manejo asíncrono en todos los controles WPF |
| 30 | (Punto Extra) `ILibroRepositorio` e Inyección | ✔️ Cumplido | Interfaz en Negocio inyectada en `LibroNegocio` |

---

## 4. OBSERVACIONES Y CONCLUSIONES

1. **Desacoplamiento Estricto:** Al utilizar la arquitectura en 4 capas, se ha logrado una mantenibilidad superior. La capa de presentación en WPF ignora por completo la existencia de SQL Server o ADO.NET; únicamente se comunica con objetos POCO y servicios de la Capa de Negocio.
2. **Robustez y Seguridad:** El uso exclusivo de `SqlParameter` elimina por completo las vulnerabilidades de Inyección SQL. Asimismo, la administración de `SqlTransaction` garantiza que un error en el descuento de inventario revierta automáticamente la creación del préstamo, evitando datos huérfanos o inconsistencias en la base de datos.
3. **Experiencia de Usuario (UX Asíncrona):** Al implementar `async/await` desde los Stored Procedures hasta los eventos click en WPF (`ExecuteReaderAsync`, `ExecuteNonQueryAsync`), la interfaz gráfica permanece responsiva y fluida en todo momento sin causar bloqueos visuales ("Pantalla congelada").
