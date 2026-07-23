import type { Team, Championship, CompetitionResult } from "./types";

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:10000/api";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, {
    ...init,
    headers: {
      ...(init?.body && !(init.body instanceof FormData)
        ? { "Content-Type": "application/json" }
        : {}),
      ...init?.headers,
    },
  });
  if (!res.ok) {
    let detail = "";
    try {
      detail = (await res.text()) ?? "";
    } catch {
      // ignore body parse errors
    }
    throw new Error(`Request failed (${res.status}): ${detail || res.statusText}`);
  }
  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

type CreateTeamPayload = Omit<Team, "id" | "score">;
type UpdateTeamPayload = Partial<Omit<Team, "score">>;
type ResultPayload = Omit<CompetitionResult, "id">;

export const api = {
  getTeams: async (categoria?: string, cidade?: string, q?: string, nivel?: number) => {
    const params = new URLSearchParams();
    if (categoria) params.append("categoria", categoria);
    if (cidade) params.append("cidade", cidade);
    if (q) params.append("q", q);
    if (nivel) params.append("nivel", String(nivel));
    const query = params.toString();
    return request<Team[]>(`/teams${query ? `?${query}` : ""}`);
  },

  createTeam: async (team: CreateTeamPayload) =>
    request<Team>("/teams", {
      method: "POST",
      body: JSON.stringify(team),
    }),

  updateTeam: async (id: string, team: UpdateTeamPayload) =>
    request<void>(`/teams/${id}`, {
      method: "PUT",
      body: JSON.stringify(team),
    }),

  deleteTeam: async (id: string) => request<void>(`/teams/${id}`, { method: "DELETE" }),

  createTeamResult: async (teamId: string, result: ResultPayload) =>
    request<CompetitionResult>(`/teams/${teamId}/results`, {
      method: "POST",
      body: JSON.stringify(result),
    }),

  updateTeamResult: async (teamId: string, resultId: string, result: ResultPayload) =>
    request<CompetitionResult>(`/teams/${teamId}/results/${resultId}`, {
      method: "PUT",
      body: JSON.stringify(result),
    }),

  deleteTeamResult: async (teamId: string, resultId: string) =>
    request<void>(`/teams/${teamId}/results/${resultId}`, { method: "DELETE" }),

  getChampionships: async () => request<Championship[]>("/championships"),

  createChampionship: async (nome: string) =>
    request<Championship>("/championships", {
      method: "POST",
      body: JSON.stringify({ nome }),
    }),

  deleteChampionship: async (id: string) =>
    request<void>(`/championships/${id}`, { method: "DELETE" }),

  uploadTeamLogo: async (teamId: string, file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    return request<{ LogoUrl: string }>(`/teams/${teamId}/logo`, {
      method: "POST",
      body: formData,
    });
  },
};
