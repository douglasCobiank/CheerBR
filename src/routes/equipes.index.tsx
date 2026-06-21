import { createFileRoute } from "@tanstack/react-router";
import { useMemo, useState } from "react";
import { useTeams } from "@/lib/teams-store";
import { TeamCard } from "@/components/team-card";
import { AddTeamDialog } from "@/components/add-team-dialog";
import { Plus, Search, X } from "lucide-react";
import { CATEGORIAS, NIVEL_MAX } from "@/lib/constants";

export const Route = createFileRoute("/equipes/")({
  component: EquipesPage,
});

function EquipesPage() {
  const { teams = [], addTeam, removeTeam } = useTeams();
  const [q, setQ] = useState("");
  const [cat, setCat] = useState<string>("");
  const [nivel, setNivel] = useState<string>("");
  const [open, setOpen] = useState(false);

  const filtered = useMemo(() => {
    return teams.filter((t) => {
      const matchQ =
        !q ||
        [t.nome, t.cidade, t.coach, t.programa]
          .filter(Boolean)
          .some((v) => v!.toLowerCase().includes(q.toLowerCase()));

      const matchC = !cat || t.categoria === cat;
      const matchN = !nivel || t.nivel === Number(nivel);
      return matchQ && matchC && matchN;
    });
  }, [teams, q, cat, nivel]);

  return (
    <main className="mx-auto max-w-7xl px-4 py-10 sm:px-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="font-display text-5xl">Equipes</h1>
          <p className="text-sm text-muted-foreground">
            {filtered.length} de {teams.length} equipes
          </p>
        </div>

        <button
          onClick={() => setOpen(true)}
          className="inline-flex items-center gap-2 rounded-full bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground hover:opacity-90"
        >
          <Plus className="h-4 w-4" /> Nova equipe
        </button>
      </div>

      <div className="mt-6 flex flex-wrap gap-3">
        <div className="relative min-w-[220px] flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <input
            name="search"
            value={q}
            onChange={(e) => setQ(e.target.value)}
            placeholder="Buscar..."
            className="w-full rounded-full border px-4 py-2.5 pl-10 text-sm"
          />
        </div>

        <select
          name="categoria"
          value={cat}
          onChange={(e) => setCat(e.target.value)}
          className="rounded-full border px-4 py-2.5 text-sm"
        >
          <option value="">Todas as categorias</option>
          {CATEGORIAS.map((c) => (
            <option key={c}>{c}</option>
          ))}
        </select>

        <select
          name="nivel"
          value={nivel}
          onChange={(e) => setNivel(e.target.value)}
          className="rounded-full border px-4 py-2.5 text-sm"
        >
          <option value="">Todos os níveis</option>
          {Array.from({ length: NIVEL_MAX }, (_, i) => i + 1).map((n) => (
            <option key={n} value={n}>
              Nível {n}
            </option>
          ))}
        </select>
      </div>

      <div className="mt-8 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
        {filtered.map((t) => (
          <div key={t.id} className="group relative">
            <TeamCard team={t} />

            <button
              onClick={() => {
                if (confirm(`Remover ${t.nome}?`)) removeTeam(t.id);
              }}
              className="absolute right-3 top-3 rounded-full bg-background/60 p-1.5 text-muted-foreground opacity-0 transition group-hover:opacity-100 hover:text-red-500"
            >
              <X className="h-3.5 w-3.5" />
            </button>
          </div>
        ))}
      </div>

      {open && (
        <AddTeamDialog
          onClose={() => setOpen(false)}
          onSubmit={async (data) => {
            await addTeam(data);
            setOpen(false);
          }}
        />
      )}
    </main>
  );
}
