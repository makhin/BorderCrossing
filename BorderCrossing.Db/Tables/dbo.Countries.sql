CREATE TABLE [dbo].[Countries] (
  [FIPS] [varchar](2) NULL,
  [ISO2] [varchar](2) NULL,
  [ISO3] [varchar](3) NULL,
  [UN] [smallint] NULL,
  [NAME] [varchar](50) NOT NULL,
  [AREA] [int] NULL,
  [POP2005] [int] NULL,
  [REGION] [smallint] NULL,
  [SUBREGION] [smallint] NULL,
  [LON] [real] NULL,
  [LAT] [real] NULL,
  [GEOM] [geometry] NULL,
  CONSTRAINT [PK_Countries_NAME] PRIMARY KEY CLUSTERED ([NAME])
)
GO