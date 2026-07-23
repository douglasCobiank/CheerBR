-- ===================================================================
-- CheerBR — Schema baseline (PostgreSQL)
-- Cria o schema final consolidado (squash das 9 migrations EF Core)
-- a partir de um banco PostgreSQL totalmente novo.
--
-- Uso:
--   psql "postgres://cheerbr:SENHA@localhost:5432/cheerbr" -f baseline_clean.sql
--
-- Compativel com o estado esperado pelo EF Core (AppDbContextModelSnapshot):
-- apos aplicar este script, registre todas as migrations no historico
-- para que `dotnet ef database update` seja no-op no futuro.
-- ===================================================================

-- Historico de migrations do EF Core
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Tabela: Teams
CREATE TABLE IF NOT EXISTS "Teams" (
    "Id"           text NOT NULL,
    "Nome"         text NOT NULL,
    "Programa"     text,
    "Nivel"        integer,
    "Cidade"       text NOT NULL,
    "Estado"       text NOT NULL,
    "Categoria"    text NOT NULL,
    "Instagram"    text,
    "Facebook"     text,
    "Coach"        text,
    "Fundacao"     text,
    "Status"       text NOT NULL,
    "LogoUrl"      text,
    "Score"        integer NOT NULL DEFAULT 0,
    CONSTRAINT "PK_Teams" PRIMARY KEY ("Id")
);

-- Tabela: CompetitionResults
CREATE TABLE IF NOT EXISTS "CompetitionResults" (
    "Id"             text NOT NULL,
    "TeamId"         text NOT NULL,
    "Ano"            integer NOT NULL,
    "NomeCampeonato" text NOT NULL,
    "Importancia"    text NOT NULL,
    "Nivel"          integer NOT NULL,
    "TipoCategoria"  text NOT NULL,
    "Colocacao"      integer NOT NULL,
    CONSTRAINT "PK_CompetitionResults" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CompetitionResults_Teams_TeamId"
        FOREIGN KEY ("TeamId") REFERENCES "Teams" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_CompetitionResults_TeamId"
    ON "CompetitionResults" ("TeamId");

-- Tabela: Championships
CREATE TABLE IF NOT EXISTS "Championships" (
    "Id"   text NOT NULL,
    "Nome" text NOT NULL,
    CONSTRAINT "PK_Championships" PRIMARY KEY ("Id")
);

-- Marca todas as migrations EF como ja aplicadas, evitando replay:
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES
    ('20260617225333_InitialCreate',          '10.0.9'),
    ('20260617230310_InitialCreate2',          '10.0.9'),
    ('20260617231136_InitialCreate3',          '10.0.9'),
    ('20260619022240_TempMigration',          '10.0.9'),
    ('20260620200619_AddCompetitionResults',  '10.0.9'),
    ('20260620203931_AddLogoUrlAndResults',   '10.0.9'),
    ('20260620205857_RemoveParticipantes',    '10.0.9'),
    ('20260621183346_AddChampionships',       '10.0.9')
ON CONFLICT ("MigrationId") DO NOTHING;
