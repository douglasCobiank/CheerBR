# Cheer PR — Mapa das Equipes de Cheerleading do Paraná

Plataforma full-stack para cadastro, ranking e dashboard de equipes de cheerleading do Paraná, com sistema de pontuação estilo ProCheer.

---

## Arquitetura

```
cheerleading-app/
├── src/                    # Frontend (React + TanStack + Tailwind)
│   ├── components/         # Componentes reutilizáveis
│   ├── data/               # Seed data (89 equipes)
│   ├── hooks/              # Custom hooks
│   ├── lib/                # API client, store, utils
│   └── routes/             # Páginas (TanStack Router)
├── backend/                # Backend (.NET 10 Web API)
│   ├── Cheer.Domain/       # Entidades e interfaces
│   ├── Cheer.Application/  # DTOs e serviços (lógica de negócio)
│   ├── Cheer.Infrastructure/# EF Core, repositórios, migrations
│   └── Cheer.Api/          # Controllers REST
└── ...
```

---

## Frontend

### Stack
- **React 19** + **TypeScript**
- **TanStack Router** (rotas file-based)
- **TanStack Query** (cache e mutações)
- **TanStack Start** (SSR)
- **Tailwind CSS v4** + **shadcn/ui**
- **Recharts** (gráficos do dashboard)
- **Zustand** (estado global — atualmente não utilizado, usa TanStack Query)
- **Zod** (validação de formulários)

### Rotas

| Rota             | Página             | Descrição                            |
| ---------------- | ------------------ | ------------------------------------ |
| `/`              | Home               | Hero, stats, top 3 equipes           |
| `/equipes`       | Lista de Equipes   | Grid com busca/filtro + criar equipe |
| `/equipes/$id`   | Detalhe da Equipe  | Editar, upload logo, resultados      |
| `/ranking`       | Ranking            | Pódio + tabela ordenada por score    |
| `/dashboard`     | Dashboard          | Gráficos e indicadores              |

### API Client (`src/lib/api.ts`)
Comunica com `https://cheerbr-2.onrender.com/api`:
- `GET /teams` — listar equipes
- `GET /teams/:id` — detalhe
- `POST /teams` — criar equipe
- `PUT /teams/:id` — atualizar equipe
- `DELETE /teams/:id` — remover equipe
- `GET /ranking` — ranking (filtro por categoria)
- `GET /stats/overview` — estatísticas do dashboard
- `POST /teams/:id/results` — adicionar resultado
- `POST /teams/:id/logo` — upload de logo

### Store (`src/lib/teams-store.ts`)
Hooks TanStack Query: `useTeams()`, `useTeamResults()`, `useUploadLogo()`.

### Tipos Principais (`src/lib/teams-store.ts`)

```typescript
type Team = {
  id: string;
  nome: string;
  programa: string | null;
  nivel: number | null;
  cidade: string;
  estado: string;
  categoria: string;
  instagram: string | null;
  facebook: string | null;
  coach: string | null;
  fundacao: string | null;
  status: string;
  logoUrl: string | null;
  score: number;
};
```

---

## Backend (.NET 10)

### Estrutura em Camadas

| Camada              | Projeto              | Responsabilidade                          |
| ------------------- | -------------------- | ----------------------------------------- |
| **Domain**          | `Cheer.Domain`       | Entidades (`Team`, `CompetitionResult`) e interfaces |
| **Application**     | `Cheer.Application`  | DTOs, `ITeamService`, `TeamService`       |
| **Infrastructure**  | `Cheer.Infrastructure| EF Core `AppDbContext`, `TeamRepository`, Migrations |
| **API**             | `Cheer.Api`          | Controllers REST, Swagger, CORS           |

### Entidades

- **Team**: `Id`, `Nome`, `Programa`, `Nivel`, `Cidade`, `Estado`, `Categoria`, `Instagram`, `Facebook`, `Coach`, `Fundacao`, `Status`, `LogoUrl`, `Score`, `Results` (navigation)
- **CompetitionResult**: `Id`, `TeamId`, `Ano`, `NomeCampeonato`, `Importancia`, `Nivel`, `TipoCategoria`, `Colocacao`

### Sistema de Pontuação (ProCheer)

O score é calculado com base nos resultados de competições:

```
pontos_base = f(colocacao)
  # 1º=100, 2º=70, 3º=50, 4º=30, 5º=20, demais=10

pesos = importancia * nivel * tipo_categoria
  # Importancia: Internacional=3.0, Nacional=2.5, Estadual=2.0, Regional=1.7, Municipal=1.5
  # Nivel: 1=1.1, 2=1.2, 3=1.3, 4=1.4, 5=1.5
  # Tipo: Team Cheer=1.5, Grupo=1.2, Duplas=1.1, Skills=0.9

decay = max(0, 1 - (ano_atual - ano_resultado) * 0.1)

score_total = Σ(pontos_base * pesos * decay)
```

### Endpoints da API

| Método | Rota                        | Descrição              |
| ------ | --------------------------- | ---------------------- |
| GET    | `/api/teams`                | Lista equipes          |
| GET    | `/api/teams/{id}`           | Detalhe da equipe      |
| POST   | `/api/teams`                | Criar equipe           |
| PUT    | `/api/teams/{id}`           | Atualizar equipe       |
| DELETE | `/api/teams/{id}`           | Remover equipe         |
| POST   | `/api/teams/{id}/results`   | Adicionar resultado    |
| POST   | `/api/teams/{id}/logo`      | Upload de logo         |
| GET    | `/api/ranking`              | Ranking (q)            |
| GET    | `/api/stats/overview`       | Estatísticas           |
| GET    | `/`                         | Healthcheck            |

---

## Scripts

| Comando             | Descrição                  |
| ------------------- | -------------------------- |
| `npm run dev`       | Iniciar dev server         |
| `npm run build`     | Build de produção          |
| `npm run preview`   | Preview do build           |
| `npm run lint`      | ESLint                     |
| `npm run format`    | Prettier                   |

---

## Seed Data

89 equipes reais do Paraná (PR) em `src/data/teams.seed.json`. Categorias: Universitário, All star, Escolar, Outro.

---

## Deploy

- **Frontend**: Render.com (cheerbr-1.onrender.com)
- **Backend**: Render.com (cheerbr-2.onrender.com)
- **Banco**: PostgreSQL via Render
- **Docker**: Multi-stage build com .NET 10 (`backend/Dockerfile`)

---

## Erros Corrigidos

| Arquivo              | Erro                                                         |
| -------------------- | ------------------------------------------------------------ |
| `src/lib/api.ts`     | Tipo `Partial<Omit<Team, "score">>` no updateTeam            |
| `src/lib/teams-store.ts` | Uso de `Record<string, unknown>` no lugar de `any`      |
| `src/routes/equipes.index.tsx` | FormState aceitando `null` em campos opcionais     |
| `src/routes/equipes_.$id.tsx` | Formulário de edição tipado com `TeamFormState`      |
| Vários               | Substituição de `any` por tipos específicos                  |
| Vários               | Formatação Prettier                                          |
