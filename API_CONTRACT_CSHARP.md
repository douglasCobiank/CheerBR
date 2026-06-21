# Cheer PR — Contrato de API (Backend C# / .NET)

O frontend atual usa dados locais (LocalStorage + seed da planilha) no arquivo
`src/lib/teams-store.ts`. Para plugar o backend C#, basta substituir o store
por chamadas REST aos endpoints abaixo.

## Modelo: Team

```csharp
public class Team
{
    public string Id { get; set; }            // ex: "t_1"
    public string Nome { get; set; }          // obrigatório
    public string? Programa { get; set; }     // programa/ginásio
    public int? Nivel { get; set; }           // 1..6
    public string Cidade { get; set; }        // obrigatório
    public string Estado { get; set; } = "PR";
    public string Categoria { get; set; }     // "Universitário" | "All star" | "Escolar" | "Outro"
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }
    public string? Coach { get; set; }
    public string? Fundacao { get; set; }
    public string Status { get; set; }        // "Ativo" | "Inativo" | "Desconhecido"
    public int Score { get; set; }            // calculado no backend
}
```

## Cálculo do Score (estilo FIFA overall)

```csharp
int score = 1000;
if (t.Nivel.HasValue) score += t.Nivel.Value * 120;
if (t.Status == "Ativo")  score += 200;
if (t.Status == "Inativo") score -= 250;
if (!string.IsNullOrEmpty(t.Coach))     score += 80;
if (!string.IsNullOrEmpty(t.Instagram)) score += 30;
if (!string.IsNullOrEmpty(t.Facebook))  score += 20;
if (!string.IsNullOrEmpty(t.Programa))  score += 60;
if (t.Categoria == "All star")          score += 90;
```

## Endpoints

| Método | Rota                              | Descrição                                     |
|--------|-----------------------------------|-----------------------------------------------|
| GET    | `/api/teams`                      | Lista equipes (query: `?categoria=&cidade=&q=`) |
| GET    | `/api/teams/{id}`                 | Detalhe                                       |
| POST   | `/api/teams`                      | Cria equipe (recalcula score)                 |
| PUT    | `/api/teams/{id}`                 | Atualiza                                      |
| DELETE | `/api/teams/{id}`                 | Remove                                        |
| GET    | `/api/ranking?categoria=`         | Equipes ordenadas por score desc              |
| GET    | `/api/stats/overview`             | Totais, médias, top cidades, contagens        |

### Exemplo de resposta `/api/stats/overview`

```json
{
  "total": 89,
  "ativos": 32,
  "cidades": 17,
  "scoreMedio": 1340,
  "porStatus": [{"name":"Ativo","value":32}],
  "porCategoria": [{"name":"All star","value":40}],
  "porCidade": [{"name":"Curitiba","value":35}],
  "porNivel": [{"name":"Nível 2","value":18}]
}
```

## CORS

Liberar a origem do front (Lovable preview / domínio publicado):

```csharp
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("https://seu-dominio.lovable.app", "http://localhost:5173")
     .AllowAnyHeader().AllowAnyMethod()));
```

## Como plugar no frontend

1. Crie `src/lib/api.ts` com `fetch("https://api.seudominio.com/api/teams")`.
2. Troque o `useTeams` de zustand por TanStack Query (`useQuery`/`useMutation`).
3. Remova o seed `src/data/teams.seed.json` quando o banco já estiver populado
   (você pode importar esse JSON uma vez no backend para fazer o seed inicial).
