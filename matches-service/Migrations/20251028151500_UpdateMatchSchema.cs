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
            migrationBuilder.Sql("""
IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Match_Period_Positive'
      AND parent_object_id = OBJECT_ID(N'[dbo].[Matches]')
)
BEGIN
    ALTER TABLE [Matches] DROP CONSTRAINT [CK_Match_Period_Positive];
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'Period') IS NOT NULL
    AND COL_LENGTH(N'dbo.Matches', 'Quarter') IS NULL
BEGIN
    EXEC sp_rename N'dbo.Matches.Period', N'Quarter', N'COLUMN';
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'DateMatchUtc') IS NOT NULL
    AND COL_LENGTH(N'dbo.Matches', 'DateMatch') IS NULL
BEGIN
    EXEC sp_rename N'dbo.Matches.DateMatchUtc', N'DateMatch', N'COLUMN';
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'HomeFouls') IS NOT NULL
    AND COL_LENGTH(N'dbo.Matches', 'FoulsHome') IS NULL
BEGIN
    EXEC sp_rename N'dbo.Matches.HomeFouls', N'FoulsHome', N'COLUMN';
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'AwayFouls') IS NOT NULL
    AND COL_LENGTH(N'dbo.Matches', 'FoulsAway') IS NULL
BEGIN
    EXEC sp_rename N'dbo.Matches.AwayFouls', N'FoulsAway', N'COLUMN';
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'CreatedAtUtc') IS NULL
BEGIN
    ALTER TABLE [Matches] ADD [CreatedAtUtc] datetime2 NOT NULL CONSTRAINT DF_Matches_CreatedAtUtc DEFAULT (SYSUTCDATETIME());
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'FoulsHome') IS NULL
BEGIN
    ALTER TABLE [Matches] ADD [FoulsHome] int NOT NULL CONSTRAINT DF_Matches_FoulsHome DEFAULT (0);
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'FoulsAway') IS NULL
BEGIN
    ALTER TABLE [Matches] ADD [FoulsAway] int NOT NULL CONSTRAINT DF_Matches_FoulsAway DEFAULT (0);
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'TimeRemaining') IS NULL
BEGIN
    ALTER TABLE [Matches] ADD [TimeRemaining] int NOT NULL CONSTRAINT DF_Matches_TimeRemaining DEFAULT (600);
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'QuarterDurationSeconds') IS NOT NULL
BEGIN
    UPDATE [Matches] SET [TimeRemaining] = [QuarterDurationSeconds];

    IF EXISTS (
        SELECT 1
        FROM sys.check_constraints
        WHERE name = 'CK_Match_QuarterDuration_Positive'
          AND parent_object_id = OBJECT_ID(N'[dbo].[Matches]')
    )
    BEGIN
        ALTER TABLE [Matches] DROP CONSTRAINT [CK_Match_QuarterDuration_Positive];
    END

    DECLARE @defaultConstraint sysname;
    SELECT @defaultConstraint = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Matches]')
      AND c.name = 'QuarterDurationSeconds';

    IF @defaultConstraint IS NOT NULL
    BEGIN
        DECLARE @sqlDropQuarter nvarchar(4000);
        SET @sqlDropQuarter = N'ALTER TABLE [Matches] DROP CONSTRAINT ' + QUOTENAME(@defaultConstraint);
        EXEC(@sqlDropQuarter);
    END

    ALTER TABLE [Matches] DROP COLUMN [QuarterDurationSeconds];
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'TimerRunning') IS NULL
BEGIN
    ALTER TABLE [Matches] ADD [TimerRunning] bit NOT NULL CONSTRAINT DF_Matches_TimerRunning DEFAULT (0);
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'IsPaused') IS NOT NULL
BEGIN
    UPDATE [Matches]
    SET [TimerRunning] = CASE WHEN [IsPaused] = 1 THEN 0 ELSE 1 END;

    DECLARE @pausedDefault sysname;
    SELECT @pausedDefault = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Matches]')
      AND c.name = 'IsPaused';

    IF @pausedDefault IS NOT NULL
    BEGIN
        DECLARE @sqlDropPaused nvarchar(4000);
        SET @sqlDropPaused = N'ALTER TABLE [Matches] DROP CONSTRAINT ' + QUOTENAME(@pausedDefault);
        EXEC(@sqlDropPaused);
    END

    ALTER TABLE [Matches] DROP COLUMN [IsPaused];
END
""");

            migrationBuilder.Sql("""
IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Match_Quarter_Positive'
      AND parent_object_id = OBJECT_ID(N'[dbo].[Matches]')
)
    AND COL_LENGTH(N'dbo.Matches', 'Quarter') IS NOT NULL
BEGIN
    ALTER TABLE [Matches] WITH CHECK ADD CONSTRAINT [CK_Match_Quarter_Positive] CHECK ([Quarter] >= 1);
END
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Match_Quarter_Positive'
      AND parent_object_id = OBJECT_ID(N'[dbo].[Matches]')
)
    ALTER TABLE [Matches] DROP CONSTRAINT [CK_Match_Quarter_Positive];
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'TimerRunning') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Matches', 'IsPaused') IS NULL
    BEGIN
        ALTER TABLE [Matches] ADD [IsPaused] bit NULL;
    END

    UPDATE [Matches]
    SET [IsPaused] = CASE WHEN [TimerRunning] = 1 THEN 0 ELSE 1 END;

    DECLARE @timerDefault sysname;
    SELECT @timerDefault = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Matches]')
      AND c.name = 'TimerRunning';

    IF @timerDefault IS NOT NULL
    BEGIN
        DECLARE @sqlDropTimer nvarchar(4000);
        SET @sqlDropTimer = N'ALTER TABLE [Matches] DROP CONSTRAINT ' + QUOTENAME(@timerDefault);
        EXEC(@sqlDropTimer);
    END

    ALTER TABLE [Matches] DROP COLUMN [TimerRunning];

    ALTER TABLE [Matches] ALTER COLUMN [IsPaused] bit NOT NULL;

    DECLARE @pausedDefaultDown sysname;
    SELECT @pausedDefaultDown = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Matches]')
      AND c.name = 'IsPaused';

    IF @pausedDefaultDown IS NULL
    BEGIN
        ALTER TABLE [Matches] ADD CONSTRAINT DF_Matches_IsPaused DEFAULT (0) FOR [IsPaused];
    END
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'TimeRemaining') IS NOT NULL
BEGIN
    DECLARE @timeDefault sysname;
    SELECT @timeDefault = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Matches]')
      AND c.name = 'TimeRemaining';

    IF @timeDefault IS NOT NULL
    BEGIN
        DECLARE @sqlDropTime nvarchar(4000);
        SET @sqlDropTime = N'ALTER TABLE [Matches] DROP CONSTRAINT ' + QUOTENAME(@timeDefault);
        EXEC(@sqlDropTime);
    END

    ALTER TABLE [Matches] DROP COLUMN [TimeRemaining];
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'FoulsAway') IS NOT NULL
BEGIN
    DECLARE @foulsAwayDefault sysname;
    SELECT @foulsAwayDefault = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Matches]')
      AND c.name = 'FoulsAway';

    IF @foulsAwayDefault IS NOT NULL
    BEGIN
        DECLARE @sqlDropFoulsAway nvarchar(4000);
        SET @sqlDropFoulsAway = N'ALTER TABLE [Matches] DROP CONSTRAINT ' + QUOTENAME(@foulsAwayDefault);
        EXEC(@sqlDropFoulsAway);
    END

    ALTER TABLE [Matches] DROP COLUMN [FoulsAway];
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'FoulsHome') IS NOT NULL
BEGIN
    DECLARE @foulsHomeDefault sysname;
    SELECT @foulsHomeDefault = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Matches]')
      AND c.name = 'FoulsHome';

    IF @foulsHomeDefault IS NOT NULL
    BEGIN
        DECLARE @sqlDropFoulsHome nvarchar(4000);
        SET @sqlDropFoulsHome = N'ALTER TABLE [Matches] DROP CONSTRAINT ' + QUOTENAME(@foulsHomeDefault);
        EXEC(@sqlDropFoulsHome);
    END

    ALTER TABLE [Matches] DROP COLUMN [FoulsHome];
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'CreatedAtUtc') IS NOT NULL
BEGIN
    DECLARE @createdDefault sysname;
    SELECT @createdDefault = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Matches]')
      AND c.name = 'CreatedAtUtc';

    IF @createdDefault IS NOT NULL
    BEGIN
        DECLARE @sqlDropCreated nvarchar(4000);
        SET @sqlDropCreated = N'ALTER TABLE [Matches] DROP CONSTRAINT ' + QUOTENAME(@createdDefault);
        EXEC(@sqlDropCreated);
    END

    ALTER TABLE [Matches] DROP COLUMN [CreatedAtUtc];
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'TimeRemaining') IS NULL
    AND COL_LENGTH(N'dbo.Matches', 'QuarterDurationSeconds') IS NULL
BEGIN
    ALTER TABLE [Matches] ADD [QuarterDurationSeconds] int NOT NULL CONSTRAINT DF_Matches_QuarterDurationSeconds DEFAULT (600);
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'Quarter') IS NOT NULL
    AND COL_LENGTH(N'dbo.Matches', 'Period') IS NULL
BEGIN
    EXEC sp_rename N'dbo.Matches.Quarter', N'Period', N'COLUMN';
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Matches', 'DateMatch') IS NOT NULL
    AND COL_LENGTH(N'dbo.Matches', 'DateMatchUtc') IS NULL
BEGIN
    EXEC sp_rename N'dbo.Matches.DateMatch', N'DateMatchUtc', N'COLUMN';
END
""");

            migrationBuilder.Sql("""
IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Match_Period_Positive'
      AND parent_object_id = OBJECT_ID(N'[dbo].[Matches]')
)
    AND COL_LENGTH(N'dbo.Matches', 'Period') IS NOT NULL
BEGIN
    ALTER TABLE [Matches] WITH CHECK ADD CONSTRAINT [CK_Match_Period_Positive] CHECK ([Period] > 0);
END
""");
        }
    }
}
