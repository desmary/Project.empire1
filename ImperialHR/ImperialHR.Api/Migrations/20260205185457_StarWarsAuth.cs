
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImperialHR.Api.Migrations
{
   
    public partial class StarWarsAuth : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Робимо безпечним: якщо колонок не існує — просто пропускаємо
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Requests]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.Requests', 'DecidedAt') IS NOT NULL
        ALTER TABLE [dbo].[Requests] DROP COLUMN [DecidedAt];

    IF COL_LENGTH('dbo.Requests', 'DecisionComment') IS NOT NULL
        ALTER TABLE [dbo].[Requests] DROP COLUMN [DecisionComment];

    IF COL_LENGTH('dbo.Requests', 'StartDate') IS NOT NULL
        ALTER TABLE [dbo].[Requests] DROP COLUMN [StartDate];

    IF COL_LENGTH('dbo.Requests', 'EndDate') IS NOT NULL
        ALTER TABLE [dbo].[Requests] DROP COLUMN [EndDate];
END
");
        }
     
       protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Нічого не відкатуємо (для навчального проєкту ок),
            // бо ми прибираємо старі колонки, які вже не використовуються.
        }
    }
}
