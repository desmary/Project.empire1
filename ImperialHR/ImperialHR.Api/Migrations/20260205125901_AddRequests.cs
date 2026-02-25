using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImperialHR.Api.Migrations
{
    public partial class AddRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Requests]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Requests] (
        [Id] INT NOT NULL IDENTITY(1,1),
        [EmployeeId] INT NOT NULL,
        [ApproverId] INT NOT NULL,
        [Type] INT NOT NULL,
        [Status] INT NOT NULL,
        [From] DATETIME2 NOT NULL,
        [To] DATETIME2 NOT NULL,
        [Comment] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_Requests] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Requests_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Requests_Employees_ApproverId] FOREIGN KEY ([ApproverId]) REFERENCES [dbo].[Employees]([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_Requests_EmployeeId] ON [dbo].[Requests]([EmployeeId]);
    CREATE INDEX [IX_Requests_ApproverId] ON [dbo].[Requests]([ApproverId]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Requests]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Requests];
END
");
        }
    }
}