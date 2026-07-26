import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { Team, CompetitionResult } from "./types";
import { api } from "./api";
import { queryKeys } from "./query-keys";

/**
 * Invalida todas as query keys derivadas das equipes.
 * Centraliza a logica de cache invalidation para evitar blocos
 * triplicados de invalidateQueries nas paginas.
 */
export function useInvalidateAll() {
  const queryClient = useQueryClient();
  return () => {
    queryClient.invalidateQueries({ queryKey: queryKeys.teams });
  };
}

export function useTeams() {
  const invalidateAll = useInvalidateAll();

  const teamsQuery = useQuery({
    queryKey: queryKeys.teams,
    queryFn: () => api.getTeams(),
    initialData: [],
  });

  const addMutation = useMutation({
    mutationFn: api.createTeam,
    onSuccess: invalidateAll,
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Team> }) => api.updateTeam(id, data),
    onSuccess: invalidateAll,
  });

  const removeMutation = useMutation({
    mutationFn: api.deleteTeam,
    onSuccess: invalidateAll,
  });

  return {
    teams: teamsQuery.data ?? [],
    isLoading: teamsQuery.isLoading,
    isError: teamsQuery.isError,
    error: teamsQuery.error,
    refetch: teamsQuery.refetch,

    isCreating: addMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: removeMutation.isPending,

    addTeam: addMutation.mutateAsync,
    updateTeam: updateMutation.mutateAsync,
    removeTeam: removeMutation.mutateAsync,
  };
}

export function useTeamResults(teamId: string) {
  const invalidateAll = useInvalidateAll();

  const addResultMutation = useMutation({
    mutationFn: (data: Omit<CompetitionResult, "id">) => api.createTeamResult(teamId, data),
    onSuccess: invalidateAll,
  });

  const updateResultMutation = useMutation({
    mutationFn: ({ resultId, data }: { resultId: string; data: Omit<CompetitionResult, "id"> }) =>
      api.updateTeamResult(teamId, resultId, data),
    onSuccess: invalidateAll,
  });

  const deleteResultMutation = useMutation({
    mutationFn: (resultId: string) => api.deleteTeamResult(teamId, resultId),
    onSuccess: invalidateAll,
  });

  return {
    addResult: addResultMutation.mutateAsync,
    isCreating: addResultMutation.isPending,

    updateResult: updateResultMutation.mutateAsync,
    isUpdating: updateResultMutation.isPending,

    deleteResult: deleteResultMutation.mutateAsync,
    isDeleting: deleteResultMutation.isPending,
  };
}

export function useUploadLogo(teamId: string) {
  const invalidateAll = useInvalidateAll();

  const uploadMutation = useMutation({
    mutationFn: (file: File) => api.uploadTeamLogo(teamId, file),
    onSuccess: invalidateAll,
  });

  return {
    uploadLogo: uploadMutation.mutateAsync,
    isUploading: uploadMutation.isPending,
  };
}
