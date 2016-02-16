PRAGMA foreign_keys = off;

BEGIN TRANSACTION; -- Table: Section

CREATE TABLE [Section] ( [SectionID] INTEGER PRIMARY KEY NOT NULL, [Description] nvarchar(100) NOT NULL
); 

-- Table: List
CREATE TABLE [List] ( 
    [ListID] INTEGER PRIMARY KEY NOT NULL, 
    [Name] nvarchar(50), 
    [FlightSim] nvarchar(12), 
    [FlightSimVersion] nvarchar(12), 
    [AircraftManufacturer] nvarchar(50), 
    [AircraftModel] nvarchar(50), 
    [SimManufacturer] nvarchar(50), 
    [SimModel] nvarchar(50), 
    [SimVersion] nvarchar(12), 
    [FlowcortVersion] nvarchar(12), 
    [Type] char(1) DEFAULT 'C' NOT NULL CHECK ([Type] IN ('F', 'C', 'T')), [Version] nvarchar(12)
); 

-- Table: Item
CREATE TABLE Item ( 
    ItemID INTEGER PRIMARY KEY NOT NULL, 
    SectionID int NOT NULL, 
    Position int NOT NULL, 
    Location nvarchar (50), 
    Area nvarchar (50), 
    Part nvarchar (50), 
    "Action" nvarchar (50), 
    ValToSet nvarchar (50), 
    CoP bit NOT NULL DEFAULT 0, 
    Turnaround bit NOT NULL DEFAULT 0, 
    Event bit NOT NULL DEFAULT 0, 
    Subsection bit NOT NULL DEFAULT 0, 
    Done bit DEFAULT (0) NOT NULL, 
    Image1 nvarchar (50), 
    Image2 nvarchar (50), 
    Image3 nvarchar (50), 
    Audio nvarchar (50), 
    Video nvarchar (250), 
    Remarks nvarchar (500), 
    FOREIGN KEY (SectionID) 
    REFERENCES Section (SectionID)
); 

COMMIT TRANSACTION;

PRAGMA foreign_keys = ON;
