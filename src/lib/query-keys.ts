/**
 * Query keys centralizadas.
 *
 * Em TanStack Query, as chaves sao arrays hierarquicos. Manter todas aqui
 * evita literals espalhados ("teams", "championships"...) e typos silenciosos
 * quando uma pagina invalida chaves definidas em outro modulo.
 *
 * Incluir parametros de filtro dentro da chave (`["teams", { categoria, q }]`)
 * e essencial para que cache hits concorrentem com invalidacao seletiva.
 */
export const queryKeys = {
  teams: ["teams"] as const,
  team: (id: string) => ["teams", "detail", id] as const,
  teamsList: (filters?: { categoria?: string; cidade?: string; q?: string; nivel?: number }) =>
    ["teams", "list", filters ?? {}] as const,
  championships: ["championships"] as const,
  ranking: (categoria?: string) => ["ranking", categoria ?? "all"] as const,
  stats: ["stats", "overview"] as const,
} as const;
