IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Person_PersonId]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Person] DROP CONSTRAINT [DF_Person_PersonId]
END
GO

-- Drop the table if it already exists
IF OBJECT_ID('[dbo].[Person]', 'U') IS NOT NULL
DROP TABLE [dbo].[Person]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Person](
	[PersonId] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[EmailAddress] [nvarchar](255) NULL,
 CONSTRAINT [PK_Person2] PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Person] ADD  CONSTRAINT [DF_Person_PersonId]  DEFAULT (newid()) FOR [PersonId]
GO


