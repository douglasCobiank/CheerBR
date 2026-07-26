import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { Championship } from "./types";
import { api } from "./api";
import { queryKeys } from "./query-keys";

/**
 * Gerencia campeonatos via TanStack Query.
 *
 * Elimina o antigo padrao useState/useEffect/localStorage que
 * (1) duplicava o cache do TanStack e (2) criava IDs locais
 * ("local_<ts>_<rand>") que o backend rejeitava.
 *
 * Se o backend retornar 404/network error, o query invalida
 * e a UI mostra "Criado, mas pendente" (melhor que ID fake).
 */
export function useChampionships() {
  const queryClient = useQueryClient();

  const championshipsQuery = useQuery({
    queryKey: queryKeys.championships,
    queryFn: () => api.getChampionships(),
    initialData: [],
  });

  const createMutation = useMutation({
    mutationFn: (nome: string) => api.createChampionship(nome),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.championships });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.deleteChampionship(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.championships });
    },
  });

  return {
    championships: championshipsQuery.data ?? [],
    isLoading: championshipsQuery.isLoading,
    isError: championshipsQuery.isError,
    error: championshipsQuery.error,

    createChampionship: createMutation.mutateAsync,
    isCreating: createMutation.isPending,

    deleteChampionship: deleteMutation.mutateAsync,
    isDeleting: deleteMutation.isPending,
  };
}
