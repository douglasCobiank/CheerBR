import { createFileRoute, lazyRouteComponent } from "@tanstack/react-router";

import { Skeleton } from "@/components/ui/skeleton";

export const Route = createFileRoute("/dashboard")({
  head: () => ({
    meta: [
      { title: "Dashboard — Cheer PR" },
      {
        name: "description",
        content:
          "Visão geral das equipes de cheerleading do PR: status, categorias, distribuição por cidade e nível técnico.",
      },
    ],
  }),
  component: lazyRouteComponent(() =>
    import("./-components/DashboardPage").then((m) => m.DashboardPage),
  ),
  pendingComponent: () => (
    <main className="mx-auto max-w-7xl px-4 py-10 sm:px-6">
      <Skeleton className="h-12 w-44" />
      <Skeleton className="mt-2 h-4 w-72" />
      <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-24 rounded-2xl" />
        ))}
      </div>
      <div className="mt-8 grid gap-6 lg:grid-cols-2">
        {Array.from({ length: 2 }).map((_, i) => (
          <Skeleton key={i} className="h-72 rounded-2xl" />
        ))}
      </div>
    </main>
  ),
});
