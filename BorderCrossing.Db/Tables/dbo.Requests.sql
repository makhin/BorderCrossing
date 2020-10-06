CREATE TABLE [dbo].[Requests] (
  [RequestId] [uniqueidentifier] NOT NULL,
  [IpAddress] [nvarchar](max) NULL,
  [UserAgent] [nvarchar](max) NULL,
  [Date] [datetime2] NOT NULL,
  CONSTRAINT [PK_Requests] PRIMARY KEY NONCLUSTERED ([RequestId])
)
GO

CREATE UNIQUE CLUSTERED INDEX [UK_Requests_Date]
  ON [dbo].[Requests] ([Date])
GO