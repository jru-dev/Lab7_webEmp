# 📚 Sistema de Gestión de Biblioteca (BibliotecaDB)
> **Laboratorio N°07 - Arquitectura en 4 Capas (.NET 6 C# / WPF / ADO.NET / SQL Server)**  
> **Curso:** Desarrollo de Aplicaciones Empresariales Avanzadas — Tecsup

---

## 🏛️ Arquitectura del Sistema

El proyecto implementa un desacoplamiento estricto de responsabilidades organizado en cuatro capas independientes:

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
                                 ▼ (Referencia transversal)
                  ┌──────────────────────────────┐
                  │     Biblioteca.Entidades     │
                  │      (Clases POCO / DTO)     │
                  └──────────────────────────────┘
```

### 📦 Estructura de Proyectos

1. **`Biblioteca.Entidades`**: Clases POCO (`Autor`, `Libro`, `Socio`, `Prestamo`, `DetallePrestamo`, `ReportePrestamoDto`) sin dependencias externas ni lógica de persistencia.
2. **`Biblioteca.Datos`**: Acceso exclusivo a SQL Server mediante ADO.NET (`SqlCommand`, `SqlParameter`, `SqlTransaction`). Consume Stored Procedures parametrizados para mitigar inyecciones SQL.
3. **`Biblioteca.Negocio`**: Centraliza las reglas de validación y de dominio del negocio (límite de 3 libros por socio, morosidad de S/ 1.50 por día, restricción de bajas lógicas con préstamos pendientes y excepciones personalizadas `ReglaNegocioException`).
4. **`Biblioteca.WPF`**: Interfaz gráfica en Windows Presentation Foundation con tema oscuro moderno, comunicación asíncrona (`async/await`) y sin referencias a la capa de datos.

---

## 🛠️ Tecnologías y Requisitos

- **Plataforma:** .NET 6.0 SDK (Windows Desktop / WPF)
- **Lenguaje:** C# 10
- **Base de Datos:** Microsoft SQL Server 2019 / 2022 / Express / LocalDB
- **Herramienta CLI:** `dotnet` CLI y `sqlcmd`

---

## ⚙️ Configuración y Puesta en Marcha

### 1. Clonar el Repositorio
```bash
git clone https://github.com/jru-dev/Lab7_webEmp.git
cd Lab7_webEmp
```

### 2. Configurar la Base de Datos
Ejecuta el script SQL en tu instancia local de SQL Server:
```powershell
sqlcmd -S localhost -E -i "bibliotecaDB.sql"
```
*(Si usas SQL Server Express o una instancia nombrada, sustituye `localhost` por `localhost\SQLEXPRESS`).*

### 3. Cadena de Conexión
Verifica la cadena en `Biblioteca.WPF/App.config`:
```xml
<connectionStrings>
    <add name="BibliotecaDB" 
         connectionString="Server=localhost;Database=BibliotecaDB;Trusted_Connection=True;TrustServerCertificate=True;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

### 4. Compilar y Ejecutar

Restaurar dependencias y compilar:
```powershell
dotnet restore BibliotecaDB.sln
dotnet build BibliotecaDB.sln
```

Ejecutar la aplicación WPF:
```powershell
dotnet run --project Biblioteca.WPF
```
*(O ejecutar directamente el binario generado en `Biblioteca.WPF/bin/Debug/net6.0-windows/Biblioteca.WPF.exe`)*.

---

## 📋 Módulos del Sistema

- **📚 Mantenimiento de Libros:** CRUD completo con filtro de búsqueda por título/autor y validación de duplicados por ISBN.
- **👥 Mantenimiento de Socios:** Registro y control de socios con validación de DNI único.
- **📋 Registro de Préstamos:** Préstamo multi-libro con carrito dinámico, validación de stock y restricción de máx. 3 libros pendientes.
- **↩️ Devoluciones y Multas:** Identificación de préstamos activos, cálculo en tiempo real de días de morosidad y liquidación de penalidad (S/ 1.50/día).
- **📊 Reporte de Préstamos:** Consulta y visualización por rango de fechas mediante `INNER JOIN` de 4 tablas.
