# Documentação Arquitetural — CheerBR

> Documentação técnica completa do sistema Cheer PR (mapa de equipes de
> cheerleading do Paraná). Complementar ao `README.md` (setup) e
> `CHANGELOG.md` (histórico).

---

## 1. Contexto e Regras de Negócio

### Propósito

Sistema para **cadastrar, ranquear e visualizar** equipes de cheerleading do
Paraná (PR). Cada equipe possui dados cadastrais, um logo, e um histórico de
**resultados em campeonatos**. O score de cada equipe é calculado
automaticamente a partir dos resultados por um algoritmo de pontuação próprio
chamado **ProCheer**, aplicando pesos de importância do campeonato, nível da
equipe, categoria disputada, e um **decaimento temporal** anual.

### Módulos funcionais

| Módulo          | Descrição                                                                |
| --------------- | ------------------------------------------------------------------------ |
| **Equipes**     | CRUD completo de equipes; busca/filtro por nome/cidade/categoria/nível. |
| **Resultados**  | CRUD de resultados (campeonatos) por equipe; recalcula o score ao mutar. |
| **Ranking**     | Endpoint/API e página que ordena equipes por `Score` decrescente.        |
| **Dashboard**   | Gráficos agregados (por status, categoria, cidade, nível) + score médio. |
| **Campeonatos** | CRUD da entidade `Championship` (usada como dropdown ao lançar resultado). |
| **Upload**      | Upload de logo por equipe (disco local + URL persistida em `Team.LogoUrl`). |

### Regras de pontuação (ProCheer)

Implementadas em `backend/Cheer.Domain/Entities/Team.cs::CalculateScore` e em
constantes de `backend/Cheer.Domain/Constants/ScoreConstants.cs`:

```
score_total = Σ resultados(pontos_base * peso_importancia * peso_nivel * peso_categoria * decay_ano)

pontos_base(colocacao):  1º=100, 2º=70, 3º=50, 4º=30, 5º=20, demais=10
peso_importancia:        Internacional=3.0, Nacional=2.5, Estadual=2.0,
                          Regional=1.7, Municipal=1.5
peso_nivel:              1→1.1, 2→1.2, 3→1.3, 4→1.4, 5→1.5, 6→1.6
peso_categoria:          Team Cheer=1.5, Grupo=1.2, Duplas=1.1, Skills=0.9
decay_ano:               max(0, 1 - (ano_atual - ano_resultado) * 0.1)

Score team é recalculado a cada mutação de resultado (add/update/delete).
```

### Lacunas funcionais / segurança

- **Sem autenticação**: a API é pública — qualquer cliente pode listar, criar,
  editar e excluir equipes, resultados e campeonatos. JWT + BCrypt + RBAC não
  foram implementados neste release (scope separado).
- **Sem autorização granular**: qualquer cargo (quando auth for adicionada)
  deve ser modelado.
- **Logos em disco local**: persistidos em `wwwroot/uploads` (container/Docker
  via volume); não há estratégia de cleanup de logos órfãos pós-update/delete.

---

## 2. Arquitetura Técnica

### Tech stack

| Camada          | Tecnologia                                                                              |
| --------------- | --------------------------------------------------------------------------------------- |
| **Frontend**    | React 19, TypeScript, Vite 8, TanStack Router/Query/Start (SSR), Tailwind v4, shadcn/ui, Recharts, Zod |
| **Backend**     | .NET 10 ASP.NET Core Web API, Swashbuckle (Swagger)                                      |
| **ORM**         | Entity Framework Core 10 + provider `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.2     |
| **Banco**       | PostgreSQL 16 (auto-hospedado via Docker; anteriormente, Render.com managed Postgres)   |
| **Infra local** | Docker Compose (Postgres 16-alpine + backend .NET em container, ambos volumes nomeados) |
| **Runtime**     | Kestrel (porta 10000), usuário `app` não-root no container                               |

### Componentes e fluxo de dados

```
┌────────────────────────────────────────────────────────────────────────┐
│                            FRONTEND (Vite/SSR)                          │
│                                                                        │
│   Routes (TanStack Router)   Components        Hooks (TanStack Query)  │
│        │                          │                 │                  │
│        └───────────┬──────────────┴─────────────────┘                  │
│                    │                                                  │
│              src/lib/api.ts                                           │
│              (fetch → VITE_API_URL)                                   │
└────────────────────┬───────────────────────────────────────────────────┘
                     │  HTTPS/HTTP (JSON, multipart/form-data p/ logo)
                     ▼
┌────────────────────────────────────────────────────────────────────────┐
│                       BACKEND (.NET 10 Web API)                         │
│                                                                        │
│   Cheer.Api            Cheer.Application          Cheer.Infrastructure │
│   Program.cs           ITeamService/TeamService   AppDbContext (EF)    │
│   Controllers          IChampionshipService       Repositories         │
│   [DataAnnotations]    Mappings/DTOs              Migrations          │
│   (validação ex)                                                         │
└────────────────────┬───────────────────────────────────────────────────┘
                     │  Npgsql (EF Core)
                     ▼
┌────────────────────────────────────────────────────────────────────────┐
│                     PostgreSQL 16 (Docker, volume)                      │
│                                                                        │
│   Teams ──< CompetitionResults            Championships               │
└────────────────────────────────────────────────────────────────────────┘
```

**Fluxos principais:**

1. **Leitura (lista/dashboard/ranking)** — Frontend → `api.getTeams()` →
   `GET /api/teams` → `TeamsController` → `TeamService` →
   `TeamRepository` (EF Core / Npgsql) → Postgres. O score já vem calculado.
2. **Escrita de equipe** — Frontend → `useTeams().addTeam/updateTeam` →
   `POST/PUT /api/teams` → validação DataAnnotations → `TeamService` →
   `TeamRepository` → DB.
3. **Escrita de resultado** — Frontend → `useTeamResults().addResult/...` →
   `POST/.../results` → validação → `TeamService` recalcula
   `Team.CalculateScore(currentYear)` antes de salvar → DB.
4. **Upload de logo** — Frontend → `api.uploadTeamLogo` (FormData) →
   `TeamsController.UploadLogo` → valida Content-Type/extensão/size →
   grava em `UPLOADS_PATH` (`/var/lib/cheerbr/uploads`) → gera URL e atualiza
   `Team.LogoUrl` via `TeamService.UpdateTeamAsync`.

### Camadas do backend (.NET)

| Projeto                  | Responsabilidade                                                   |
| ------------------------ | ------------------------------------------------------------------ |
| `Cheer.Domain`           | Entidades (`Team`, `CompetitionResult`, `Championship`), `ScoreConstants`, interfaces (`ITeamRepository`, `IChampionshipRepository`). Zero dependências externas. |
| `Cheer.Application`      | DTOs (com DataAnnotations), serviços (`TeamService`, `ChampionshipService`), mappings estáticos (`TeamMappings`). Contratos `ITeamService`/`IChampionshipService`. Zero NuGet. |
| `Cheer.Infrastructure`   | EF Core: `AppDbContext`, `AppDbContextFactory` (design-time), repositórios (`TeamRepository`, `ChampionshipRepository`), migrations. Pacotes: `EFCore.Design`, `Npgsql.EFCore.PostgreSQL`. |
| `Cheer.Api`              | Composição (`Program.cs`): controllers, Swagger, CORS configurável, `AddDbContext` com retry-on-failure, `UseStaticFiles` em `/uploads`, healthcheck. Pacotes: `OpenApi`, `EFCore.Design`, `Npgsql.EFCore.PostgreSQL`, `Swashbuckle`. |

**Injeção de dependência** (`Program.cs`):
- `AppDbContext` → Scoped
- `ITeamRepository`/`TeamRepository` → Scoped
- `IChampionshipRepository`/`ChampionshipRepository` → Scoped
- `ITeamService`/`TeamService` → Scoped
- `IChampionshipService`/`ChampionshipService` → Scoped

### Configuração

- **`Program.cs`** lê:
  - `DATABASE_URL` (env, prioritária) ou `ConnectionStrings:DefaultConnection`
    (appsettings) — necessário para subir.
  - `CORS_ALLOWED_ORIGINS` (env, CSV) ou serção `Cors:AllowedOrigins`
    (appsettings) — vazio = `AllowAnyOrigin`.
  - `UPLOADS_PATH` (env) — fallback `wwwroot/uploads`.
- **`AppDbContextFactory`** (design-time): mesma prioridade — `DATABASE_URL` →
  env `ConnectionStrings__DefaultConnection` → fallback localhost.
- **Dockerfile**: multi-stage (sdk:10.0 → aspnet:10.0), roda como `USER app`
  (não-root), `chown` em `/var/lib/cheerbr/uploads`.
- **docker-compose.yml**: `postgres:16-alpine` (volume `chebr_pgdata`) +
  `api` (builda o backend, conecta ao `postgres` via `DATABASE_URL`,
  volume `cheerbr_uploads`).

---

## 3. Mapeamento de Rotas e Entidades

### Modelo de dados (ERD)

```
┌─────────────────────┐
│      Teams          │
├─────────────────────┤        ┌──────────────────────────────┐
│ Id        text PK   │   1   │    CompetitionResults        │
│ Nome      text      │───────┤ Id              text PK      │
│ Programa  text?     │   N   │ TeamId          text  FK      │
│ Nivel     int?      │       │ Ano             int          │
│ Cidade    text      │       │ NomeCampeonato  text         │
│ Estado    text      │       │ Importancia     text         │
│ Categoria text      │       │ Nivel           int          │
│ Instagram text?     │       │ TipoCategoria   text         │
│ Facebook  text?     │       │ Colocacao       int          │
│ Coach     text?     │       └──────────────────────────────┘
│ Fundacao  text?     │
│ Status    text      │
│ LogoUrl   text?     │
│ Score     int        │
└─────────────────────┘

┌─────────────────────┐
│   Championships     │
├─────────────────────┤
│ Id   text PK        │
│ Nome text           │
└─────────────────────┘
```

- **PKs**: `text` (Guid string gerado em `Team.Id` e `CompetitionResult.Id`
  via `Guid.NewGuid().ToString()` no construtor da entidade).
- **FK** `CompetitionResults.TeamId → Teams.Id`, **ON DELETE CASCADE**.
- **Índice**: `IX_CompetitionResults_TeamId` (criado pela migration).
- **Sem seed** (`OnModelCreating` não chama `HasData`).

### Endpoints REST

Base URL em produção: backend liga em `http://+:10000`. Em dev (fora Docker):
`http://localhost:10000`. CORS deve incluir a origem do frontend.

| Método | Rota                                | Controller / Action           | DTO entrada                     | Saída                              |
| ------ | ----------------------------------- | ----------------------------- | ------------------------------- | ---------------------------------- |
| GET    | `/`                                 | `Program.cs` inline           | —                               | `{ status, environment, time }`    |
| GET    | `/api/teams[?categoria&cidade&q&nivel]` | `TeamsController.GetTeams`   | (query)                        | `TeamDto[]`                         |
| GET    | `/api/teams/{id}`                   | `TeamsController.GetTeam`     | —                               | `TeamDto` / 404                     |
| POST   | `/api/teams`                        | `TeamsController.CreateTeam`  | `CreateTeamDto`                  | `TeamDto` (201 + Location)         |
| PUT    | `/api/teams/{id}`                   | `TeamsController.UpdateTeam`  | `UpdateTeamDto` (= Create+Id)    | 204 / 400 (id mismatch) / 404      |
| DELETE | `/api/teams/{id}`                   | `TeamsController.DeleteTeam`   | —                               | 204                                 |
| POST   | `/api/teams/{id}/results`           | `TeamsController.AddResult`    | `CreateCompetitionResultDto`     | `CompetitionResultDto` / 404       |
| PUT    | `/api/teams/{id}/results/{resultId}` | `TeamsController.UpdateResult` | `UpdateCompetitionResultDto`    | `CompetitionResultDto` / 404       |
| DELETE | `/api/teams/{id}/results/{resultId}` | `TeamsController.DeleteResult` | —                               | 204 / 404                           |
| POST   | `/api/teams/{id}/logo`              | `TeamsController.UploadLogo`   | `multipart: file=<binary>`       | `{ LogoUrl }` / 400 (formato/size) |
| GET    | `/api/ranking[?categoria]`          | `RankingController.GetRanking` | (query)                        | `TeamDto[]` (ordenado por score desc) |
| GET    | `/api/stats/overview`               | `StatsController.GetOverview`  | —                               | `StatsOverviewDto`                  |
| GET    | `/api/championships`                | `ChampionshipsController.GetAll` | —                              | `ChampionshipDto[]`                 |
| POST   | `/api/championships`                | `ChampionshipsController.Create` | `CreateChampionshipDto`         | `ChampionshipDto` (201)            |
| PUT    | `/api/championships/{id}`           | `ChampionshipsController.Update` | `CreateChampionshipDto`        | 204 / 404                           |
| DELETE | `/api/championships/{id}`           | `ChampionshipsController.Delete` | —                              | 204                                 |

### Rotas do frontend (TanStack Router)

| Rota             | Página             | Arquivo                       |
| ---------------- | ------------------ | ----------------------------- |
| `/`              | Home               | `src/routes/index.tsx`        |
| `/equipes`       | Lista de equipes    | `src/routes/equipes.index.tsx` |
| `/equipes/$id`   | Detalhe da equipe   | `src/routes/equipes_.$id.tsx`  |
| `/ranking`       | Ranking            | `src/routes/ranking.tsx`       |
| `/dashboard`     | Dashboard          | `src/routes/dashboard.tsx`    |
| `/campeonatos`   | Campeonatos        | `src/routes/campeonatos.tsx`  |

### Clientes JS da API (frontend)

- **`src/lib/api.ts`** — wrapper HTTP centralizado (`request<T>`).
  URL base via `import.meta.env.VITE_API_URL`.
- **`src/lib/teams-store.ts`** — hooks TanStack Query: `useTeams`,
  `useTeamResults`, `useUploadLogo`, `useInvalidateAll`.
- **`src/lib/championships-store.ts`** — `useChampionships` com fallback para
  `localStorage` quando a API falha (atenção: edits não sincronizam com o DB se
  o backend estiver down).

### Diagrama de estado (frontend)

- Cache central: `@tanstack/react-query`. Keys principais: `["teams"]`,
  `["ranking"]`, `["stats"]` (todas invalidadas pelo `useInvalidateAll` em
  mutações de equipe ou resultado).

---

## 4. Migração de banco (resumo executivo)

> Detalhes no `README.md` e `CHANGELOG.md`.

1. Antes: PostgreSQL managed no **Render.com** (`oregon-postgres.render.com`).
2. Agora: PostgreSQL 16 **auto-hospedado** via `docker compose up -d postgres`.
3. Caminhos para popular:
   - **Banco novo**: aplicar `backend/Cheer.Infrastructure/Migrations/baseline_clean.sql` (squash das 9 migrations, cria as 3 tabelas + índices + `__EFMigrationsHistory` já preenchida).
   - **Migrar dados do Render**: `pg_dump --no-owner --no-privileges` + `psql < dump.sql` no local.
   - **EF migrations**: `DATABASE_URL=… dotnet ef database update` (replay incremental das 9).
4. Strings de conexão: lidas de `DATABASE_URL` (env) em runtime e em design-time (`AppDbContextFactory`); `appsettings*.json` contêm apenas fallback `localhost`.
5. Segredos: `.env` (gitignored) + `.env.example` (template). Senha antiga do Render **removida** do repo e deve ser **rotacionada** no painel Render.

---

## 5. Scripts e comandos úteis

```bash
# Backend
dotnet build backend/Cheer.slnx -c Release
DATABASE_URL=… dotnet ef migrations list --project backend/Cheer.Infrastructure --startup-project backend/Cheer.Api
DATABASE_URL=… dotnet ef database update --project backend/Cheer.Infrastructure --startup-project backend/Cheer.Api
DATABASE_URL=… dotnet ef migrations script --idempotent -o snap.sql --project backend/Cheer.Infrastructure --startup-project backend/Cheer.Api

# Frontend
npm install
npm run dev
npm run build
npm run lint

# Docker (banco local)
docker compose up -d postgres
docker compose exec postgres psql -U cheerbr -d cheerbr -c "\dt"
docker compose down              # para serviços (mantém volumes)
docker compose down -v           # DESTRÓI volumes (ATENÇÃO: apaga dados)
```
