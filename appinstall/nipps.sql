
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

use [NIPPS_SM_TEST]
go

--PLEASE check the bottom of this script to update the username and password

create table [auth].[Users] (
	[UserId] int not null primary key identity(1,1),
	[UserName] varchar(100) not null,
	[PasswordHash] varchar(255) not null,
	[FirstName] varchar(255) not null,
	[LastName] varchar(255) not null,
	[Email] varchar(512) not null,
	[InvalidAttemptCount] int not null default 0,
	[LastInvalidAttempt] datetime not null default getdate(),
	[LastSuccessAttempt] datetime not null default getdate(),
	[PasswordUpdateDate] datetime not null default getdate(),
	[CreateDate] datetime not null default getdate(),
	[UpdateDate] datetime not null default getdate()
	
)
go
create unique index [IDX_Users_UserId] on [auth].[Users] ([UserName])
go

--default password of ippsadmin is "IppsAdmin". change the PasswordUpdateDate, so password should not expired at first login
insert into [auth].[Users] 
([UserName],[FirstName],[LastName],[Email], [PasswordHash], [PasswordUpdateDate])
values ('ippsadmin', 'IPPS', 'System Admin', 'ippsadmin@system', 'PRplzaqrr8as8KmvF1Tit67pB5O9QlimWxKR3XgMsiQ=', dateadd(MI, -10, getdate()));
go

--conf schema & tables
CREATE SCHEMA [conf]
GO

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

insert into [conf].[ParameterCategories] ([CategoryName]) values 
('EMAIL'),
('PHONELOCK'),
('NIPPSHOST'),
('WEATHERFORECAST'),
('EXCHANGERATES'),
('BACKUP'),
('CAFETERIAMENU'),
('MEETINGROOMORDER')
go

insert into [conf].[SystemParameters] ([CategoryId], [ParameterName], [ParameterValue]) 
	values 
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EMAIL'), 'EmailHost', 'smtp.gmail.com'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EMAIL'), 'EmailPort', '587'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EMAIL'), 'EmailUser', 'ippsuser'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EMAIL'), 'EmailPassword', 'phoneService'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EMAIL'), 'EmailFrom', 'ippsuser@google.com'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'PHONELOCK'), 'CucmIp', '192.168.111.60'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'PHONELOCK'), 'AxlUser', 'ccmadmin'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'PHONELOCK'), 'AxlPassword', 'netas1234*'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'PHONELOCK'), 'FileName', '${basedir}/logs/Netas.Nipps.Service.PhoneLock.log'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'PHONELOCK'), 'Layout', '${date:format=yyyyMMdd.HHmmss.fff} ${level} ${callsite} ${message} ${exception} ${newline}'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'PHONELOCK'), 'ArchiveFileName', '${basedir}/logs/Netas.Nipps.Service.PhoneLock.{#}.log'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'PHONELOCK'), 'ArchiveEvery', 'Day'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'PHONELOCK'), 'ArchiveNumbering', 'Date'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'PHONELOCK'), 'MaxArchiveFiles', '50'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'PHONELOCK'), 'ArchiveAboveSize', '10'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'PHONELOCK'), 'ConcurrentWrites', 'true'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'PHONELOCK'), 'CreateDirs', 'true'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'PHONELOCK'), 'MinLevel', 'Trace'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'NIPPSHOST'), 'localhost', 'localhost:1473'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'NIPPSHOST'), 'tge36appsrv', 'tge36appsrv'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'WEATHERFORECAST'), 'FileName', '${basedir}/logs/Netas.Nipps.Service.WeatherForecast.log'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'WEATHERFORECAST'), 'Layout', '${date:format=yyyyMMdd.HHmmss.fff} ${level} ${callsite} ${message} ${exception} ${newline}'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'WEATHERFORECAST'), 'ArchiveFileName', '${basedir}/logs/Netas.Nipps.Service.WeatherForecast.{#}.log'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'WEATHERFORECAST'), 'ArchiveEvery', 'Day'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'WEATHERFORECAST'), 'ArchiveNumbering', 'Date'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'WEATHERFORECAST'), 'MaxArchiveFiles', '50'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'WEATHERFORECAST'), 'ArchiveAboveSize', '10'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'WEATHERFORECAST'), 'ConcurrentWrites', 'true'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'WEATHERFORECAST'), 'CreateDirs', 'true'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'WEATHERFORECAST'), 'MinLevel', 'Trace'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EXCHANGERATES'), 'FileName', '${basedir}/logs/Netas.Nipps.Service.ExchangeRates.log'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EXCHANGERATES'), 'Layout', '${date:format=yyyyMMdd.HHmmss.fff} ${level} ${callsite} ${message} ${exception} ${newline}'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EXCHANGERATES'), 'ArchiveFileName', '${basedir}/logs/Netas.Nipps.Service.ExchangeRates.{#}.log'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EXCHANGERATES'), 'ArchiveEvery', 'Day'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EXCHANGERATES'), 'ArchiveNumbering', 'Date'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EXCHANGERATES'), 'MaxArchiveFiles', '50'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EXCHANGERATES'), 'ArchiveAboveSize', '10'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EXCHANGERATES'), 'ConcurrentWrites', 'true'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EXCHANGERATES'), 'CreateDirs', 'true'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EXCHANGERATES'), 'MinLevel', 'Trace'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'BACKUP'), 'TargetPath', 'C:\NIPPSPackages\'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'WEATHERFORECAST'), 'WeatherApiCurrent', 'http://www.mgm.gov.tr/sunum/sondurum-show-2.aspx?m=ISTANBUL&amp;rC=fff&amp;rZ=fff'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'WEATHERFORECAST'), 'WeatherApiForecast', 'http://www.mgm.gov.tr/sunum/tahmin-show-2.aspx?m=ISTANBUL&amp;basla=1&amp;bitir=2&amp;rC=fff&amp;rZ=fff'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'EXCHANGERATES'), 'FinanceApiUrl', 'http://finance.yahoo.com/d/quotes.csv?e=.csv&amp;f=sl1d1t1&amp;s=EURTRY=X,USDTRY=X'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'CAFETERIAMENU'), 'FileName', '${basedir}/logs/Netas.Nipps.Service.CafeteriaMenu.log'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'CAFETERIAMENU'), 'Layout', '${date:format=yyyyMMdd.HHmmss.fff} ${level} ${callsite} ${message} ${exception} ${newline}'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'CAFETERIAMENU'), 'ArchiveFileName', '${basedir}/logs/Netas.Nipps.Service.CafeteriaMenu.{#}.log'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'CAFETERIAMENU'), 'ArchiveEvery', 'Day'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'CAFETERIAMENU'), 'ArchiveNumbering', 'Date'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'CAFETERIAMENU'), 'MaxArchiveFiles', '50'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'CAFETERIAMENU'), 'ArchiveAboveSize', '10'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'CAFETERIAMENU'), 'ConcurrentWrites', 'true'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'CAFETERIAMENU'), 'CreateDirs', 'true'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'CAFETERIAMENU'), 'MinLevel', 'Trace'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'MEETINGROOMORDER'), 'FileName', '${basedir}/logs/Netas.Nipps.Service.MeetingRoomOrder.log'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'MEETINGROOMORDER'), 'Layout', '${date:format=yyyyMMdd.HHmmss.fff} ${level} ${callsite} ${message} ${exception} ${newline}'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'MEETINGROOMORDER'), 'ArchiveFileName', '${basedir}/logs/Netas.Nipps.Service.MeetingRoomOrder.{#}.log'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'MEETINGROOMORDER'), 'ArchiveEvery', 'Day'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'MEETINGROOMORDER'), 'ArchiveNumbering', 'Date'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'MEETINGROOMORDER'), 'MaxArchiveFiles', '50'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'MEETINGROOMORDER'), 'ArchiveAboveSize', '10'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'MEETINGROOMORDER'), 'ConcurrentWrites', 'true'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'MEETINGROOMORDER'), 'CreateDirs', 'true'),
		((select [CategoryId] from [conf].[ParameterCategories] where [CategoryName] = 'MEETINGROOMORDER'), 'MinLevel', 'Trace')
go

--log schema & tables
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

insert into [log].[NippsModules] ([ModuleName], [ParentId], [ModuleStatus], [LogLevelId], [LogReportLevelId], [ArchiveEvery], [ArchiveAboveSize], [MaxArchiveFiles]) 
	values 
		('SystemManager', 0, 0, 4, 5, 3, 100, 9),
		('PhoneLock', 0, 0, 4, 5, 3, 100, 9),
		('WeatherForecast', 0, 0, 4, 5, 3, 100, 9),
		('AuthManager', 0, 0, 4, 5, 3, 100, 9),
		('ConfigManager', 0, 0, 4, 5, 3, 100, 9),
		('LogManager', 0, 0, 4, 5, 3, 100, 9),
		('DeployManager', 0, 0, 4, 5, 3, 100, 9),
		('ExchangeRates', 0, 0, 4, 5, 3, 100, 9)
GO

update [log].[NippsModules] set [ParentId] = (select [ModuleId] from [log].[NippsModules] where [ModuleName] = 'SystemManager') where [ModuleName] in ('AuthManager', 'ConfigManager', 'LogManager', 'DeployManager') 
GO

if not exists (select principal_id from sys.server_principals where name = 'NippsUser') begin
	create login [NippsUser] with password = 'Nipps_2015' 
end
go

if not exists (select principal_id from sys.database_principals where name = 'NippsUser') begin
	create user [NippsUser] for login [NippsUser]
end
go

ALTER ROLE [db_datareader] ADD MEMBER [NippsUser]
GO

ALTER ROLE [db_datawriter] ADD MEMBER [NippsUser]
GO

