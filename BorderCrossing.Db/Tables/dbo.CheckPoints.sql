CREATE TABLE [dbo].[CheckPoints] (
  [CheckPointId] [int] IDENTITY,
  [RequestId] [uniqueidentifier] NULL,
  [Date] [datetime2] NOT NULL,
  [Point] [geometry] NULL,
  [CountryName] [varchar](50) NULL,
  CONSTRAINT [PK_CheckPoints] PRIMARY KEY CLUSTERED ([CheckPointId])
)
GO

CREATE INDEX [IX_CheckPoints_RequestId]
  ON [dbo].[CheckPoints] ([RequestId])
GO

ALTER TABLE [dbo].[CheckPoints]
  ADD CONSTRAINT [FK_CheckPoints_Requests_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [dbo].[Requests] ([RequestId])
GO