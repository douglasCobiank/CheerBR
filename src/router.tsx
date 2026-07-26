import { QueryClient } from "@tanstack/react-query";
import { createRouter } from "@tanstack/react-router";
import { routeTree } from "./routeTree.gen";

const DEFAULT_STALE_TIME = 30_000; // 30s: evita re-fetchs agressivos mas mantem frescor
const DEFAULT_GC_TIME = 5 * 60_000; // 5min
const DEFAULT_RETRY = 1; // 1 retry apenas (default TanStack sao 3, lenta erro de surface)

export const getRouter = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: DEFAULT_STALE_TIME,
        gcTime: DEFAULT_GC_TIME,
        retry: DEFAULT_RETRY,
        refetchOnWindowFocus: false, // SPA admin: refetch on focus e ruidoso e desnecessario
      },
      mutations: {
        retry: 0, // mutations de admin: nao retentativas automaticas (usuario decide)
      },
    },
  });

  const router = createRouter({
    routeTree,
    context: { queryClient },
    scrollRestoration: true,
    defaultPreloadStaleTime: 0,
  });

  return router;
};
