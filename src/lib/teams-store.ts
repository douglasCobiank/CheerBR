import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { Team } from "./types";
import { api } from "./api";

export function useTeams() {
  const queryClient = useQueryClient();

  const teamsQuery = useQuery({
    queryKey: ["teams"],
    queryFn: () => api.getTeams(),
    initialData: [],
  });

  const invalidateAll = () => {
    queryClient.invalidateQueries({ queryKey: ["teams"] });
    queryClient.invalidateQueries({ queryKey: ["ranking"] });
    queryClient.invalidateQueries({ queryKey: ["stats"] });
  };

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

    isCreating: addMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: removeMutation.isPending,

    addTeam: addMutation.mutateAsync,
    updateTeam: updateMutation.mutateAsync,
    removeTeam: removeMutation.mutateAsync,
  };
}

export function useTeamResults(teamId: string) {
  const queryClient = useQueryClient();

  const addResultMutation = useMutation({
    mutationFn: (data: Record<string, unknown>) => api.createTeamResult(teamId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teams"] });
      queryClient.invalidateQueries({ queryKey: ["ranking"] });
      queryClient.invalidateQueries({ queryKey: ["stats"] });
    },
  });

  return {
    addResult: addResultMutation.mutateAsync,
    isCreating: addResultMutation.isPending,
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
