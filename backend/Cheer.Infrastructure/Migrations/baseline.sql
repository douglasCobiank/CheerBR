CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617225333_InitialCreate') THEN
    CREATE TABLE "Teams" (
        "Id" text NOT NULL,
        "Nome" text NOT NULL,
        "Programa" text,
        "Nivel" integer,
        "Cidade" text NOT NULL,
        "Estado" text NOT NULL,
        "Categoria" text NOT NULL,
        "Instagram" text,
        "Facebook" text,
        "Coach" text,
        "Fundacao" text,
        "Status" text NOT NULL,
        "Score" integer NOT NULL,
        CONSTRAINT "PK_Teams" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617225333_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260617225333_InitialCreate', '10.0.9');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617230310_InitialCreate2') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260617230310_InitialCreate2', '10.0.9');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260617231136_InitialCreate3') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260617231136_InitialCreate3', '10.0.9');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260619022240_TempMigration') THEN
    CREATE TABLE "CompetitionResults" (
        "Id" text NOT NULL,
        "TeamId" text NOT NULL,
        "Ano" integer NOT NULL,
        "NomeCampeonato" text NOT NULL,
        "Importancia" text NOT NULL,
        "Nivel" integer NOT NULL,
        "Participantes" integer NOT NULL,
        "TipoCategoria" text NOT NULL,
        "Colocacao" integer NOT NULL,
        CONSTRAINT "PK_CompetitionResults" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CompetitionResults_Teams_TeamId" FOREIGN KEY ("TeamId") REFERENCES "Teams" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260619022240_TempMigration') THEN
    CREATE INDEX "IX_CompetitionResults_TeamId" ON "CompetitionResults" ("TeamId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260619022240_TempMigration') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260619022240_TempMigration', '10.0.9');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260620200619_AddCompetitionResults') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260620200619_AddCompetitionResults', '10.0.9');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260620203931_AddLogoUrlAndResults') THEN
    ALTER TABLE "Teams" ADD "LogoUrl" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260620203931_AddLogoUrlAndResults') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260620203931_AddLogoUrlAndResults', '10.0.9');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260620205857_RemoveParticipantes') THEN
    ALTER TABLE "CompetitionResults" DROP COLUMN "Participantes";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260620205857_RemoveParticipantes') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260620205857_RemoveParticipantes', '10.0.9');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621183346_AddChampionships') THEN
    CREATE TABLE "Championships" (
        "Id" text NOT NULL,
        "Nome" text NOT NULL,
        CONSTRAINT "PK_Championships" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260621183346_AddChampionships') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260621183346_AddChampionships', '10.0.9');
    END IF;
END $EF$;
COMMIT;

