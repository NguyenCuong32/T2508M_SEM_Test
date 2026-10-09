create database BATTLEGAME;
GO

use tempdb
GO

create table Player (
	PlayerID uniqueidentifier Primary key default NewID(),
	PlayerName nvarchar(64) not null unique,
	Fullname nvarchar(128) not null,
	Age int not null check (age > 0),
	[Level] int not null default 1 check ([Level] >= 1),
	Email nvarchar(256) not null unique
);

create table Asset (
	AssetID uniqueidentifier Primary key default NewID(),
	AssetName nvarchar(64) not null unique,
	LevelRequire int not null check (LevelRequire >= 1),
);

create table PlayerAsset (
	PlayerID uniqueidentifier not null,
	AssetID uniqueidentifier not null,
	
	constraint PK_PlayerAsset
		Primary key (PlayerID, AssetID),

	constraint FK_PlayerAsset_Player
		Foreign key (PlayerID) References Player(PlayerID),

	constraint FK_PlayerAsset_Asset
		Foreign key (AssetID) References Asset(AssetID),
);
GO

insert into Player(PlayerName, Fullname, Age, [Level], Email)
Values
	('Player 1', 'Nguyen Van Binh', 20, 10, 'player1@example.com'),
	('Player 2', 'Tran Tam Duong', 19, 3, 'player2@example.com'),
	('Player 3', 'Van Duong Tam', 23, 10, 'player3@example.com');

insert into Asset(AssetName, LevelRequire)
Values
	('Hero1', 1),
	('Hero2', 1);

insert into PlayerAsset(PlayerID, AssetID)
Select p.PlayerID, a.AssetID
From Player p
Join Asset a
	On(p.PlayerName = 'Player 1' And a.AssetName = 'Hero1')
	Or(p.PlayerName = 'Player 2' And a.AssetName = 'Hero2')
	Or(p.PlayerName = 'Player 3' And a.AssetName = 'Hero1');
GO

Select
	ROW_NUMBER() Over (
		Order by p.PlayerName
	) As [No],
	p.PlayerName,
	p.[Level],
	p.Age,
	a.AssetName
From Player p
Inner Join PlayerAsset pa
	On p.PlayerID = pa.PlayerID
Inner Join Asset a
	On pa.AssetID = a.AssetID
Order by p.PlayerName;