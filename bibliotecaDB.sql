/* =========================================================
   BASE DE DATOS: BibliotecaDB
   Laboratorio N°07 - Arquitectura de Capas
   ========================================================= */

IF DB_ID('BibliotecaDB') IS NOT NULL
    DROP DATABASE BibliotecaDB;
GO

CREATE DATABASE BibliotecaDB;
GO

USE BibliotecaDB;
GO

/* =========================================================
   1. TABLAS
   ========================================================= */

CREATE TABLE Autores (
    AutorId      INT IDENTITY(1,1) PRIMARY KEY,
    Nombre       VARCHAR(100) NOT NULL,
    Nacionalidad VARCHAR(50),
    Activo       BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE Libros (
    LibroId    INT IDENTITY(1,1) PRIMARY KEY,
    Titulo     VARCHAR(150) NOT NULL,
    ISBN       VARCHAR(20)  NOT NULL UNIQUE,
    AutorId    INT          NOT NULL,
    Ejemplares INT          NOT NULL DEFAULT 1,
    Activo     BIT          NOT NULL DEFAULT 1,
    CONSTRAINT FK_Libros_Autores FOREIGN KEY (AutorId)
        REFERENCES Autores(AutorId)
);
GO

CREATE TABLE Socios (
    SocioId INT IDENTITY(1,1) PRIMARY KEY,
    DNI     VARCHAR(15)  NOT NULL UNIQUE,
    Nombre  VARCHAR(100) NOT NULL,
    Email   VARCHAR(100),
    Activo  BIT          NOT NULL DEFAULT 1
);
GO

CREATE TABLE Prestamos (
    PrestamoId    INT IDENTITY(1,1) PRIMARY KEY,
    SocioId       INT          NOT NULL,
    FechaPrestamo DATE         NOT NULL DEFAULT GETDATE(),
    FechaLimite   DATE         NOT NULL,
    Estado        VARCHAR(20)  NOT NULL DEFAULT 'Activo',
    CONSTRAINT FK_Prestamos_Socios FOREIGN KEY (SocioId)
        REFERENCES Socios(SocioId)
);
GO

CREATE TABLE DetallePrestamo (
    PrestamoId      INT  NOT NULL,
    LibroId         INT  NOT NULL,
    FechaDevolucion DATE NULL,
    CONSTRAINT PK_DetallePrestamo PRIMARY KEY (PrestamoId, LibroId),
    CONSTRAINT FK_Detalle_Prestamos FOREIGN KEY (PrestamoId)
        REFERENCES Prestamos(PrestamoId),
    CONSTRAINT FK_Detalle_Libros FOREIGN KEY (LibroId)
        REFERENCES Libros(LibroId)
);
GO

/* =========================================================
   2. DATOS DE PRUEBA
   ========================================================= */

SET IDENTITY_INSERT Autores OFF;

INSERT INTO Autores (Nombre, Nacionalidad) VALUES
('Gabriel García Márquez', 'Colombiana'),
('Jorge Luis Borges',      'Argentina'),
('Isabel Allende',         'Chilena'),
('Mario Vargas Llosa',     'Peruana'),
('Julio Cortázar',         'Argentina'),
('Pablo Neruda',           'Chilena'),
('Octavio Paz',            'Mexicana'),
('Miguel de Cervantes',    'Española');
GO

INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares) VALUES
('Cien años de soledad',             '978-0307474728', 1, 5),
('El amor en los tiempos del cólera','978-0307387264', 1, 3),
('Crónica de una muerte anunciada',  '978-1400034956', 1, 4),
('Ficciones',                        '978-0802130303', 2, 2),
('El Aleph',                         '978-0142437883', 2, 3),
('La casa de los espíritus',         '978-1501117015', 3, 4),
('Eva Luna',                         '978-1501117022', 3, 2),
('La ciudad y los perros',           '978-0374523442', 4, 3),
('Conversación en La Catedral',      '978-0374523503', 4, 2),
('Rayuela',                          '978-0394752846', 5, 4),
('Bestiario',                        '978-0394717258', 5, 2),
('Veinte poemas de amor',            '978-0811217040', 6, 6),
('Residencia en la tierra',          '978-0811217064', 6, 3),
('El laberinto de la soledad',       '978-0143109871', 7, 3),
('Piedra de sol',                    '978-0811217095', 7, 2),
('Don Quijote de la Mancha',         '978-0060934347', 8, 8),
('Novelas ejemplares',               '978-0142437876', 8, 3),
('Relato de un náufrago',            '978-1400034963', 1, 2),
('Historia de Mayta',                '978-0374523504', 4, 2),
('La invención de Morel',            '978-0811217071', 2, 3);
GO

INSERT INTO Socios (DNI, Nombre, Email) VALUES
('12345678', 'Ana Pérez',        'ana.perez@mail.com'),
('23456789', 'Luis Gómez',       'luis.gomez@mail.com'),
('34567890', 'María Rodríguez',  'maria.rodriguez@mail.com'),
('45678901', 'Carlos Sánchez',   'carlos.sanchez@mail.com'),
('56789012', 'Laura Fernández',  'laura.fernandez@mail.com'),
('67890123', 'Pedro Martínez',   'pedro.martinez@mail.com'),
('78901234', 'Sofía López',      'sofia.lopez@mail.com'),
('89012345', 'Diego Ramírez',    'diego.ramirez@mail.com'),
('90123456', 'Elena Torres',     'elena.torres@mail.com'),
('01234567', 'Javier Castro',    'javier.castro@mail.com');
GO

-- Préstamo 1: Ana Pérez con 3 libros pendientes (regla de negocio)
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (1, '2026-09-01', '2026-09-15', 'Activo');
INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(1, 1,  NULL),
(1, 10, NULL),
(1, 16, NULL);

-- Préstamo 2: Luis Gómez con 1 libro pendiente
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (2, '2026-09-05', '2026-09-19', 'Activo');
INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(2, 4, NULL);

-- Préstamo 3: María Rodríguez (1 devuelto, 1 pendiente)
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (3, '2026-08-20', '2026-09-03', 'Activo');
INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(3, 6,  '2026-08-30'),
(3, 13, NULL);

-- Préstamo 4: Carlos Sánchez (todos devueltos → Cerrado)
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (4, '2026-08-10', '2026-08-24', 'Cerrado');
INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(4, 2, '2026-08-22'),
(4, 8, '2026-08-23');

-- Préstamo 5: Laura Fernández con 1 libro pendiente
INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (5, '2026-09-10', '2026-09-24', 'Activo');
INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(5, 15, NULL);
GO

/* =========================================================
   3. PROCEDIMIENTOS ALMACENADOS
   ========================================================= */

/* ---------- AUTORES ---------- */

CREATE OR ALTER PROCEDURE sp_Autores_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AutorId, Nombre, Nacionalidad, Activo
    FROM Autores
    WHERE Activo = 1
    ORDER BY Nombre;
END;
GO

CREATE OR ALTER PROCEDURE sp_Autores_ObtenerPorId
    @AutorId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AutorId, Nombre, Nacionalidad, Activo
    FROM Autores
    WHERE AutorId = @AutorId;
END;
GO

/* ---------- LIBROS ---------- */

CREATE OR ALTER PROCEDURE sp_Libros_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares, l.Activo,
           a.Nombre AS NombreAutor
    FROM Libros l
    INNER JOIN Autores a ON l.AutorId = a.AutorId
    WHERE l.Activo = 1
    ORDER BY l.Titulo;
END;
GO

CREATE OR ALTER PROCEDURE sp_Libros_Buscar
    @Filtro VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares, l.Activo,
           a.Nombre AS NombreAutor
    FROM Libros l
    INNER JOIN Autores a ON l.AutorId = a.AutorId
    WHERE l.Activo = 1
      AND (l.Titulo LIKE '%' + @Filtro + '%'
           OR a.Nombre LIKE '%' + @Filtro + '%')
    ORDER BY l.Titulo;
END;
GO

CREATE OR ALTER PROCEDURE sp_Libros_ObtenerPorId
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares, l.Activo,
           a.Nombre AS NombreAutor
    FROM Libros l
    INNER JOIN Autores a ON l.AutorId = a.AutorId
    WHERE l.LibroId = @LibroId;
END;
GO

CREATE OR ALTER PROCEDURE sp_Libros_Insertar
    @Titulo     VARCHAR(150),
    @ISBN       VARCHAR(20),
    @AutorId    INT,
    @Ejemplares INT,
    @NuevoId    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares, Activo)
    VALUES (@Titulo, @ISBN, @AutorId, @Ejemplares, 1);
    SET @NuevoId = SCOPE_IDENTITY();
END;
GO

CREATE OR ALTER PROCEDURE sp_Libros_Actualizar
    @LibroId    INT,
    @Titulo     VARCHAR(150),
    @ISBN       VARCHAR(20),
    @AutorId    INT,
    @Ejemplares INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Libros
    SET Titulo = @Titulo,
        ISBN = @ISBN,
        AutorId = @AutorId,
        Ejemplares = @Ejemplares
    WHERE LibroId = @LibroId;
END;
GO

CREATE OR ALTER PROCEDURE sp_Libros_EliminarLogico
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Libros SET Activo = 0 WHERE LibroId = @LibroId;
END;
GO

CREATE OR ALTER PROCEDURE sp_Libros_ExisteISBN
    @ISBN    VARCHAR(20),
    @LibroId INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Libros WHERE ISBN = @ISBN AND LibroId <> @LibroId;
END;
GO

CREATE OR ALTER PROCEDURE sp_Libros_TienePrestamosPendientes
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*)
    FROM DetallePrestamo dp
    WHERE dp.LibroId = @LibroId AND dp.FechaDevolucion IS NULL;
END;
GO

CREATE OR ALTER PROCEDURE sp_Libros_DescontarEjemplar
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Libros SET Ejemplares = Ejemplares - 1 WHERE LibroId = @LibroId;
END;
GO

CREATE OR ALTER PROCEDURE sp_Libros_DevolverEjemplar
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Libros SET Ejemplares = Ejemplares + 1 WHERE LibroId = @LibroId;
END;
GO

/* ---------- SOCIOS ---------- */

CREATE OR ALTER PROCEDURE sp_Socios_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SocioId, DNI, Nombre, Email, Activo
    FROM Socios
    WHERE Activo = 1
    ORDER BY Nombre;
END;
GO

CREATE OR ALTER PROCEDURE sp_Socios_Buscar
    @Filtro VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SocioId, DNI, Nombre, Email, Activo
    FROM Socios
    WHERE Activo = 1
      AND (Nombre LIKE '%' + @Filtro + '%' OR DNI LIKE '%' + @Filtro + '%')
    ORDER BY Nombre;
END;
GO

CREATE OR ALTER PROCEDURE sp_Socios_ObtenerPorId
    @SocioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SocioId, DNI, Nombre, Email, Activo
    FROM Socios
    WHERE SocioId = @SocioId;
END;
GO

CREATE OR ALTER PROCEDURE sp_Socios_Insertar
    @DNI    VARCHAR(15),
    @Nombre VARCHAR(100),
    @Email  VARCHAR(100),
    @NuevoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Socios (DNI, Nombre, Email, Activo)
    VALUES (@DNI, @Nombre, @Email, 1);
    SET @NuevoId = SCOPE_IDENTITY();
END;
GO

CREATE OR ALTER PROCEDURE sp_Socios_Actualizar
    @SocioId INT,
    @DNI     VARCHAR(15),
    @Nombre  VARCHAR(100),
    @Email   VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Socios
    SET DNI = @DNI, Nombre = @Nombre, Email = @Email
    WHERE SocioId = @SocioId;
END;
GO

CREATE OR ALTER PROCEDURE sp_Socios_EliminarLogico
    @SocioId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Socios SET Activo = 0 WHERE SocioId = @SocioId;
END;
GO

CREATE OR ALTER PROCEDURE sp_Socios_ExisteDNI
    @DNI     VARCHAR(15),
    @SocioId INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM Socios WHERE DNI = @DNI AND SocioId <> @SocioId;
END;
GO

CREATE OR ALTER PROCEDURE sp_Socios_TienePrestamosPendientes
    @SocioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*)
    FROM Prestamos p
    INNER JOIN DetallePrestamo dp ON p.PrestamoId = dp.PrestamoId
    WHERE p.SocioId = @SocioId AND dp.FechaDevolucion IS NULL;
END;
GO

CREATE OR ALTER PROCEDURE sp_Socios_ContarLibrosPendientes
    @SocioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*)
    FROM Prestamos p
    INNER JOIN DetallePrestamo dp ON p.PrestamoId = dp.PrestamoId
    WHERE p.SocioId = @SocioId AND dp.FechaDevolucion IS NULL;
END;
GO

/* ---------- PRESTAMOS ---------- */

-- Insertar cabecera del préstamo
CREATE OR ALTER PROCEDURE sp_Prestamos_Insertar
    @SocioId      INT,
    @FechaPrestamo DATE,
    @FechaLimite   DATE,
    @Estado        VARCHAR(20),
    @NuevoId       INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
    VALUES (@SocioId, @FechaPrestamo, @FechaLimite, @Estado);
    SET @NuevoId = SCOPE_IDENTITY();
END;
GO

-- Insertar detalle (un libro del préstamo)
CREATE OR ALTER PROCEDURE sp_DetallePrestamo_Insertar
    @PrestamoId INT,
    @LibroId    INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion)
    VALUES (@PrestamoId, @LibroId, NULL);
END;
GO

-- Obtener el detalle de un préstamo
CREATE OR ALTER PROCEDURE sp_DetallePrestamo_ListarPorPrestamo
    @PrestamoId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT dp.PrestamoId, dp.LibroId, l.Titulo, dp.FechaDevolucion
    FROM DetallePrestamo dp
    INNER JOIN Libros l ON dp.LibroId = l.LibroId
    WHERE dp.PrestamoId = @PrestamoId;
END;
GO

-- Registrar devolución de un libro del detalle
CREATE OR ALTER PROCEDURE sp_DetallePrestamo_RegistrarDevolucion
    @PrestamoId      INT,
    @LibroId         INT,
    @FechaDevolucion DATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE DetallePrestamo
    SET FechaDevolucion = @FechaDevolucion
    WHERE PrestamoId = @PrestamoId AND LibroId = @LibroId;
END;
GO

-- Verificar si al préstamo le quedan pendientes
CREATE OR ALTER PROCEDURE sp_Prestamos_ContarPendientes
    @PrestamoId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*)
    FROM DetallePrestamo
    WHERE PrestamoId = @PrestamoId AND FechaDevolucion IS NULL;
END;
GO

-- Cambiar estado del préstamo
CREATE OR ALTER PROCEDURE sp_Prestamos_ActualizarEstado
    @PrestamoId INT,
    @Estado     VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Prestamos SET Estado = @Estado WHERE PrestamoId = @PrestamoId;
END;
GO

-- Obtener préstamo por Id
CREATE OR ALTER PROCEDURE sp_Prestamos_ObtenerPorId
    @PrestamoId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PrestamoId, SocioId, FechaPrestamo, FechaLimite, Estado
    FROM Prestamos
    WHERE PrestamoId = @PrestamoId;
END;
GO

-- Reporte de préstamos por rango de fechas (INNER JOIN entre las 4 tablas)
CREATE OR ALTER PROCEDURE sp_Prestamos_ReportePorFechas
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  p.PrestamoId,
            s.SocioId,
            s.Nombre  AS Socio,
            s.DNI,
            l.Titulo  AS Libro,
            l.ISBN,
            p.FechaPrestamo,
            p.FechaLimite,
            dp.FechaDevolucion,
            p.Estado
    FROM Prestamos p
    INNER JOIN DetallePrestamo dp ON p.PrestamoId = dp.PrestamoId
    INNER JOIN Libros  l ON dp.LibroId = l.LibroId
    INNER JOIN Socios  s ON p.SocioId = s.SocioId
    WHERE p.FechaPrestamo BETWEEN @FechaInicio AND @FechaFin
    ORDER BY p.FechaPrestamo, s.Nombre, l.Titulo;
END;
GO

-- Listar préstamos activos con detalle (para pantalla de devolución)
CREATE OR ALTER PROCEDURE sp_Prestamos_ListarActivos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  p.PrestamoId,
            s.Nombre AS Socio,
            p.FechaPrestamo,
            p.FechaLimite,
            p.Estado
    FROM Prestamos p
    INNER JOIN Socios s ON p.SocioId = s.SocioId
    WHERE p.Estado = 'Activo'
    ORDER BY p.FechaPrestamo DESC;
END;
GO

/* =========================================================
   4. VERIFICACIÓN
   ========================================================= */

-- Socio con 3 libros pendientes (Ana Pérez, SocioId = 1)
SELECT s.SocioId, s.Nombre, COUNT(*) AS LibrosPendientes
FROM Socios s
INNER JOIN Prestamos p        ON s.SocioId = p.SocioId
INNER JOIN DetallePrestamo dp ON p.PrestamoId = dp.PrestamoId
WHERE dp.FechaDevolucion IS NULL
GROUP BY s.SocioId, s.Nombre
ORDER BY LibrosPendientes DESC;
GO