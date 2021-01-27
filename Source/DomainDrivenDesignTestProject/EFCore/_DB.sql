CREATE TABLE [dbo].[IncomingFileTransaction](
	[Id] [uniqueidentifier] NOT NULL,
	[ConcurrencyVersion] [timestamp] NOT NULL,
	[CreatedUtc] [datetime2](7) NOT NULL,
	[ModifiedUtc] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_IncomingFileTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[IncomingFileTransactionEvent](
	[Id] [uniqueidentifier] NOT NULL,
	[IncomingFileTransactionId] [uniqueidentifier] NOT NULL,
	[Type] [int] NOT NULL,
 CONSTRAINT [PK_IncomingFileTransactionEvent] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[IncomingFileTransactionInfo](
	[Id] [uniqueidentifier] NOT NULL,
	[IncomingFileTransactionId] [uniqueidentifier] NOT NULL,
	[Mpxn] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK_IncomingFileTransactionInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
