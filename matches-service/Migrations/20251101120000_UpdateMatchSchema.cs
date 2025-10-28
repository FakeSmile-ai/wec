using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchesService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMatchSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Match_Period_Positive",
                table: "Matches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Match_QuarterDuration_Positive",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "QuarterDurationSeconds",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Matches");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Matches",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "FoulsAway",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FoulsHome",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quarter",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TimeRemaining",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 600);

            migrationBuilder.AddColumn<bool>(
                name: "TimerRunning",
                table: "Matches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Match_Fouls_NonNegative",
                table: "Matches",
                sql: "[FoulsHome] >= 0 AND [FoulsAway] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Match_Quarter_Range",
                table: "Matches",
                sql: "[Quarter] BETWEEN 1 AND 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Match_TimeRemaining_NonNegative",
                table: "Matches",
                sql: "[TimeRemaining] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Match_Fouls_NonNegative",
                table: "Matches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Match_Quarter_Range",
                table: "Matches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Match_TimeRemaining_NonNegative",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "FoulsAway",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "FoulsHome",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Quarter",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TimeRemaining",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TimerRunning",
                table: "Matches");

            migrationBuilder.AddColumn<int>(
                name: "Period",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "QuarterDurationSeconds",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 600);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Matches",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Match_Period_Positive",
                table: "Matches",
                sql: "[Period] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Match_QuarterDuration_Positive",
                table: "Matches",
                sql: "[QuarterDurationSeconds] > 0");
        }
    }
}
