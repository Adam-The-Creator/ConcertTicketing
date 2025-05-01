/*
	PROCEDURE DEFINITIONS
*/


/*
	VARIABLE DECLARATIONS 
*/


/*
	NOTES

	SQL SERVER ARCHITECTURE
		
		SERVER NAME					(NAT) IP:PORT			HOST IP:PORT
		DESKTOP-3I9NATQ\MAIN		192.168.63.129:1500		HOST-IP:11500		PORT FORWARDING
		DESKTOP-3I9NATQ\SECONDARY	192.168.63.129:1501		HOST-IP:11501		PORT FORWARDING
		DESKTOP-3I9NATQ\LOG			192.168.63.129:1502		HOST-IP:11502		PORT FORWARDING

	MAIN SERVER ROLES:
		Provides RW access to the API.
		Master-Master Replication between MAIN-SECONDARY.

	SECONDARY SERVER ROLES:
		Provides RW access to the API.
		Master-Master Replication between MAIN-SECONDARY.

	LOG SERVER ROLES:
		It receives backup and log datas.
		Master-Slave Replication between MAIN-LOG.
		Master-Slave Replication between SECONDARY-LOG.
*/


/*
	CREATING DATABASE AND TABLES
*/
--CREATE DATABASE ConcertTicketingDB
GO
USE ConcertTicketingDB


/* CREATEING TABLE FOR USER CREDENTIALS */
IF OBJECT_ID('Passwords') IS NULL CREATE TABLE Passwords(		--MOT
	-- ATTRIBUTES
	ID UNIQUEIDENTIFIER CONSTRAINT DF_Passwords_ID DEFAULT NEWID(),
	HashedPassword VARCHAR(72) NOT NULL,
	--Salt VARCHAR(256),					--Not needed in case of BCrypt
	-- CONSTRAINTS
	CONSTRAINT PK_Passwords_ID PRIMARY KEY(ID)
);

IF OBJECT_ID('UserRoles') IS NULL CREATE TABLE UserRoles(
	-- ATTRIBUTES
	ID TINYINT IDENTITY(1, 1),
	RoleName VARCHAR(20) NOT NULL,
	-- CONSTRAINTS
	CONSTRAINT PK_UserRoles_ID PRIMARY KEY(ID),
	CONSTRAINT UQ_UserRoles_RoleName UNIQUE(RoleName),
	CONSTRAINT CK_UserRoles_RoleName CHECK (RoleName IN ('Admin', 'Customer'))
);

IF OBJECT_ID('Users') IS NULL CREATE TABLE Users(				--MOT
	-- ATTRIBUTES
	ID UNIQUEIDENTIFIER CONSTRAINT DF_Users_ID DEFAULT NEWID(),
	Username VARCHAR(256) NOT NULL,
	Email VARCHAR(256) NOT NULL,
	SignedIn DATETIME NULL CONSTRAINT DF_Users_SignedIn DEFAULT NULL,
	Created DATETIME NOT NULL CONSTRAINT DF_Users_Created DEFAULT GETDATE(),
	PasswordID UNIQUEIDENTIFIER,
	UserRoleID TINYINT NOT NULL,
	-- CONSTRAINTS
	CONSTRAINT PK_Users_ID PRIMARY KEY(ID),
	CONSTRAINT UQ_Users_Email UNIQUE(Email),
	CONSTRAINT CK_Users_Email CHECK (Email LIKE '%@%.%'),
	CONSTRAINT UQ_Users_PasswordID UNIQUE(PasswordID),
	CONSTRAINT FK_Users_PasswordID FOREIGN KEY(PasswordID) REFERENCES Passwords(ID) ON DELETE NO ACTION,
	CONSTRAINT FK_Users_UserRoleID FOREIGN KEY(UserRoleID) REFERENCES UserRoles(ID) ON DELETE CASCADE ON UPDATE CASCADE
);

/* CREATING TABLES FOR CONCERTS */
IF OBJECT_ID('ConcertGroups') IS NULL CREATE TABLE ConcertGroups(
	-- Concerts can be a) individual, b) included in a music festival daily ticket, or c) part of a festival but requiring a separate purchase.
	-- ATTRIBUTES
	ID INT IDENTITY(1, 1),
	Name VARCHAR(256) NOT NULL,
	-- CONSTRAINTS
	CONSTRAINT PK_ConcertGroups_ID PRIMARY KEY(ID),
	CONSTRAINT UQ_ConcertGroups_Name UNIQUE(Name)
);

IF OBJECT_ID('ConcertStatuses') IS NULL CREATE TABLE ConcertStatuses(
	-- ATTRIBUTES
	ID TINYINT IDENTITY(1, 1),
	Status VARCHAR(20) NOT NULL,
	-- CONSTRAINTS
	CONSTRAINT PK_ConcertStatuses_ID PRIMARY KEY(ID),
	CONSTRAINT UQ_ConcertStatuses_Status UNIQUE(Status),
	CONSTRAINT CK_ConcertStatuses_Status CHECK (Status IN ('Upcoming', 'Cancelled', 'Finished'))
);

IF OBJECT_ID('Venues') IS NULL CREATE TABLE Venues(
	-- ATTRIBUTES
	ID BIGINT IDENTITY(1, 1),
	Name VARCHAR(256) NOT NULL,
	Location VARCHAR(256) NOT NULL,
	Type VARCHAR(256),		-- concert hall, theater, stadium, ...
	Capacity INT,
	-- CONSTRAINTS
	CONSTRAINT PK_Venues_ID PRIMARY KEY(ID),
	CONSTRAINT CK_Venues_Capacity CHECK (Capacity > 0 OR Capacity IS NULL)
);

IF OBJECT_ID('Artists') IS NULL CREATE TABLE Artists(
	-- ATTRIBUTES
	ID BIGINT IDENTITY(1, 1),
	ArtistName NVARCHAR(128) NOT NULL,		-- Martin Garrix, David Guetta, ... / ALTERNATE KEY
	-- CONSTRAINTS
	CONSTRAINT PK_Artists_ID PRIMARY KEY(ID),
	CONSTRAINT UQ_Artists_Name UNIQUE(ArtistName)
);

IF OBJECT_ID('Genres') IS NULL CREATE TABLE Genres(
	-- ATTRIBUTES
	ID INT IDENTITY(1, 1),
	GenreName VARCHAR(256) NOT NULL,		-- rock, jazz, classical, ... / ALTERNATE KEY
	-- CONSTRAINTS
	CONSTRAINT PK_Genres_ID PRIMARY KEY(ID),
	CONSTRAINT UQ_Genres_Name UNIQUE(GenreName)
);
	
IF OBJECT_ID('ArtistRoles') IS NULL CREATE TABLE ArtistRoles(
	-- ATTRIBUTES
	ID TINYINT IDENTITY(1, 1),
	RoleName VARCHAR(256) NOT NULL,		-- opening act, special guest, ... / ALTERNATE KEY
	-- CONSTRAINTS
	CONSTRAINT PK_ArtistRoles_ID PRIMARY KEY(ID),
	CONSTRAINT UQ_ArtistRoles_Role UNIQUE(RoleName)
);

IF OBJECT_ID('GenresOfArtists') IS NULL CREATE TABLE GenresOfArtists(
	--Reference Table to represent M:N relations
	-- ATTRIBUTES
	ArtistID BIGINT,
	GenreID INT,
	-- CONSTRAINTS
	CONSTRAINT PK_GenresOfArtists_ArtistID_GenreID PRIMARY KEY(ArtistID, GenreID),
	CONSTRAINT FK_GenresOfArtists_ArtistID FOREIGN KEY(ArtistID) REFERENCES Artists(ID) ON DELETE NO ACTION ON UPDATE CASCADE,
	CONSTRAINT FK_GenresOfArtists_GenreID FOREIGN KEY(GenreID) REFERENCES Genres(ID) ON DELETE NO ACTION ON UPDATE CASCADE
);

IF OBJECT_ID('Concerts') IS NULL CREATE TABLE Concerts(
	-- ATTRIBUTES
	ID BIGINT IDENTITY(1, 1),
	ConcertName NVARCHAR(256) NOT NULL,	-- / ALTERNATE KEY
	Description NVARCHAR(1024),
	Date DATETIME NOT NULL,				-- / ALTERNATE KEY
	VenueID BIGINT NOT NULL,
	MainArtistID BIGINT,
	ConcertGroupID INT,
	StatusID TINYINT NOT NULL,
	-- CONSTRAINTS
	CONSTRAINT PK_Concerts_ID PRIMARY KEY(ID),
	CONSTRAINT FK_Concerts_VenueID FOREIGN KEY(VenueID) REFERENCES Venues(ID) ON DELETE NO ACTION ON UPDATE CASCADE,
	CONSTRAINT FK_Concerts_MainArtistID FOREIGN KEY(MainArtistID) REFERENCES Artists(ID) ON DELETE NO ACTION ON UPDATE CASCADE,
	CONSTRAINT FK_Concerts_ConcertGroupID FOREIGN KEY(ConcertGroupID) REFERENCES ConcertGroups(ID) ON DELETE SET NULL ON UPDATE CASCADE,
	CONSTRAINT FK_Concerts_StatusID FOREIGN KEY(StatusID) REFERENCES ConcertStatuses(ID) ON DELETE NO ACTION ON UPDATE CASCADE,
	CONSTRAINT UQ_Concerts_Name_Date_VenueID UNIQUE (ConcertName, Date, VenueID)
);

IF OBJECT_ID('ArtistRolesAtConcerts') IS NULL CREATE TABLE ArtistRolesAtConcerts(
	--Intersection Table
	-- ATTRIBUTES
	--ID BIGINT PRIMARY KEY IDENTITY(1, 1),
	ConcertID BIGINT,
	ArtistID BIGINT,
	RoleID TINYINT,
	-- CONSTRAINTS
	CONSTRAINT PK_ArtistRolesAtConcerts_ConcertID_ArtistID_RoleID PRIMARY KEY (ConcertID, ArtistID, RoleID),								-- / COMPOSITE PRIMARY KEY
	CONSTRAINT FK_ArtistRolesAtConcerts_ConcertID FOREIGN KEY(ConcertID) REFERENCES Concerts(ID) ON DELETE NO ACTION ON UPDATE NO ACTION,
	CONSTRAINT FK_ArtistRolesAtConcerts_ArtistID FOREIGN KEY(ArtistID) REFERENCES Artists(ID) ON DELETE NO ACTION ON UPDATE NO ACTION,			-- / ALTERNATE KEY
	CONSTRAINT FK_ArtistRolesAtConcerts_RoleID FOREIGN KEY(RoleID) REFERENCES ArtistRoles(ID) ON DELETE NO ACTION ON UPDATE NO ACTION				-- / ALTERNATE KEY
);

/* CREATING TABLES FOR TICKETS */
IF OBJECT_ID('TicketDetails') IS NULL CREATE TABLE TicketDetails(				--MOT
	-- ATTRIBUTES
	ID BIGINT IDENTITY(1, 1),
	Description NVARCHAR(256),
	Price MONEY NOT NULL,
	StartDate DATETIME NULL CONSTRAINT DF_TicketDetails_StartDate DEFAULT NULL,				-- The earliest date when a ticket of this category can be purchased.
	EndDate DATETIME NULL,																	-- The latest date when a ticket of this category can be purchased
	Area VARCHAR(256),																		-- front-row seats, VIP area, ...
	ConcertID BIGINT,
	-- CONSTRAINTS
	CONSTRAINT PK_TicketDetails_ID PRIMARY KEY(ID),
	CONSTRAINT CK_TicketDetails_EndDate CHECK (StartDate IS NULL OR EndDate >= StartDate),
	CONSTRAINT FK_TicketDetails_ConcertID FOREIGN KEY(ConcertID) REFERENCES Concerts(ID) ON DELETE CASCADE ON UPDATE CASCADE
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

IF OBJECT_ID('Tickets') IS NULL CREATE TABLE Tickets(							--MOT
	-- ATTRIBUTES
	ID UNIQUEIDENTIFIER CONSTRAINT DF_Tickets_ID DEFAULT NEWID(),
	SerialNumber VARCHAR(256) NOT NULL,		-- / ALTERNATE KEY
	Seat VARCHAR(256),
	PurchaseDate DATETIME,
	TicketDetailID BIGINT,
	ConcertID BIGINT,
	TicketStatusID TINYINT,
	-- CONSTRAINTS
	CONSTRAINT PK_Tickets_ID PRIMARY KEY(ID),
	CONSTRAINT UQ_Tickets_SerialNumber UNIQUE(SerialNumber),
	CONSTRAINT FK_Tickets_TicketDetailID FOREIGN KEY(TicketDetailID) REFERENCES TicketDetails(ID) ON DELETE NO ACTION ON UPDATE NO ACTION,
	CONSTRAINT FK_Tickets_ConcertID FOREIGN KEY(ConcertID) REFERENCES Concerts(ID) ON DELETE NO ACTION ON UPDATE NO ACTION,
	CONSTRAINT FK_Tickets_TicketStatusID FOREIGN KEY(TicketStatusID) REFERENCES TicketStatuses(ID) ON DELETE NO ACTION ON UPDATE NO ACTION
);

/* CREATING TABLES FOR DISCOUNTS */
IF OBJECT_ID('DiscountStatuses') IS NULL CREATE TABLE DiscountStatuses(
	-- ATTRIBUTES
	ID TINYINT IDENTITY(1, 1),
	Status VARCHAR(20) NOT NULL,
	-- CONSTRAINTS
	CONSTRAINT PK_DiscountStatuses_ID PRIMARY KEY(ID),
	CONSTRAINT UQ_DiscountStatuses_Status UNIQUE(Status),
	CONSTRAINT CK_DiscountStatuses_Status CHECK (Status IN ('Unused', 'Used'))
);

IF OBJECT_ID('Discounts') IS NULL CREATE TABLE Discounts(
	-- ATTRIBUTES
	ID UNIQUEIDENTIFIER CONSTRAINT DF_Discounts_ID DEFAULT NEWID(),
	DiscountCode VARCHAR(128) NULL CONSTRAINT DF_Discounts_DiscountCode DEFAULT NULL,
	DiscountValue TINYINT NOT NULL,
	Created DATETIME NOT NULL CONSTRAINT DF_Discounts_Created DEFAULT GETDATE(),
	StartDate DATETIME NOT NULL,
	EndDate DATETIME NOT NULL,
	StatusID TINYINT NOT NULL,
	-- CONSTRAINTS
	CONSTRAINT PK_Discounts_ID PRIMARY KEY(ID),
	CONSTRAINT CK_Discounts_DiscountValue CHECK (DiscountValue BETWEEN 0 AND 100),
	CONSTRAINT CK_Discounts_EndDate CHECK (StartDate < EndDate),
	CONSTRAINT FK_Discounts_StatusID FOREIGN KEY(StatusID) REFERENCES DiscountStatuses(ID) ON DELETE NO ACTION ON UPDATE CASCADE
);

/* CREATING TABLES FOR ORDERS */
IF OBJECT_ID('Orders') IS NULL CREATE TABLE Orders(								--MOT
	-- ATTRIBUTES
	ID UNIQUEIDENTIFIER CONSTRAINT DF_Orders_ID DEFAULT NEWID(),
	OrderDate DATETIME NOT NULL,
	DeliveryAddress VARCHAR(256),
	DeliveryEmail VARCHAR(256) NOT NULL,
	PreferredDeliveryTime DATETIME,
	Paid DATETIME,
	Sent DATETIME,
	TotalPrice MONEY NOT NULL,				-- Price of the tickets
	DiscountedPrice MONEY NOT NULL,
	Currency VARCHAR(3) NOT NULL,			-- ISO 4217
	DiscountID UNIQUEIDENTIFIER NULL,
	UserID UNIQUEIDENTIFIER,
	-- CONSTRAINTS
	CONSTRAINT PK_Orders_ID PRIMARY KEY(ID),
	CONSTRAINT CK_Orders_DeliveryEmail CHECK (DeliveryEmail LIKE '%@%.%'),
	CONSTRAINT CK_Orders_DiscountedPrice CHECK (DiscountedPrice >= 0 AND DiscountedPrice <= TotalPrice),
	CONSTRAINT FK_Orders_DiscountID FOREIGN KEY (DiscountID) REFERENCES Discounts(ID) ON DELETE NO ACTION ON UPDATE CASCADE,
	CONSTRAINT FK_Orders_UserID FOREIGN KEY(UserID) REFERENCES Users(ID) ON DELETE SET NULL
);

IF OBJECT_ID('OrderTickets') IS NULL CREATE TABLE OrderTickets(					--MOT
	--Reference Table
	-- ATTRIBUTES
	ID UNIQUEIDENTIFIER CONSTRAINT DF_OrderTickets_ID DEFAULT NEWID(),
	TicketID UNIQUEIDENTIFIER,		-- / ALTERNATE KEY
	OrderID UNIQUEIDENTIFIER,		-- / ALTERNATE KEY
	-- CONSTRAINTS
	CONSTRAINT PK_OrderTickets_ID PRIMARY KEY(ID),
	CONSTRAINT FK_OrderTickets_TicketID FOREIGN KEY(TicketID) REFERENCES Tickets(ID) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT FK_OrderTickets_OrderID FOREIGN KEY(OrderID) REFERENCES Orders(ID) ON DELETE CASCADE ON UPDATE CASCADE
);

/* CREATE NONCLUSTERED INDICES */
GO
CREATE NONCLUSTERED INDEX IX_Passwords_HashedPassword ON Passwords(HashedPassword);
CREATE NONCLUSTERED INDEX IX_UserRoles_RoleName ON UserRoles(RoleName);
--CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);		-- It has been already created by UNIQUE CONSTRAINT
CREATE NONCLUSTERED INDEX IX_Users_Username ON Users(Username);
CREATE NONCLUSTERED INDEX IX_ConcertStatuses_Status ON ConcertStatuses(Status);
CREATE NONCLUSTERED INDEX IX_Venues_Location ON Venues(Location);
CREATE NONCLUSTERED INDEX IX_Artists_ArtistName ON Artists(ArtistName);
CREATE NONCLUSTERED INDEX IX_Genres_GenreName ON Genres(GenreName);
CREATE NONCLUSTERED INDEX IX_ArtistRoles_RoleName ON ArtistRoles(RoleName);
CREATE NONCLUSTERED INDEX IX_GenresOfArtists_ArtistID ON GenresOfArtists(ArtistID);
CREATE NONCLUSTERED INDEX IX_GenresOfArtists_GenreID ON GenresOfArtists(GenreID);
CREATE NONCLUSTERED INDEX IX_Concerts_Date ON Concerts(Date);
CREATE NONCLUSTERED INDEX IX_Concerts_VenueID ON Concerts(VenueID);
CREATE NONCLUSTERED INDEX IX_Concerts_MainArtistID ON Concerts(MainArtistID);
CREATE NONCLUSTERED INDEX IX_Concerts_StatusID ON Concerts(StatusID);
CREATE NONCLUSTERED INDEX IX_TicketDetails_ConcertID ON TicketDetails(ConcertID);
CREATE NONCLUSTERED INDEX IX_TicketStatuses_Status ON TicketStatuses(Status);
CREATE NONCLUSTERED INDEX IX_Tickets_ConcertID ON Tickets(ConcertID);
CREATE NONCLUSTERED INDEX IX_OrderTickets_OrdersID ON OrderTickets(OrderID);
CREATE NONCLUSTERED INDEX IX_OrderTickets_TicketID ON OrderTickets(TicketID);

/* INIT ROLES AND STATUSES */
INSERT INTO UserRoles(RoleName) VALUES ('Admin'), ('Customer');
INSERT INTO ConcertStatuses(Status) VALUES ('Upcoming'), ('Cancelled'), ('Finished');
INSERT INTO TicketStatuses(Status) VALUES ('Available'), ('Reserved'), ('Paid'), ('Cancelled');


/* DROP NONCLUSTERED INDICES */
/*
GO
DROP INDEX dbo.Passwords.IX_Passwords_HashedPassword;
DROP INDEX dbo.UserRoles.IX_UserRoles_RoleName;
--DROP INDEX dbo.Users.IX_Users_Email;
DROP INDEX dbo.Users.IX_Users_Username;
DROP INDEX dbo.ConcertStatuses.IX_ConcertStatuses_Status;
DROP INDEX dbo.Venues.IX_Venues_Location;
DROP INDEX dbo.Artists.IX_Artists_ArtistName;
DROP INDEX dbo.Genres.IX_Genres_GenreName;
DROP INDEX dbo.ArtistRoles.IX_ArtistRoles_RoleName;
DROP INDEX dbo.GenresOfArtists.IX_GenresOfArtists_ArtistID;
DROP INDEX dbo.GenresOfArtists.IX_GenresOfArtists_GenreID;
DROP INDEX dbo.Concerts.IX_Concerts_Date;
DROP INDEX dbo.Concerts.IX_Concerts_VenueID;
DROP INDEX dbo.Concerts.IX_Concerts_MainArtistID;
DROP INDEX dbo.Concerts.IX_Concerts_StatusID;
DROP INDEX dbo.TicketDetails.IX_TicketDetails_ConcertID;
DROP INDEX dbo.TicketStatuses.IX_TicketStatuses_Status;
DROP INDEX dbo.Tickets.IX_Tickets_ConcertID;
DROP INDEX dbo.OrderTickets.IX_OrderTickets_OrdersID;
DROP INDEX dbo.OrderTickets.IX_OrderTickets_TicketID;
*/

/* DROP TABLES */
/*
IF OBJECT_ID('OrderTickets') IS NOT NULL DROP TABLE OrderTickets;
IF OBJECT_ID('Orders') IS NOT NULL DROP TABLE Orders;
IF OBJECT_ID('Discounts') IS NOT NULL DROP TABLE Discounts;
IF OBJECT_ID('DiscountStatuses') IS NOT NULL DROP TABLE DiscountStatuses;
IF OBJECT_ID('Tickets') IS NOT NULL DROP TABLE Tickets;
IF OBJECT_ID('TicketDetails') IS NOT NULL DROP TABLE TicketDetails;
IF OBJECT_ID('TicketStatuses') IS NOT NULL DROP TABLE TicketStatuses;
IF OBJECT_ID('Users') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('Passwords') IS NOT NULL DROP TABLE Passwords;
IF OBJECT_ID('UserRoles') IS NOT NULL DROP TABLE UserRoles;
IF OBJECT_ID('GenresOfArtists') IS NOT NULL DROP TABLE GenresOfArtists;
IF OBJECT_ID('Genres') IS NOT NULL DROP TABLE Genres;
IF OBJECT_ID('ArtistRolesAtConcerts') IS NOT NULL DROP TABLE ArtistRolesAtConcerts;
IF OBJECT_ID('Concerts') IS NOT NULL DROP TABLE Concerts;
IF OBJECT_ID('ConcertGroups') IS NOT NULL DROP TABLE ConcertGroups;
IF OBJECT_ID('ConcertStatuses') IS NOT NULL DROP TABLE ConcertStatuses;
IF OBJECT_ID('ArtistRoles') IS NOT NULL DROP TABLE ArtistRoles;
IF OBJECT_ID('Venues') IS NOT NULL DROP TABLE Venues;
IF OBJECT_ID('Artists') IS NOT NULL DROP TABLE Artists;
*/

----------------------------------------------------------------------------------------------
--------------------------------------- DANGER ZONE ------------------------------------------
----------------------------------------------------------------------------------------------

/*
	DROPPING DATABASE WILL ALSO DELETE THE USERS AND THEIR ROLES AND ACCESS
	CONNCECTED TO THE DATABASE.
*/

/* DROP DATABASE */
/*
USE master;
GO
DROP DATABASE ConcertTicketingDB;
*/