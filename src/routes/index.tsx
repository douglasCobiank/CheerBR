import { createFileRoute, Link } from "@tanstack/react-router";
import { useTeams } from "@/lib/teams-store";
import { TeamCard } from "@/components/team-card";
import { Trophy, Users, Sparkles, ArrowRight } from "lucide-react";

export const Route = createFileRoute("/")({
  head: () => ({
    meta: [
      { title: "Cheer PR — Início" },
      {
        name: "description",
        content: "Destaques, ranking e dashboard das equipes de cheerleading do Brasil.",
      },
    ],
  }),
  component: Home,
});

function Home() {
  const { teams = [] } = useTeams();
  const top3 = [...teams].sort((a, b) => b.score - a.score).slice(0, 3);
  const ativos = teams.filter((t) => t.status === "Ativo").length;
  const cidades = new Set(teams.map((t) => t.cidade)).size;

  return (
    <main className="mx-auto max-w-7xl px-4 sm:px-6">
      {/* Hero */}
      <section className="relative mt-10 overflow-hidden rounded-3xl border border-border/60 bg-[image:var(--gradient-hero)] px-6 py-16 text-center shadow-[var(--shadow-glow)] sm:py-24">
        <div className="absolute inset-0 opacity-30 mix-blend-overlay [background:radial-gradient(circle_at_30%_20%,white,transparent_40%),radial-gradient(circle_at_70%_80%,white,transparent_40%)]" />
        <div className="relative">
          <span className="inline-flex items-center gap-2 rounded-full bg-background/30 px-3 py-1 text-xs uppercase tracking-[0.25em] backdrop-blur">
            <Sparkles className="h-3 w-3" /> Mapa Cheer · Brasil
          </span>
          <h1 className="mx-auto mt-6 max-w-3xl font-display text-5xl leading-none sm:text-7xl">
            Onde o cheer brasileiro vive, treina e compete.
          </h1>
          <p className="mx-auto mt-5 max-w-xl text-base text-white/85">
            Cadastre equipes, acompanhe destaques e veja o ranking inspirado na FIFA.
          </p>
          <div className="mt-8 flex flex-wrap justify-center gap-3">
            <Link
              to="/equipes"
              className="inline-flex items-center gap-2 rounded-full bg-background px-6 py-3 text-sm font-semibold text-foreground transition hover:scale-105"
            >
              Adicionar equipe <ArrowRight className="h-4 w-4" />
            </Link>
            <Link
              to="/ranking"
              className="inline-flex items-center gap-2 rounded-full border border-white/40 bg-white/10 px-6 py-3 text-sm font-semibold text-white backdrop-blur transition hover:bg-white/20"
            >
              Ver ranking
            </Link>
          </div>
        </div>
      </section>

      {/* Stats */}
      <section className="mt-10 grid gap-4 sm:grid-cols-3">
        {[
          { icon: Users, label: "Equipes cadastradas", value: teams.length },
          { icon: Trophy, label: "Equipes ativas", value: ativos },
          { icon: Sparkles, label: "Cidades", value: cidades },
        ].map((s) => (
          <div
            key={s.label}
            className="rounded-2xl border border-border/60 bg-[image:var(--gradient-card)] p-6"
          >
            <s.icon className="h-5 w-5 text-primary" />
            <div className="mt-3 font-display text-5xl">{s.value}</div>
            <div className="mt-1 text-xs uppercase tracking-wider text-muted-foreground">
              {s.label}
            </div>
          </div>
        ))}
      </section>

      {/* Highlights */}
      <section className="mt-14">
        <div className="mb-6 flex items-end justify-between">
          <div>
            <h2 className="font-display text-4xl">Destaques</h2>
            <p className="text-sm text-muted-foreground">
              Top 3 equipes pelo score Cheer BR.
            </p>
          </div>
          <Link
            to="/ranking"
            className="text-sm text-primary hover:underline"
          >
            Ver ranking completo →
          </Link>
        </div>
        <div className="grid gap-5 md:grid-cols-3">
          {top3.map((t, i) => (
            <TeamCard key={t.id} team={t} rank={i + 1} />
          ))}
        </div>
      </section>
    </main>
  );
}
