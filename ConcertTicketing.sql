/*
	PROCEDURE DEFINITIONS
*/


/*
	GLOBAL VARIABLE DECLARATIONS 
*/
GO
EXEC sp_set_session_context 'DatabaseName', N'ConcertTicketingDB';
EXEC sp_set_session_context 'DropDB', 0;
EXEC sp_set_session_context 'DropTables', 0;
EXEC sp_set_session_context 'DropTriggers', 0;
EXEC sp_set_session_context 'EnableCDC', 1;

/*
	Megjegyzések:
	IDEA: Minden táblához/cellához, amin UPDATE-tel lesz változtatva adat, pl. status, ...
	azokhoz egy-egy log tábla készítése és trigger alapú logolás.
	Ez hasznos lehet audithoz is. Pl. lehetne egy LOG szerver, ami logshipping-gel
	kaphatná csak a log táblákról a snapshot-okat.
*/


/*
	CREATING DATABASE AND TABLES
*/
BEGIN
	CREATE DATABASE ConcertTicketingDB
	USE ConcertTicketingDB


/* CREATEING TABLE FOR CUSTOMER CREDENTIALS */
	IF OBJECT_ID('Passwords') IS NULL CREATE TABLE Passwords(
		-- ATTRIBUTES
		ID VARCHAR(36),
		Password VARCHAR(72) NOT NULL,
		Salt VARCHAR(256),					--Not needed in case of BCrypt
		-- CONSTRAINTS
		CONSTRAINT PK_Passwords_ID PRIMARY KEY(ID),
		CONSTRAINT DF_Passwords_ID DEFAULT NEWID() FOR ID
	);

	IF OBJECT_ID('Customers') IS NULL CREATE TABLE Customers(
		-- ATTRIBUTES
		ID VARCHAR(36),
		Name VARCHAR(256) NOT NULL,
		Email VARCHAR(256) NOT NULL,
		SignedIn DATETIME,
		Created DATETIME NOT NULL,
		PasswordID VARCHAR(36),
		-- CONSTRAINTS
		CONSTRAINT PK_Customers_ID PRIMARY KEY(ID),
		CONSTRAINT DF_Customers_ID DEFAULT NEWID() FOR ID,
		CONSTRAINT UQ_Customers_Email UNIQUE(Email),
		CONSTRAINT CK_Customers_Email CHECK (Email LIKE '%@%.%'),
		CONSTRAINT DF_Customers_Created DEFAULT GETDATE() FOR Created,
		CONSTRAINT FK_Customers_PasswordID FOREIGN KEY(PasswordID) REFERENCES Passwords(ID) ON DELETE SET NULL
	);

/* CREATING TABLES FOR CONCERTS */
	IF OBJECT_ID('ConcertGroups') IS NULL CREATE TABLE ConcertGroups(
		-- Concerts can be a) individual, b) included in a music festival daily ticket, or c) part of a festival but requiring a separate purchase.
		-- ATTRIBUTES
		ID INT IDENTITY(1, 1),
		Name VARCHAR(256),
		-- CONSTRAINTS
		CONSTRAINT PK_ConcertGroups_ID PRIMARY KEY(ID)
	);

	IF OBJECT_ID('ConcertStatuses') IS NULL CREATE TABLE ConcertStatuses(
		-- ATTRIBUTES
		ID TINYINT IDENTITY(1, 1),
		Status VARCHAR(20) NOT NULL,
		-- CONSTRAINTS
		CONSTRAINT PK_ConcertStatuses_ID PRIMARY KEY(ID),
		CONSTRAINT UQ_ConcertStatuses_Status UNIQUE(Status),
		CONSTRAINT CK_ConcertStatuses_Status CHECK (Status IN ('Upcomming', 'Cancelled', 'Finished'))
	);

	IF OBJECT_ID('Venues') IS NULL CREATE TABLE Venues(
		-- ATTRIBUTES
		ID INT IDENTITY(1, 1),
		Name VARCHAR(256),
		Location VARCHAR(256) NOT NULL,
		Type VARCHAR(256),		-- concert hall, theater, stadium, ...
		Capacity INT,
		-- CONSTRAINTS
		CONSTRAINT PK_Venues_ID PRIMARY KEY(ID),
		CONSTRAINT CK_Venues_Capacity CHECK (Capacity > 0 OR Capacity IS NULL)
	);

	IF OBJECT_ID('Artists') IS NULL CREATE TABLE Artists(
		-- ATTRIBUTES
		ID INT IDENTITY(1, 1),
		Name NVARCHAR(128) NOT NULL,		-- Martin Garrix, David Guetta, ... / ALTERNATE KEY
		-- CONSTRAINTS
		CONSTRAINT PK_Artists_ID PRIMARY KEY(ID),
		CONSTRAINT UQ_Artists_Name UNIQUE(Name)
	);

	IF OBJECT_ID('Genres') IS NULL CREATE TABLE Genres(
		-- ATTRIBUTES
		ID INT IDENTITY(1, 1),
		Name VARCHAR(256) NOT NULL,		-- rock, jazz, classical, ... / ALTERNATE KEY
		-- CONSTRAINTS
		CONSTRAINT PK_Genres_ID PRIMARY KEY(ID),
		CONSTRAINT UQ_Genres_Name UNIQUE(Name)
	);
	
	IF OBJECT_ID('Roles') IS NULL CREATE TABLE Roles(
		-- ATTRIBUTES
		ID TINYINT IDENTITY(1, 1),
		Role VARCHAR(256) NOT NULL,		-- opening act, special guest, ... / ALTERNATE KEY
		-- CONSTRAINTS
		CONSTRAINT PK_Roles_ID PRIMARY KEY(ID),
		CONSTRAINT UQ_Roles_Role UNIQUE(Role)
	);

	IF OBJECT_ID('GenresOfArtists') IS NULL CREATE TABLE GenresOfArtists(
		--Reference Table to represent M:N relations
		-- ATTRIBUTES
		ID INT IDENTITY(1, 1),
		ArtistID INT,
		GenreID INT,
		-- CONSTRAINTS
		CONSTRAINT PK_GenresOfArtists_ID PRIMARY KEY(ID),
		CONSTRAINT FK_GenresOfArtists_ArtistID FOREIGN KEY(ArtistID) REFERENCES Artists(ID) ON DELETE CASCADE ON UPDATE CASCADE,
		CONSTRAINT FK_GenresOfArtists_GenreID FOREIGN KEY(GenreID) REFERENCES Genres(ID) ON DELETE CASCADE ON UPDATE CASCADE
	);

	IF OBJECT_ID('Concerts') IS NULL CREATE TABLE Concerts(
		-- ATTRIBUTES
		ID BIGINT IDENTITY(1, 1),
		Name NVARCHAR(256) NOT NULL,	-- / ALTERNATE KEY
		Description NVARCHAR(1024),
		Date DATETIME NOT NULL,			-- / ALTERNATE KEY
		VenueID INT,
		MainArtistID INT,
		ConcertGroupID INT,
		StatusID TINYINT,
		-- CONSTRAINTS
		CONSTRAINT PK_Concerts_ID PRIMARY KEY(ID),
		CONSTRAINT FK_Concerts_VenueID FOREIGN KEY(VenueID) REFERENCES Venues(ID) ON DELETE NO ACTION,
		CONSTRAINT FK_Concerts_ArtistID FOREIGN KEY(MainArtistID) REFERENCES Artists(ID) ON DELETE NO ACTION,
		CONSTRAINT FK_Concerts_ConcertGroupID FOREIGN KEY(ConcertGroupID) REFERENCES ConcertGroups(ID) ON DELETE SET NULL,
		CONSTRAINT FK_Concerts_StatusID FOREIGN KEY(StatusID) REFERENCES ConcertStatuses(ID) ON DELETE SET NULL,
		CONSTRAINT UQ_Concerts_Name_Date_VenueID UNIQUE (Name, Date, VenueID)
	);

	IF OBJECT_ID('ConcertRoles') IS NULL CREATE TABLE ConcertRoles(
		--Intersection Table
		-- ATTRIBUTES
		--ID BIGINT PRIMARY KEY IDENTITY(1, 1),
		ConcertID BIGINT,
		ArtistID INT,
		RoleID TINYINT,
		-- CONSTRAINTS
		CONSTRAINT PK_ConcertRoles_ConcertID_ArtistID_RoleID PRIMARY KEY (ConcertID, ArtistID, RoleID),								-- / COMPOSITE PRIMARY KEY
		CONSTRAINT FK_ConcertRoles_ConcertID FOREIGN KEY(ConcertID) REFERENCES Concerts(ID) ON DELETE CASCADE ON UPDATE CASCADE,
		CONSTRAINT FK_ConcertRoles_ArtistID FOREIGN KEY(ArtistID) REFERENCES Artists(ID) ON DELETE CASCADE ON UPDATE CASCADE,		-- / ALTERNATE KEY
		CONSTRAINT FK_ConcertRoles_RoleID FOREIGN KEY(RoleID) REFERENCES Roles(ID) ON DELETE CASCADE ON UPDATE CASCADE				-- / ALTERNATE KEY
	);

/* CREATING TABLES FOR TICKETS */
	IF OBJECT_ID('TicketCategories') IS NULL CREATE TABLE TicketCategories(
		-- ATTRIBUTES
		ID BIGINT IDENTITY(1, 1),
		Description NVARCHAR(256),
		Price MONEY,
		StartDate DATETIME,					-- The earliest date when a ticket of this category can be purchased.
		EndDate DATETIME,					-- The latest date when a ticket of this category can be purchased
		Area VARCHAR(256),					-- front-row seats, VIP area, ...
		ConcertID BIGINT,
		-- CONSTRAINTS
		CONSTRAINT PK_TicketCategories_ID PRIMARY KEY(ID),
		CONSTRAINT DF_TicketCategories_StartDate DEFAULT NULL FOR StartDate,
		CONSTRAINT CK_TicketCategories_EndDate CHECK (StartDate IS NULL OR EndDate >= StartDate),
		CONSTRAINT FK_TicketCategories_ConcertID FOREIGN KEY(ConcertID) REFERENCES Concerts(ID) ON DELETE SET NULL
	);

	IF OBJECT_ID('TicketStatuses') IS NULL CREATE TABLE TicketStatuses (
		-- ATTRIBUTES
		ID TINYINT IDENTITY(1,1),
		Status VARCHAR(32) NOT NULL,
		-- CONSTRAINTS
		CONSTRAINT PK_TicketStatuses_ID PRIMARY KEY(ID),
		CONSTRAINT UQ_TicketStatuses_Status UNIQUE(Status),
		CONSTRAINT CK_TicketStatuses_Status CHECK (Status IN ('Available', 'Reserved', 'Paid', 'Cancelled'))
	);

	IF OBJECT_ID('Tickets') IS NULL CREATE TABLE Tickets(
		-- ATTRIBUTES
		ID VARCHAR(36),
		SerialNumber VARCHAR(256) NOT NULL,		-- / ALTERNATE KEY
		Seat VARCHAR(256),
		PurchaseDate DATETIME,
		TicketCategoryID BIGINT,
		ConcertID BIGINT,
		TicketStatusID TINYINT,
		-- CONSTRAINTS
		CONSTRAINT PK_Tickets_ID PRIMARY KEY(ID),
		CONSTRAINT DF_Tickets_ID DEFAULT NEWID() FOR ID,
		CONSTRAINT UQ_Tickets_SerialNumber UNIQUE(SerialNumber),
		CONSTRAINT FK_Tickets_TicketCategoryID FOREIGN KEY(TicketCategoryID) REFERENCES TicketCategories(ID) ON DELETE SET NULL,
		CONSTRAINT FK_Tickets_ConcertID FOREIGN KEY(ConcertID) REFERENCES Concerts(ID) ON DELETE SET NULL,
		CONSTRAINT FK_Tickets_TicketStatusID FOREIGN KEY(TicketStatusID) REFERENCES TicketStatuses(ID) ON DELETE SET NULL
	);

/* CREATING TABLES FOR ORDERS */
	IF OBJECT_ID('Orders') IS NULL CREATE TABLE Orders(
		-- ATTRIBUTES
		ID VARCHAR(36),
		OrderDate DATETIME NOT NULL,
		DeliveryAddress VARCHAR(256),
		DeliveryEmail VARCHAR(256),
		PreferredDeliveryTime DATETIME,
		Paid DATETIME,
		Sent DATETIME,
		TotalPrice MONEY NOT NULL,
		Discount MONEY,
		DiscountedPrice MONEY NOT NULL,
		Currency VARCHAR(3) NOT NULL,			-- ISO 4217
		CustomerID VARCHAR(36),
		-- CONSTRAINTS
		CONSTRAINT PK_Orders_ID PRIMARY KEY(ID),
		CONSTRAINT DF_Orders_ID DEFAULT NEWID() FOR ID,
		CONSTRAINT CK_Orders_Discount CHECK (Discount IS NULL OR Discount >= 0 AND Discount <= TotalPrice),
		CONSTRAINT CK_Orders_DiscountedPrice CHECK (DiscountedPrice >= 0 AND DiscountedPrice <= TotalPrice),
		CONSTRAINT FK_Orders_CustomerID FOREIGN KEY(CustomerID) REFERENCES Customers(ID) ON DELETE SET NULL
	);

	IF OBJECT_ID('OrderTickets') IS NULL CREATE TABLE OrderTickets(
		--Reference Table
		-- ATTRIBUTES
		ID INT IDENTITY(1, 1),
		TicketID VARCHAR(36),		-- / ALTERNATE KEY
		OrdersID VARCHAR(36),		-- / ALTERNATE KEY
		-- CONSTRAINTS
		CONSTRAINT PK_OrderTickets_ID PRIMARY KEY(ID),
		CONSTRAINT FK_OrderTickets_TicketID FOREIGN KEY(TicketID) REFERENCES Tickets(ID) ON DELETE CASCADE ON UPDATE CASCADE,
		CONSTRAINT FK_OrderTickets_OrdersID FOREIGN KEY(OrdersID) REFERENCES Orders(ID) ON DELETE CASCADE ON UPDATE CASCADE
	);

	--/* CREATING TABLE FOR LOGGING */
	--IF OBJECT_ID('Logs') IS NULL CREATE TABLE Logs(
	--	-- ATTRIBUTES
	--	ID BIGINT IDENTITY(1, 1),
	--	TableName VARCHAR(256) NOT NULL,
	--	RekordID VARCHAR(256) NOT NULL,
	--	FieldName VARCHAR(256) NOT NULL,
	--	OldValue VARCHAR(256),
	--	NewValue VARCHAR(256),
	--	ChangedAt DATETIME,
	--	ChangedBy VARCHAR(256) NOT NULL,
	--	Event VARCHAR(256) NOT NULL,
	--	-- CONSTRAINTS
	--	CONSTRAINT PK_Logs_ID PRIMARY KEY(ID),
	--	CONSTRAINT DF_Logs_ChangedAt DEFAULT GETDATE() FOR ChangedAt,
	--	CONSTRAINT CK_Logs_Event CHECK (Event IN ('Created', 'Updated', 'Deleted'))
	--);
END

/* TRIGGERS */
--GO
--CREATE TRIGGER trg_Customers_Log ON Customers AFTER INSERT, UPDATE, DELETE AS BEGIN
--    SET NOCOUNT ON;
--    DECLARE @user NVARCHAR(256) = SYSTEM_USER;

--    -- LOG INSERT EVENTS
--    INSERT INTO Logs (TableName, RekordID, FieldName, OldValue, NewValue, ChangedBy, Event)
--    SELECT 'Customers', ID, 'AllFields', NULL, CONCAT(Name, ' | ', Email), @user, 'Created' FROM inserted
--    WHERE NOT EXISTS ( SELECT 1 FROM deleted WHERE deleted.ID = inserted.ID );

--    -- LOG DELETE EVENTS
--    INSERT INTO Logs (TableName, RekordID, FieldName, OldValue, NewValue, ChangedBy, Event)
--    SELECT 'Customers', ID, 'AllFields', CONCAT(Name, ' | ', Email), NULL, @user, 'Deleted' FROM deleted
--    WHERE NOT EXISTS ( SELECT 1 FROM inserted WHERE inserted.ID = deleted.ID );

--    -- LOG UPDATE EVENTS (Email)
--    INSERT INTO Logs (TableName, RekordID, FieldName, OldValue, NewValue, ChangedBy, Event)
--    SELECT 'Customers', i.ID, 'Email', d.Email, i.Email, @user, 'Updated' FROM inserted i
--    JOIN deleted d ON i.ID = d.ID WHERE i.Email <> d.Email;

--	-- LOG UPDATE EVENTS (Name)
--	INSERT INTO Logs (TableName, RekordID, FieldName, OldValue, NewValue, ChangedBy, Event)
--    SELECT 'Customers', i.ID, 'Name', d.Name, i.Name, @user, 'Updated' FROM inserted i
--    JOIN deleted d ON i.ID = d.ID WHERE ISNULL(i.Name, '') <> ISNULL(d.Name, '');
--END;

/* DROP TABLES */
/*
IF OBJECT_ID('OrderTickets') IS NOT NULL DROP TABLE OrderTickets;
IF OBJECT_ID('Orders') IS NOT NULL DROP TABLE Orders;
IF OBJECT_ID('Tickets') IS NOT NULL DROP TABLE Tickets;
IF OBJECT_ID('TicketCategories') IS NOT NULL DROP TABLE TicketCategories;
IF OBJECT_ID('TicketStatuses') IS NOT NULL DROP TABLE TicketStatuses;
IF OBJECT_ID('Customers') IS NOT NULL DROP TABLE Customers;
IF OBJECT_ID('Passwords') IS NOT NULL DROP TABLE Passwords;
IF OBJECT_ID('GenresOfArtists') IS NOT NULL DROP TABLE GenresOfArtists;
IF OBJECT_ID('Genres') IS NOT NULL DROP TABLE Genres;
IF OBJECT_ID('ConcertRoles') IS NOT NULL DROP TABLE ConcertRoles;
IF OBJECT_ID('Concerts') IS NOT NULL DROP TABLE Concerts;
IF OBJECT_ID('ConcertGroups') IS NOT NULL DROP TABLE ConcertGroups;
IF OBJECT_ID('ConcertStatuses') IS NOT NULL DROP TABLE ConcertStatuses;
IF OBJECT_ID('Roles') IS NOT NULL DROP TABLE Roles;
IF OBJECT_ID('Venues') IS NOT NULL DROP TABLE Venues;
IF OBJECT_ID('Artists') IS NOT NULL DROP TABLE Artists;
*/

/* DROP DATABASE */
/*
USE master;
DROP DATABASE ConcertTicketingDB;
*/