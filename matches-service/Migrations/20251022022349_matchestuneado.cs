using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchesService.Migrations
{
    /// <inheritdoc />
    public partial class matchestuneado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamWins_MatchId",
                table: "TeamWins");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateRegistered",
                table: "TeamWins",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "ScoreEvents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateRegister",
                table: "ScoreEvents",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Matches",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "QuarterDurationSeconds",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 600,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Matches",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Fouls",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateRegister",
                table: "Fouls",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_TeamWins_MatchId",
                table: "TeamWins",
                column: "MatchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScoreEvents_MatchId_TeamId",
                table: "ScoreEvents",
                columns: new[] { "MatchId", "TeamId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ScoreEvent_Points_Range",
                table: "ScoreEvents",
                sql: "[Points] BETWEEN -3 AND 3");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_DateMatch",
                table: "Matches",
                column: "DateMatch");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_Status_DateMatch",
                table: "Matches",
                columns: new[] { "Status", "DateMatch" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Match_HomeAway_Distinct",
                table: "Matches",
                sql: "[HomeTeamId] <> [AwayTeamId]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Match_Period_Positive",
                table: "Matches",
                sql: "[Period] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Match_QuarterDuration_Positive",
                table: "Matches",
                sql: "[QuarterDurationSeconds] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Fouls_MatchId_TeamId",
                table: "Fouls",
                columns: new[] { "MatchId", "TeamId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamWins_MatchId",
                table: "TeamWins");

            migrationBuilder.DropIndex(
                name: "IX_ScoreEvents_MatchId_TeamId",
                table: "ScoreEvents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ScoreEvent_Points_Range",
                table: "ScoreEvents");

            migrationBuilder.DropIndex(
                name: "IX_Matches_DateMatch",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_Status_DateMatch",
                table: "Matches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Match_HomeAway_Distinct",
                table: "Matches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Match_Period_Positive",
                table: "Matches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Match_QuarterDuration_Positive",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Fouls_MatchId_TeamId",
                table: "Fouls");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Matches");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateRegistered",
                table: "TeamWins",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "ScoreEvents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateRegister",
                table: "ScoreEvents",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<int>(
                name: "QuarterDurationSeconds",
                table: "Matches",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 600);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Fouls",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateRegister",
                table: "Fouls",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateIndex(
                name: "IX_TeamWins_MatchId",
                table: "TeamWins",
                column: "MatchId");
        }
    }
}
