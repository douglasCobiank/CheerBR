import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { Championship } from "./types";
import { api } from "./api";

export function useChampionships() {
  const queryClient = useQueryClient();

  const championshipsQuery = useQuery({
    queryKey: ["championships"],
    queryFn: () => api.getChampionships(),
    initialData: [],
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["championships"] });
  };

  const createMutation = useMutation({
    mutationFn: api.createChampionship,
    onSuccess: invalidate,
  });

  const deleteMutation = useMutation({
    mutationFn: api.deleteChampionship,
    onSuccess: invalidate,
  });

  return {
    championships: championshipsQuery.data ?? ([] as Championship[]),
    isLoading: championshipsQuery.isLoading,
    createChampionship: createMutation.mutateAsync,
    deleteChampionship: deleteMutation.mutateAsync,
    isCreating: createMutation.isPending,
  };
}
