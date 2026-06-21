import { createFileRoute } from "@tanstack/react-router";
import { useMemo } from "react";
import { useTeams } from "@/lib/teams-store";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { CHART_COLORS, CHART_TOOLTIP_STYLES } from "@/lib/constants";

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
  component: DashboardPage,
});

function DashboardPage() {
  const { teams = [] } = useTeams();

  const byStatus = useMemo(() => {
    const m: Record<string, number> = {};
    teams.forEach((t) => (m[t.status] = (m[t.status] || 0) + 1));
    return Object.entries(m).map(([name, value]) => ({ name, value }));
  }, [teams]);

  const byCategoria = useMemo(() => {
    const m: Record<string, number> = {};
    teams.forEach((t) => (m[t.categoria] = (m[t.categoria] || 0) + 1));
    return Object.entries(m).map(([name, value]) => ({ name, value }));
  }, [teams]);

  const byCidade = useMemo(() => {
    const m: Record<string, number> = {};
    teams.forEach((t) => (m[t.cidade] = (m[t.cidade] || 0) + 1));
    return Object.entries(m)
      .map(([name, value]) => ({ name, value }))
      .sort((a, b) => b.value - a.value)
      .slice(0, 8);
  }, [teams]);

  const byNivel = useMemo(() => {
    const m: Record<string, number> = {};
    teams.forEach((t) => {
      const k = t.nivel ? `Nível ${t.nivel}` : "Sem nível";
      m[k] = (m[k] || 0) + 1;
    });
    return Object.entries(m).map(([name, value]) => ({ name, value }));
  }, [teams]);

  const avgScore = Math.round(teams.reduce((s, t) => s + t.score, 0) / Math.max(teams.length, 1));

  return (
    <main className="mx-auto max-w-7xl px-4 py-10 sm:px-6">
      <h1 className="font-display text-5xl">Dashboard</h1>
      <p className="text-sm text-muted-foreground">Panorama das equipes cadastradas no Cheer PR.</p>

      <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Stat label="Total" value={teams.length} />
        <Stat label="Score médio" value={avgScore} />
        <Stat label="Cidades" value={new Set(teams.map((t) => t.cidade)).size} />
        <Stat label="All Star" value={teams.filter((t) => t.categoria === "All star").length} />
      </div>

      <div className="mt-8 grid gap-6 lg:grid-cols-2">
        <Card title="Equipes por cidade (top 8)">
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={byCidade}>
              <CartesianGrid strokeDasharray="3 3" stroke="oklch(0.32 0.05 270)" />
              <XAxis
                dataKey="name"
                stroke="oklch(0.72 0.03 260)"
                fontSize={11}
                angle={-25}
                textAnchor="end"
                height={70}
              />
              <YAxis stroke="oklch(0.72 0.03 260)" fontSize={11} allowDecimals={false} />
              <Tooltip contentStyle={CHART_TOOLTIP_STYLES} />
              <Bar dataKey="value" fill={CHART_COLORS[0]} radius={[8, 8, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </Card>

        <Card title="Distribuição por nível">
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={byNivel}>
              <CartesianGrid strokeDasharray="3 3" stroke="oklch(0.32 0.05 270)" />
              <XAxis dataKey="name" stroke="oklch(0.72 0.03 260)" fontSize={11} />
              <YAxis stroke="oklch(0.72 0.03 260)" fontSize={11} allowDecimals={false} />
              <Tooltip contentStyle={CHART_TOOLTIP_STYLES} />
              <Bar dataKey="value" fill={CHART_COLORS[1]} radius={[8, 8, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </Card>

        <Card title="Status">
          <ResponsiveContainer width="100%" height={300}>
            <PieChart>
              <Pie data={byStatus} dataKey="value" nameKey="name" outerRadius={110} label>
                {byStatus.map((_, i) => (
                  <Cell key={i} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                ))}
              </Pie>
              <Tooltip contentStyle={CHART_TOOLTIP_STYLES} />
            </PieChart>
          </ResponsiveContainer>
        </Card>

        <Card title="Categorias">
          <ResponsiveContainer width="100%" height={300}>
            <PieChart>
              <Pie data={byCategoria} dataKey="value" nameKey="name" outerRadius={110} label>
                {byCategoria.map((_, i) => (
                  <Cell key={i} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                ))}
              </Pie>
              <Tooltip contentStyle={CHART_TOOLTIP_STYLES} />
            </PieChart>
          </ResponsiveContainer>
        </Card>
      </div>
    </main>
  );
}

function Stat({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-2xl border border-border/60 bg-[image:var(--gradient-card)] p-5">
      <div className="text-xs uppercase tracking-wider text-muted-foreground">{label}</div>
      <div className="mt-2 font-display text-5xl text-foreground">{value}</div>
    </div>
  );
}

function Card({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="rounded-2xl border border-border/60 bg-card p-5">
      <h3 className="mb-4 font-display text-2xl">{title}</h3>
      {children}
    </div>
  );
}
