
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

use [Apps_SM]
go

if object_id('auth.Users', 'U') is not null drop table [auth].[Users]
go
IF SCHEMA_ID('auth') IS NOT NULL DROP SCHEMA [auth]
GO

CREATE SCHEMA [auth]
GO

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
