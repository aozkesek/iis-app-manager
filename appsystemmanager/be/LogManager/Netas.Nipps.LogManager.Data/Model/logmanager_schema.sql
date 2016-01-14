
use [NIPPS_SM]
go

IF OBJECT_ID('log.NippsModules', 'U') IS NOT NULL DROP TABLE [log].[NippsModules]
GO
IF OBJECT_ID('log.NippsLogs', 'U') IS NOT NULL DROP TABLE [log].[NippsLogs]
GO
IF SCHEMA_ID('log') IS NOT NULL DROP SCHEMA [log]
GO

CREATE SCHEMA [log]
GO

CREATE TABLE [log].[NippsModules](
	[ModuleId] int primary key IDENTITY(1,1) NOT NULL,
	[ModuleName] varchar(255) NOT NULL,
	[ParentId] int,
	[ModuleStatus] int,
	[ModuleLogInfo] varchar(255),
	[LogLevelId] int NOT NULL default 4, --WARNING
	[LogReportLevelId] int NOT NULL default 4, --WARNING
	[ArchiveEvery] int NOT NULL, --Day, Hour, Minute
	[ArchiveAboveSize] int NOT NULL default 10, --in MB
	[MaxArchiveFiles] int NOT NULL default 9,
	[CreateDate] datetime NOT NULL default getdate(),
	[UpdateDate] datetime NOT NULL default getdate()

) ON [PRIMARY]
GO

create unique index [IDX_NippsModules_ModuleName] on [log].[NippsModules] ([ModuleName])
GO

CREATE TABLE [log].[NippsLogs](
	[LogId] int primary key IDENTITY(1,1) NOT NULL,
	[LogModuleName] varchar(255) NOT NULL,
	[LogLevelId] int NOT NULL,
	[CreateDate] datetime NOT NULL default getdate(),
	[UpdateDate] datetime NOT NULL default getdate(),
	[CheckedBy] varchar(255),
	[LogMessage] varchar(8000)

) ON [PRIMARY]
GO

create index [IDX_NippsLogs_LogModuleName] on [log].[NippsLogs] ([LogModuleName])
GO

