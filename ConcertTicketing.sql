/*
	PROCEDURE DEFINITIONS
*/
GO
DROP PROCEDURE IF EXISTS ExecuteSQLQuery;
GO
CREATE PROCEDURE ExecuteSQLQuery @SqlQuery NVARCHAR(MAX) AS BEGIN
    BEGIN TRY
        EXEC sp_executesql @SqlQuery;
    END TRY
    BEGIN CATCH
        PRINT 'Error during executing the sql query:';
        PRINT ERROR_MESSAGE();
    END CATCH
END;
--DROP PROCEDURE IF EXISTS ExecuteSqlQuery;

/*
	VARIABLE DECLARATIONS 
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
		ID VARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
		Password VARCHAR(72) NOT NULL,
		Salt VARCHAR(256)					--Not needed in case of BCrypt
	);

	IF OBJECT_ID('Customers') IS NULL CREATE TABLE Customers(
		ID VARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
		Name VARCHAR(256) NOT NULL,
		Email VARCHAR(256) UNIQUE NOT NULL CHECK (Email LIKE '%@%.%'),
		SignedIn DATETIME,
		Created DATETIME NOT NULL DEFAULT GETDATE(),
		PasswordID VARCHAR(36) CONSTRAINT FK_Customers_PasswordID FOREIGN KEY REFERENCES Passwords(ID) ON DELETE SET NULL
	);

/* CREATING TABLES FOR CONCERTS */
	IF OBJECT_ID('ConcertGroups') IS NULL CREATE TABLE ConcertGroups(
		-- Concerts can be a) individual, b) included in a music festival daily ticket, or c) part of a festival but requiring a separate purchase. 
		ID INT PRIMARY KEY IDENTITY(1, 1),
		Name VARCHAR(256)
	);

	IF OBJECT_ID('ConcertStatuses') IS NULL CREATE TABLE ConcertStatuses(
		ID TINYINT PRIMARY KEY IDENTITY(1, 1),
		Status VARCHAR(20) UNIQUE NOT NULL CHECK (Status IN ('Upcomming', 'Cancelled', 'Finished'))
	);

	IF OBJECT_ID('Venues') IS NULL CREATE TABLE Venues(
		ID INT PRIMARY KEY IDENTITY(1, 1),
		Name VARCHAR(256),
		Location VARCHAR(256) NOT NULL,
		Type VARCHAR(256),		-- concert hall, theater, stadium, ...
		Capacity INT CHECK (Capacity > 0 OR Capacity IS NULL)
	);

	IF OBJECT_ID('Artists') IS NULL CREATE TABLE Artists(
		ID INT PRIMARY KEY IDENTITY(1, 1),
		Name NVARCHAR(128) UNIQUE NOT NULL		-- Martin Garrix, David Guetta, ... / ALTERNATE KEY
	);

	IF OBJECT_ID('Genres') IS NULL CREATE TABLE Genres(
		ID INT PRIMARY KEY IDENTITY(1, 1),
		Name VARCHAR(256) UNIQUE NOT NULL		-- rock, jazz, classical, ... / ALTERNATE KEY
	);
	
	IF OBJECT_ID('Roles') IS NULL CREATE TABLE Roles(
		ID TINYINT PRIMARY KEY IDENTITY(1, 1),
		Role VARCHAR(256) UNIQUE NOT NULL		-- opening act, special guest, ... / ALTERNATE KEY
	);

	IF OBJECT_ID('GenresOfArtists') IS NULL CREATE TABLE GenresOfArtists(
		--Reference Table to represent M:N relations
		ID INT PRIMARY KEY IDENTITY(1, 1),
		ArtistID INT CONSTRAINT FK_GenresOfArtists_ArtistID FOREIGN KEY REFERENCES Artists(ID) ON DELETE CASCADE ON UPDATE CASCADE,
		GenreID INT CONSTRAINT FK_GenresOfArtists_GenreID FOREIGN KEY REFERENCES Genres(ID) ON DELETE CASCADE ON UPDATE CASCADE
	);

	IF OBJECT_ID('Concerts') IS NULL CREATE TABLE Concerts(
		ID BIGINT PRIMARY KEY IDENTITY(1, 1),
		Name NVARCHAR(256) NOT NULL,	-- / ALTERNATE KEY
		Description NVARCHAR(1024),
		Date DATETIME NOT NULL,			-- / ALTERNATE KEY
		VenueID INT CONSTRAINT FK_Concerts_VenueID FOREIGN KEY REFERENCES Venues(ID) ON DELETE NO ACTION,
		MainArtistID INT CONSTRAINT FK_Concerts_ArtistID FOREIGN KEY REFERENCES Artists(ID) ON DELETE NO ACTION,
		ConcertGroupID INT CONSTRAINT FK_Concerts_ConcertGroupID FOREIGN KEY REFERENCES ConcertGroups(ID) ON DELETE SET NULL,
		StatusID TINYINT CONSTRAINT FK_Concerts_StatusID FOREIGN KEY REFERENCES ConcertStatuses(ID) ON DELETE SET NULL,
		UNIQUE (Name, Date, VenueID)
	);

	IF OBJECT_ID('ConcertRoles') IS NULL CREATE TABLE ConcertRoles(
		--Intersection Table
		--ID BIGINT PRIMARY KEY,
		ConcertID BIGINT CONSTRAINT FK_ConcertRoles_ConcertID FOREIGN KEY REFERENCES Concerts(ID) ON DELETE CASCADE ON UPDATE CASCADE,
		ArtistID INT CONSTRAINT FK_ConcertRoles_ArtistID FOREIGN KEY REFERENCES Artists(ID) ON DELETE CASCADE ON UPDATE CASCADE,		-- / ALTERNATE KEY
		RoleID TINYINT CONSTRAINT FK_ConcertRoles_RoleID FOREIGN KEY REFERENCES Roles(ID) ON DELETE CASCADE ON UPDATE CASCADE,			-- / ALTERNATE KEY
		PRIMARY KEY (ConcertID, ArtistID, RoleID)		-- / COMPOSITE PRIMARY KEY
	);

/* CREATING TABLES FOR TICKETS */
	IF OBJECT_ID('TicketCategories') IS NULL CREATE TABLE TicketCategories(
		ID BIGINT PRIMARY KEY IDENTITY(1, 1),
		Description NVARCHAR(256),
		Price MONEY,
		StartDate DATETIME DEFAULT NULL,	-- The earliest date when a ticket of this category can be purchased.
		EndDate DATETIME,					-- The latest date when a ticket of this category can be purchased
		Area VARCHAR(256),					-- front-row seats, VIP area, ...
		ConcertID BIGINT CONSTRAINT FK_TicketCategories_ConcertID FOREIGN KEY REFERENCES Concerts(ID) ON DELETE SET NULL,
		CHECK (StartDate IS NULL OR EndDate >= StartDate)
	);

	IF OBJECT_ID('TicketStatuses') IS NULL CREATE TABLE TicketStatuses (
		ID TINYINT PRIMARY KEY IDENTITY(1,1),
		Status VARCHAR(32) UNIQUE NOT NULL CHECK (Status IN ('Available', 'Reserved', 'Paid', 'Cancelled'))
	);

	IF OBJECT_ID('Tickets') IS NULL CREATE TABLE Tickets(
		ID VARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
		SerialNumber VARCHAR(256) UNIQUE NOT NULL,		-- / ALTERNATE KEY
		Seat VARCHAR(256),
		PurchaseDate DATETIME,
		TicketCategoryID BIGINT CONSTRAINT FK_Tickets_TicketCategoryID FOREIGN KEY REFERENCES TicketCategories(ID) ON DELETE SET NULL,
		ConcertID BIGINT CONSTRAINT FK_Tickets_ConcertID FOREIGN KEY REFERENCES Concerts(ID) ON DELETE SET NULL,
		TicketStatusID TINYINT CONSTRAINT FK_Tickets_TicketStatusID FOREIGN KEY REFERENCES TicketStatuses(ID) ON DELETE SET NULL
	);

/* CREATING TABLES FOR ORDERS */
	IF OBJECT_ID('Orders') IS NULL CREATE TABLE Orders(
		ID VARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
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
		CustomerID VARCHAR(36) CONSTRAINT FK_Orders_CustomerID FOREIGN KEY REFERENCES Customers(ID) ON DELETE SET NULL,
		CHECK (Discount IS NULL OR Discount >= 0 AND Discount <= TotalPrice),
		CHECK (DiscountedPrice >= 0 AND DiscountedPrice <= TotalPrice)
	);

	IF OBJECT_ID('OrderTickets') IS NULL CREATE TABLE OrderTickets(
		--Reference Table
		ID INT PRIMARY KEY IDENTITY(1, 1),
		TicketID VARCHAR(36) CONSTRAINT FK_OrderTickets_TicketID FOREIGN KEY REFERENCES Tickets(ID) ON DELETE CASCADE ON UPDATE CASCADE,		-- / ALTERNATE KEY
		OrdersID VARCHAR(36) CONSTRAINT FK_OrderTickets_OrdersID FOREIGN KEY REFERENCES Orders(ID) ON DELETE CASCADE ON UPDATE CASCADE			-- / ALTERNATE KEY
	);

/* CREATING TABLE FOR LOGGING */
--IF OBJECT_ID('Logs') IS NULL BEGIN
--	CREATE TABLE Logs(
--		ID BIGINT PRIMARY KEY IDENTITY(1, 1),
--		TableName VARCHAR(256) NOT NULL,
--		RekordID VARCHAR(256) NOT NULL,
--		FieldName VARCHAR(256) NOT NULL,
--		OldValue VARCHAR(256),
--		NewValue VARCHAR(256),
--		ChangedAt DATETIME DEFAULT GETDATE(),
--		ChangedBy VARCHAR(256) NOT NULL,
--		Event VARCHAR(256) NOT NULL CHECK (Event IN ('Created', 'Updated', 'Deleted'))
--	);
--END;
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

/* CAPTURE DATA CHANGE (CDC) TO LOG EVERY CHANGES IN TABLES */
--IF CONVERT(BIT, SESSION_CONTEXT(N'EnableCDC')) = 1 BEGIN
--	SET @SQLQuery = 'USE [' + CONVERT(VARCHAR, SESSION_CONTEXT(N'DatabaseName')) + ']';
--	EXEC sp_executesql @SQLQuery;

--	EXEC sys.sp_cdc_enable_db;

--	/* CUSTOMERS */
--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'Customers',
--		@role_name = NULL,
--		@supports_net_changes = 1;

--	/* CONCERTS */
--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'ConcertGroups',
--		@role_name = NULL,
--		@supports_net_changes = 1;
		
--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'ConcertStatuses',
--		@role_name = NULL,
--		@supports_net_changes = 1;

--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'Venues',
--		@role_name = NULL,
--		@supports_net_changes = 1;

--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'Artists',
--		@role_name = NULL,
--		@supports_net_changes = 1;

--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'Genres',
--		@role_name = NULL,
--		@supports_net_changes = 1;

--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'Roles',
--		@role_name = NULL,
--		@supports_net_changes = 1;

--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'GenresOfArtists',
--		@role_name = NULL,
--		@supports_net_changes = 1;

--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'Concerts',
--		@role_name = NULL,
--		@supports_net_changes = 1;

--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'ConcertRoles',
--		@role_name = NULL,
--		@supports_net_changes = 1;

--	/* TICKETS */
--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'TicketCategories',
--		@role_name = NULL,
--		@supports_net_changes = 1;
	
--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'TicketStatuses',
--		@role_name = NULL,
--		@supports_net_changes = 1;

--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'Tickets',
--		@role_name = NULL,
--		@supports_net_changes = 1;

--	/* ORDERS */
--	EXEC sys.sp_cdc_enable_table
--		@source_schema = N'dbo',
--		@source_name = N'Orders',
--		@role_name = NULL,
--		@supports_net_changes = 1;
--END;
--ELSE BEGIN
--	SET @SQLQuery = 'USE [' + CONVERT(VARCHAR, SESSION_CONTEXT(N'DatabaseName')) + ']';
--	EXEC sp_executesql @SQLQuery;

--	/* CUSTOMERS */
--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'Customers',
--		@capture_instance = N'dbo_Customers';

--	/* CONCERTS */
--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'ConcertGroups',
--		@capture_instance = N'dbo_ConcertGroups';
		
--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'ConcertStatuses',
--		@capture_instance = N'dbo_ConcertStatuses';

--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'Venues',
--		@capture_instance = N'dbo_Venues';

--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'Artists',
--		@capture_instance = N'dbo_Artists';

--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'Genres',
--		@capture_instance = N'dbo_Genres';

--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'Roles',
--		@capture_instance = N'dbo_Roles';

--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'GenresOfArtists',
--		@capture_instance = N'dbo_GenresOfArtists';

--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'Concerts',
--		@capture_instance = N'dbo_Concerts';

--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'ConcertRoles',
--		@capture_instance = N'dbo_ConcertRoles';

--	/* TICKETS */
--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'TicketCategories',
--		@capture_instance = N'dbo_TicketCategories';
	
--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'TicketStatuses',
--		@capture_instance = N'dbo_TicketStatuses';

--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'Tickets',
--		@capture_instance = N'dbo_Tickets';

--	/* ORDERS */
--	EXEC sys.sp_cdc_disable_table
--		@source_schema = N'dbo',
--		@source_name = N'Orders',
--		@capture_instance = N'dbo_Orders';

--	EXEC sys.sp_cdc_disable_db;
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
--GO
--IF DB_ID(CONVERT(VARCHAR, SESSION_CONTEXT(N'DatabaseName'))) IS NOT NULL AND CONVERT(BIT, SESSION_CONTEXT(N'DropDB')) = 1 BEGIN
--	USE master
--	DECLARE @SQLQuery NVARCHAR(256) = N'';
--	SET @SQLQuery = N'DROP DATABASE [' + CONVERT(NVARCHAR, SESSION_CONTEXT(N'DatabaseName')) + N']';
--	EXEC sp_executesql @SQLQuery;
--END;
/*
USE master;
DROP DATABASE ConcertTicketingDB;
*/