CREATE TABLE [dbo].[LocationHistoryFiles] (
  [RequestId] [uniqueidentifier] NOT NULL,
  [FileName] [nvarchar](max) NOT NULL,
  [File] [varbinary](max) NULL,
  CONSTRAINT [PK_LocationHistoryFiles] PRIMARY KEY NONCLUSTERED ([RequestId])
)
GO

ALTER TABLE [dbo].[LocationHistoryFiles]
  ADD CONSTRAINT [FK_LocationHistoryFiles_Requests_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [dbo].[Requests] ([RequestId]) ON DELETE CASCADE
GO