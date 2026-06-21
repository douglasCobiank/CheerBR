import type { Team, Championship } from "./types";

const API_URL = "https://cheerbr-2.onrender.com/api";

export const api = {
  getTeams: async (categoria?: string, cidade?: string, q?: string, nivel?: number) => {
    const params = new URLSearchParams();
    if (categoria) params.append("categoria", categoria);
    if (cidade) params.append("cidade", cidade);
    if (q) params.append("q", q);
    if (nivel) params.append("nivel", String(nivel));

    const query = params.toString();
    const url = `${API_URL}/teams${query ? `?${query}` : ""}`;
    const res = await fetch(url);
    if (!res.ok) throw new Error("Failed to fetch teams");
    return res.json() as Promise<Team[]>;
  },

  getTeam: async (id: string) => {
    const res = await fetch(`${API_URL}/teams/${id}`);
    if (!res.ok) throw new Error("Failed to fetch team");
    return res.json() as Promise<Team>;
  },

  createTeam: async (team: Omit<Team, "id" | "score">) => {
    const res = await fetch(`${API_URL}/teams`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(team),
    });
    if (!res.ok) throw new Error("Failed to create team");
    return res.json() as Promise<Team>;
  },

  updateTeam: async (id: string, team: Partial<Omit<Team, "score">>) => {
    const res = await fetch(`${API_URL}/teams/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(team),
    });
    if (!res.ok) throw new Error("Failed to update team");
  },

  deleteTeam: async (id: string) => {
    const res = await fetch(`${API_URL}/teams/${id}`, {
      method: "DELETE",
    });
    if (!res.ok) throw new Error("Failed to delete team");
  },

  getRanking: async (categoria?: string) => {
    const url = `${API_URL}/ranking${categoria ? `?categoria=${categoria}` : ""}`;
    const res = await fetch(url);
    if (!res.ok) throw new Error("Failed to fetch ranking");
    return res.json() as Promise<Team[]>;
  },

  getStatsOverview: async () => {
    const res = await fetch(`${API_URL}/stats/overview`);
    if (!res.ok) throw new Error("Failed to fetch stats overview");
    return res.json();
  },

  createTeamResult: async (teamId: string, result: Record<string, unknown>) => {
    const res = await fetch(`${API_URL}/teams/${teamId}/results`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(result),
    });
    if (!res.ok) throw new Error("Failed to create result");
    return res.json();
  },

  getChampionships: async () => {
    const res = await fetch(`${API_URL}/championships`);
    if (!res.ok) throw new Error("Failed to fetch championships");
    return res.json() as Promise<Championship[]>;
  },

  createChampionship: async (nome: string) => {
    const res = await fetch(`${API_URL}/championships`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ nome }),
    });
    if (!res.ok) throw new Error("Failed to create championship");
    return res.json() as Promise<Championship>;
  },

  deleteChampionship: async (id: string) => {
    const res = await fetch(`${API_URL}/championships/${id}`, {
      method: "DELETE",
    });
    if (!res.ok) throw new Error("Failed to delete championship");
  },

  updateTeamResult: async (teamId: string, resultId: string, result: Record<string, unknown>) => {
    const res = await fetch(`${API_URL}/teams/${teamId}/results/${resultId}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(result),
    });
    if (!res.ok) throw new Error("Failed to update result");
    return res.json();
  },

  deleteTeamResult: async (teamId: string, resultId: string) => {
    const res = await fetch(`${API_URL}/teams/${teamId}/results/${resultId}`, {
      method: "DELETE",
    });
    if (!res.ok) throw new Error("Failed to delete result");
  },

  uploadTeamLogo: async (teamId: string, file: File) => {
    const formData = new FormData();
    formData.append("file", file);

    const res = await fetch(`${API_URL}/teams/${teamId}/logo`, {
      method: "POST",
      body: formData,
    });
    if (!res.ok) throw new Error("Failed to upload logo");
    return res.json();
  },
};
