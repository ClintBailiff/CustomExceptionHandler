
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ExceptionLog](
	[Id] [nvarchar](450) NOT NULL,
	[ExDate] [datetime] NOT NULL,
	[Message] [nvarchar](510) NOT NULL,
	[FileName] [nvarchar](510) NOT NULL,
	[Method] [nvarchar](255) NOT NULL,
	[LineNum] [int] NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
	[ClientIp] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_ExceptionLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


