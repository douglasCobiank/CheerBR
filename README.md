# Cheer PR — Mapa das Equipes de Cheerleading do Paraná

Plataforma full-stack para cadastro, ranking e dashboard de equipes de
cheerleading do Paraná, com sistema de pontuação estilo **ProCheer**.

---

## Visão geral

- **Frontend**: React 19 + TanStack (Router/Query/Start) + Tailwind v4 +
  shadcn/ui + Recharts. SPA com SSR.
- **Backend**: .NET 10 Web API em camadas (Domain / Application /
  Infrastructure / Api). Swagger UI exposto.
- **Banco de dados**: **PostgreSQL 16** — auto-hospedado via `docker compose`.
  Acesso via **EF Core + Npgsql**.

> ℹ️ Veja `DOCUMENTATION.md` para a documentação técnica completa
> (arquitetura, ERD, endpoints) e `CHANGELOG.md` para o histórico de mudanças.

---

## Arquitetura

```
cheerleading-app/
├── src/                       # Frontend (React + TanStack + Tailwind)
│   ├── components/            # Componentes reutilizáveis (+ ui/ shadcn)
│   ├── data/                 # Seed data (89 equipes — JSON)
│   ├── hooks/                # Custom hooks
│   ├── lib/                  # API client, store hooks, utils, constants
│   └── routes/               # Páginas (TanStack Router file-based)
├── backend/                  # Backend (.NET 10 Web API)
│   ├── Cheer.Domain/         # Entidades, constantes, interfaces
│   ├── Cheer.Application/    # DTOs, serviços (lógica de negócio), mappings
│   ├── Cheer.Infrastructure/# EF Core: AppDbContext, repositórios, migrations
│   ├── Cheer.Api/            # Controllers REST, Program.cs, Swagger
│   ├── Dockerfile            # Build multi-stage .NET 10 (não-root)
│   └── Cheer.slnx
├── docker-compose.yml        # PostgreSQL 16 + serviço de API
├── .env.example              # Template de variáveis de ambiente
└── ...
```

---

## Pré-requisitos

- **Docker** 24+ e **Docker Compose** v2 (para subir o Postgres local)
- **.NET SDK 10.0** (para rodar/debugar o backend fora do Docker)
- **Node.js 20+** e **npm** (para o frontend)
- **psql** (opcional — apenas se for importar dados do Render com `pg_dump`)

---

## Subir o PostgreSQL local (Docker)

1. **Configure o ambiente**:

   ```bash
   cp .env.example .env
   # edite .env e altere POSTGRES_PASSWORD para uma senha forte
   ```

2. **Suba o Postgres**:

   ```bash
   docker compose up -d postgres
   ```

   O Postgres 16 ficará disponível em `localhost:5432`, database `cheerbr`
   (ou o que você configurou em `.env`).

3. **Aplique o schema**. Você tem **duas opções**:

   - **DB nova/vazia** — use o schema consolidado (recomendado):

     ```bash
     docker compose exec postgres \
       psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" \
       -f /dev/stdin < backend/Cheer.Infrastructure/Migrations/baseline_clean.sql
     ```

     Ou, pelo `dotnet ef` (na pasta do backend):

     ```bash
     cd backend
     DATABASE_URL="Host=localhost;Port=5432;Database=cheerbr;Username=cheerbr;Password=SENHA;Include Error Detail=true" \
       dotnet ef database update \
         --project Cheer.Infrastructure --startup-project Cheer.Api
     ```

   - **Migrando dados do Render** (dump → restore):

     ```bash
     # 1. Dump do banco Render (executado contra o host antigo):
     pg_dump --no-owner --no-privileges \
       --host=dpg-d8ro3ie7r5hc73ej03d0-a.oregon-postgres.render.com \
       --username=cheer_br_ranking_user \
       --dbname=cheer_br_ranking \
       --file=render_dump.sql

     # 2. Restore no Postgres local:
     docker compose exec -T postgres \
       psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" < render_dump.sql
     ```

     > ⚠️ **Rotacione a senha do Render imediatamente** em
     > https://dashboard.render.com — a credential antiga estava hardcoded no
     > repositório (commitada em texto puro) e foi removida neste release.

4. **Valide**:

   ```bash
   docker compose exec postgres \
     psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" \
     -c "\dt"            # deve listar Teams, CompetitionResults, Championships, __EFMigrationsHistory
     -c "SELECT COUNT(*) FROM \"Teams\";"
   ```

---

## Rodando o projeto

### Backend (.NET)

```bash
cd backend
export DATABASE_URL="Host=localhost;Port=5432;Database=cheerbr;Username=cheerbr;Password=SENHA;Include Error Detail=true"
export UPLOADS_PATH="$PWD/Cheer.Api/wwwroot/uploads"
export CORS_ALLOWED_ORIGINS="http://localhost:5173"
dotnet run --project Cheer.Api
```

API em `http://localhost:10000` (Swagger em `/swagger`).

> Ou rode o backend em container junto do Postgres:
>
> ```bash
> docker compose up -d --build
> ```

### Frontend

```bash
cp .env.example .env           # se ainda não fez
# edite .env: VITE_API_URL=http://localhost:10000/api

npm install
npm run dev                    # http://localhost:5173
```

Scripts disponíveis:

| Comando           | Descrição         |
| ----------------- | ----------------- |
| `npm run dev`     | Dev server (HMR)  |
| `npm run build`   | Build de produção |
| `npm run preview` | Preview do build  |
| `npm run lint`    | ESLint + Prettier |
| `npm run format`  | Prettier --write  |

---

## Deploy em produção (resumo)

1. **PostgreSQL**: provisione uma instância (Docker host, VM, ou managed
   RDS/Aiven/Neon). Defina `DATABASE_URL` apontando para ela.
2. **Backend**: build via `docker build -f backend/Dockerfile backend/`. Rode o
   container expondo `10000` com env vars `DATABASE_URL`, `UPLOADS_PATH`
   (volume persistente), `CORS_ALLOWED_ORIGINS`. **O container já roda como
   usuário `app` não-root** (`USER app` no Dockerfile).
3. **Migração de schema** (no novo Postgres): `baseline_clean.sql` (DB nova) ou
   `pg_restore` do dump.
4. **Frontend**: `npm run build` e sirva os estáticos (ou deploy no
   Render/Vercel/Netlify). Defina `VITE_API_URL` apontando para o backend.
5. **Logos**: monte um volume persistente em `UPLOADS_PATH`. Em produção
   multi-instância, considere migrar para object storage (S3/R2/Azure Blob).

---

## Endpoints da API

| Método | Rota                            | Descrição                    |
| ------ | ------------------------------- | ---------------------------- |
| GET    | `/api/teams`                    | Lista equipes (filtros `q`)  |
| GET    | `/api/teams/{id}`               | Detalhe da equipe            |
| POST   | `/api/teams`                    | Criar equipe                 |
| PUT    | `/api/teams/{id}`               | Atualizar equipe             |
| DELETE | `/api/teams/{id}`               | Remover equipe               |
| POST   | `/api/teams/{id}/results`       | Adicionar resultado          |
| PUT    | `/api/teams/{id}/results/{rid}` | Editar resultado             |
| DELETE | `/api/teams/{id}/results/{rid}` | Remover resultado            |
| POST   | `/api/teams/{id}/logo`          | Upload de logo (≤ 5 MB)      |
| GET    | `/api/ranking`                  | Ranking (filtro `categoria`) |
| GET    | `/api/stats/overview`           | Estatísticas do dashboard    |
| GET    | `/api/championships`            | Lista campeonatos            |
| POST   | `/api/championships`            | Criar campeonato             |
| PUT    | `/api/championships/{id}`       | Atualizar campeonato         |
| DELETE | `/api/championships/{id}`       | Remover campeonato           |
| GET    | `/`                             | Healthcheck                  |

---

## Entidades

- **Team**: `Id`, `Nome`, `Programa`, `Nivel`, `Cidade`, `Estado`, `Categoria`,
  `Instagram`, `Facebook`, `Coach`, `Fundacao`, `Status`, `LogoUrl`, `Score`,
  `Results[]` (1-N).
- **CompetitionResult**: `Id`, `TeamId`, `Ano`, `NomeCampeonato`,
  `Importancia`, `Nivel`, `TipoCategoria`, `Colocacao`.
- **Championship**: `Id`, `Nome` (entidade de referência p/ dropdowns).

### Sistema de pontuação (ProCheer)

```
pontos_base = f(colocacao)
  # 1º=100, 2º=70, 3º=50, 4º=30, 5º=20, demais=10

pesos = importancia * nivel * tipo_categoria
  # Importancia: Internacional=3.0, Nacional=2.5, Estadual=2.0,
  #              Regional=1.7, Municipal=1.5
  # Nivel: 1=1.1, 2=1.2, 3=1.3, 4=1.4, 5=1.5, 6=1.6
  # Tipo: Team Cheer=1.5, Grupo=1.2, Duplas=1.1, Skills=0.9

decay = max(0, 1 - (ano_atual - ano_resultado) * 0.1)

score_total = Σ(pontos_base * pesos * decay)
```

Veja `backend/Cheer.Domain/Constants/ScoreConstants.cs` e
`backend/Cheer.Domain/Entities/Team.cs::CalculateScore`.

---

## Variáveis de ambiente

Veja `.env.example` para a lista completa. Resumo:

| Variável               | Onde lê           | Default                      |
| ---------------------- | ----------------- | ---------------------------- |
| `POSTGRES_USER`        | docker-compose    | `cheerbr`                    |
| `POSTGRES_PASSWORD`    | docker-compose    | `cheerbr_password_troque_me` |
| `POSTGRES_DB`          | docker-compose    | `cheerbr`                    |
| `POSTGRES_PORT`        | docker-compose    | `5432`                       |
| `DATABASE_URL`         | backend (Program) | — (obrigatório em produção)  |
| `UPLOADS_PATH`         | backend           | `wwwroot/uploads`            |
| `CORS_ALLOWED_ORIGINS` | backend           | vazio → `AllowAnyOrigin`     |
| `VITE_API_URL`         | frontend          | `http://localhost:10000/api` |

---

## Seed data

89 equipes reais de cheerleading do PR estão em
`src/data/teams.seed.json` (frontend-only). O **banco PostgreSQL** não possui
seed — para popular, importe um dump ou insira manualmente.

---

## Roadmap / Limitações conhecidas

- **Sem autenticação**: a API é pública (sem JWT/Identity/RBAC). Migrar para
  autenticação nativa é scope separado — veja `DOCUMENTATION.md`.
- **Logos em disco local**: persistem via volume Docker, mas não há GC de
  logos órfãos; considere object storage em produção multi-instância.

---

## Licença

Veja o repositório. Projeto ligado ao [Lovable](https://lovable.dev) —
não reescrever histórico git publicado.
