-- 1. Console Categories (PS4, PS5, etc.)
CREATE TABLE ConsoleCategories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    HourlyRate DECIMAL(18,2) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- 2. Physical Seats / Consoles
CREATE TABLE GameConsoles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ConsoleNumber NVARCHAR(20) NOT NULL UNIQUE,
    CategoryId INT NOT NULL,
    CurrentStatus NVARCHAR(20) NOT NULL DEFAULT 'Available', -- 'Available', 'Taken', 'UnAvailable'
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_GameConsoles_ConsoleCategories 
        FOREIGN KEY (CategoryId) REFERENCES ConsoleCategories(Id)
);

-- 3. Game Catalog
CREATE TABLE Games (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(150) NOT NULL,
    CategoryId INT NULL,
    TotalStock INT NOT NULL DEFAULT 0,
    AvailableStock INT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Games_ConsoleCategories 
        FOREIGN KEY (CategoryId) REFERENCES ConsoleCategories(Id)
);

-- 4. Advance Bookings
CREATE TABLE Bookings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ConsoleId INT NOT NULL,
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerPhone NVARCHAR(20) NOT NULL,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    TotalHours DECIMAL(4,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,                 -- Full estimated cost (TotalHours * HourlyRate)
    DepositAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,  -- What was paid upfront
    BookingStatus NVARCHAR(20) NOT NULL,                -- 'Pending', 'Confirmed', 'Cancelled'
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Bookings_GameConsoles 
        FOREIGN KEY (ConsoleId) REFERENCES GameConsoles(Id)
);

-- 5. Active & Past Play Sessions
CREATE TABLE GameSessions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BookingId INT NULL,
    ConsoleId INT NOT NULL,
    GameId INT NULL,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    TotalHours DECIMAL(4,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,                 -- Final billed cost
    PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,     -- Actual money collected so far
    SessionStatus NVARCHAR(20) NOT NULL,                -- 'Active', 'Completed'
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_GameSessions_Bookings 
        FOREIGN KEY (BookingId) REFERENCES Bookings(Id),
    CONSTRAINT FK_GameSessions_GameConsoles 
        FOREIGN KEY (ConsoleId) REFERENCES GameConsoles(Id),
    CONSTRAINT FK_GameSessions_Games 
        FOREIGN KEY (GameId) REFERENCES Games(Id)
);