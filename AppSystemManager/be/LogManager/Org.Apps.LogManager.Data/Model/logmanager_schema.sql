
use [Apps_SM]
go

IF OBJECT_ID('log.AppsModules', 'U') IS NOT NULL DROP TABLE [log].[AppsModules]
GO
IF OBJECT_ID('log.AppsLogs', 'U') IS NOT NULL DROP TABLE [log].[AppsLogs]
GO
IF SCHEMA_ID('log') IS NOT NULL DROP SCHEMA [log]
GO

CREATE SCHEMA [log]
GO

CREATE TABLE [log].[AppsModules](
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

create unique index [IDX_AppsModules_ModuleName] on [log].[AppsModules] ([ModuleName])
GO

CREATE TABLE [log].[AppsLogs](
	[LogId] int primary key IDENTITY(1,1) NOT NULL,
	[LogModuleName] varchar(255) NOT NULL,
	[LogLevelId] int NOT NULL,
	[CreateDate] datetime NOT NULL default getdate(),
	[UpdateDate] datetime NOT NULL default getdate(),
	[CheckedBy] varchar(255),
	[LogMessage] varchar(8000)

) ON [PRIMARY]
GO

create index [IDX_AppsLogs_LogModuleName] on [log].[AppsLogs] ([LogModuleName])
GO

