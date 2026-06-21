import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "./api";

export type Team = {
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

export function useTeams() {
  const queryClient = useQueryClient();

  const teamsQuery = useQuery({
    queryKey: ["teams"],
    queryFn: () => api.getTeams(),
    initialData: [],
  });

  const addMutation = useMutation({
    mutationFn: api.createTeam,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teams"] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["stats"] });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: Omit<Team, "score"> }) => api.updateTeam(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teams"] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["stats"] });
    },
  });

  const removeMutation = useMutation({
    mutationFn: api.deleteTeam,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teams"] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["stats"] });
    },
  });

  return {
    teams: teamsQuery.data,
    isLoading: teamsQuery.isLoading,
    addTeam: addMutation.mutateAsync,
    updateTeam: updateMutation.mutateAsync,
    removeTeam: removeMutation.mutateAsync,
  };
}

export function useTeamResults(teamId: string) {
  const queryClient = useQueryClient();

  const addResultMutation = useMutation({
    mutationFn: (data: any) => api.createTeamResult(teamId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teams"] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["stats"] });
    },
  });

  return {
    addResult: addResultMutation.mutateAsync,
  };
}

export function useUploadLogo(teamId: string) {
  const queryClient = useQueryClient();

  const uploadMutation = useMutation({
    mutationFn: (file: File) => api.uploadTeamLogo(teamId, file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teams"] });
    },
  });

  return {
    uploadLogo: uploadMutation.mutateAsync,
    isUploading: uploadMutation.isPending,
  };
}
