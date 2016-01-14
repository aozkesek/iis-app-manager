SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

--CREATE DATABASE NIPPS_SM
--GO

use [NIPPS_SM]
go

IF OBJECT_ID('conf.ParameterCategories', 'U') IS NOT NULL DROP TABLE [conf].[ParameterCategories]
GO
IF OBJECT_ID('conf.SystemParameters', 'U') IS NOT NULL DROP TABLE [conf].[SystemParameters]
GO
IF SCHEMA_ID('conf') IS NOT NULL DROP SCHEMA [conf]
GO

CREATE SCHEMA [conf]
GO

--CREATE LOGIN [nippsuser] WITH PASSWORD = N'nippspwd', DEFAULT_DATABASE=[NIPPS_SM], CHECK_POLICY=OFF
--GO

create table [conf].[ParameterCategories] (
	[CategoryId] int not null primary key identity(1,1),
	[CategoryName] varchar(255) not null,
	[CreateDate] datetime not null default getdate(),
	[UpdateDate] datetime not null default getdate()
) on [PRIMARY]
go

create unique index [IDX_ParameterCategories_Name] on [conf].[ParameterCategories] ([CategoryName])
go

create table [conf].[SystemParameters] (
	[ParameterId] int not null primary key identity(1,1),
	[CategoryId] int not null,
	[ParameterName] varchar(255) not null,
	[CreateDate] datetime not null default getdate(),
	[UpdateDate] datetime not null default getdate(),
	[ParameterValue] varchar(MAX)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
go

create unique index [IDX_SystemParameters_CategoryName] on [conf].[SystemParameters] ([CategoryId], [ParameterName])
go

insert into [conf].[ParameterCategories] ([CategoryName]) values ('EMAIL')
go
insert into [conf].[SystemParameters] ([ParameterName], [ParameterValue], [CategoryId]) 
	values 
		('EmailHost', 'smtp.gmail.com', 1),
		('EmailPort', '587', 1),
		('EmailUser', 'ippsuser', 1),
		('EmailPassword', 'phoneService', 1),
		('EmailFrom', 'ippsuser@', 1)
go

