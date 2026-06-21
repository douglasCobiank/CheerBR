import { createFileRoute } from "@tanstack/react-router";
import { useMemo, useState } from "react";
import { useTeams, type Team } from "@/lib/teams-store";
import { TeamCard } from "@/components/team-card";
import { Plus, Search, X } from "lucide-react";

export const Route = createFileRoute("/equipes/")({
  head: () => ({
    meta: [
      { title: "Equipes — Cheer PR" },
      {
        name: "description",
        content: "Cadastre e explore todas as equipes de cheerleading do Paraná.",
      },
    ],
  }),
  component: EquipesPage,
});

const CATEGORIAS = ["Universitário", "All star", "Escolar", "Outro"];
const STATUSES = ["Ativo", "Inativo", "Desconhecido"];

function EquipesPage() {
  const { teams = [], addTeam, removeTeam } = useTeams();
  const [q, setQ] = useState("");
  const [cat, setCat] = useState<string>("");
  const [open, setOpen] = useState(false);

  const filtered = useMemo(() => {
    return teams.filter((t) => {
      const matchQ =
        !q ||
        [t.nome, t.cidade, t.coach, t.programa]
          .filter(Boolean)
          .some((v) => v!.toLowerCase().includes(q.toLowerCase()));
      const matchC = !cat || t.categoria === cat;
      return matchQ && matchC;
    });
  }, [teams, q, cat]);

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
          className="inline-flex items-center gap-2 rounded-full bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground shadow-[var(--shadow-glow)] hover:opacity-90"
        >
          <Plus className="h-4 w-4" /> Nova equipe
        </button>
      </div>

      <div className="mt-6 flex flex-wrap gap-3">
        <div className="relative flex-1 min-w-[220px]">
          <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <input
            value={q}
            onChange={(e) => setQ(e.target.value)}
            placeholder="Buscar por nome, cidade, coach…"
            className="w-full rounded-full border border-border bg-input/60 py-2.5 pl-10 pr-4 text-sm outline-none focus:border-primary"
          />
        </div>
        <select
          value={cat}
          onChange={(e) => setCat(e.target.value)}
          className="rounded-full border border-border bg-input/60 px-4 py-2.5 text-sm outline-none focus:border-primary"
        >
          <option value="">Todas as categorias</option>
          {CATEGORIAS.map((c) => (
            <option key={c} value={c}>{c}</option>
          ))}
        </select>
      </div>

      <div className="mt-8 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
        {filtered.map((t) => (
          <div key={t.id} className="relative">
            <TeamCard team={t} />
            <button
              onClick={() => {
                if (confirm(`Remover ${t.nome}?`)) removeTeam(t.id);
              }}
              className="absolute right-3 top-3 rounded-full bg-background/60 p-1.5 text-muted-foreground opacity-0 transition group-hover:opacity-100 hover:text-destructive"
              aria-label="Remover"
            >
              <X className="h-3.5 w-3.5" />
            </button>
          </div>
        ))}
      </div>

      {open && (
        <AddTeamDialog
          onClose={() => setOpen(false)}
          onSubmit={(t) => {
            addTeam(t);
            setOpen(false);
          }}
        />
      )}
    </main>
  );
}

type FormState = Omit<Team, "id" | "score">;

function AddTeamDialog({
  onClose,
  onSubmit,
}: {
  onClose: () => void;
  onSubmit: (t: FormState) => void;
}) {
  const [form, setForm] = useState<FormState>({
  logoUrl: null,  
  nome: "",
  programa: null,
  nivel: 2,
  cidade: "",
  estado: "PR",
  categoria: "Universitário",
  instagram: null,
  facebook: null,
  coach: null,
  fundacao: null,
  status: "Ativo",
});

  const set = <K extends keyof FormState>(k: K, v: FormState[K]) =>
    setForm((f) => ({ ...f, [k]: v }));

  return (
    <div
      className="fixed inset-0 z-50 grid place-items-center bg-background/80 p-4 backdrop-blur"
      onClick={onClose}
    >
      <form
        onClick={(e) => e.stopPropagation()}
        onSubmit={(e) => {
          e.preventDefault();
          if (!form.nome || !form.cidade) return;
          onSubmit({
            ...form,
            programa: form.programa || null,
            coach: form.coach || null,
            instagram: form.instagram || null,
            facebook: form.facebook || null,
            fundacao: form.fundacao || null,
          });
        }}
        className="w-full max-w-xl rounded-2xl border border-border bg-card p-6 shadow-[var(--shadow-card)]"
      >
        <div className="flex items-center justify-between">
          <h2 className="font-display text-3xl">Nova equipe</h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-muted-foreground hover:bg-secondary"
          >
            <X className="h-4 w-4" />
          </button>
        </div>

        <div className="mt-5 grid grid-cols-1 gap-3 sm:grid-cols-2">
          <Field label="Nome *">
            <input required value={form.nome} onChange={(e) => set("nome", e.target.value)} className={input} />
          </Field>
          <Field label="Programa / Ginásio">
            <input value={form.programa ?? ""} onChange={(e) => set("programa", e.target.value)} className={input} />
          </Field>
          <Field label="Cidade *">
            <input required value={form.cidade} onChange={(e) => set("cidade", e.target.value)} className={input} />
          </Field>
          <Field label="Estado">
            <input value={form.estado} onChange={(e) => set("estado", e.target.value)} className={input} />
          </Field>
          <Field label="Categoria">
            <select value={form.categoria} onChange={(e) => set("categoria", e.target.value)} className={input}>
              {CATEGORIAS.map((c) => <option key={c}>{c}</option>)}
            </select>
          </Field>
          <Field label="Nível (1-6)">
            <input type="number" min={1} max={6} value={form.nivel ?? ""} onChange={(e) => set("nivel", e.target.value ? Number(e.target.value) : null)} className={input} />
          </Field>
          <Field label="Coach">
            <input value={form.coach ?? ""} onChange={(e) => set("coach", e.target.value)} className={input} />
          </Field>
          <Field label="Status">
            <select value={form.status} onChange={(e) => set("status", e.target.value)} className={input}>
              {STATUSES.map((s) => <option key={s}>{s}</option>)}
            </select>
          </Field>
          <Field label="Instagram">
            <input placeholder="@equipe" value={form.instagram ?? ""} onChange={(e) => set("instagram", e.target.value)} className={input} />
          </Field>
          <Field label="Facebook">
            <input value={form.facebook ?? ""} onChange={(e) => set("facebook", e.target.value)} className={input} />
          </Field>
          <Field label="Ano de fundação">
            <input value={form.fundacao ?? ""} onChange={(e) => set("fundacao", e.target.value)} className={input} />
          </Field>
        </div>

        <div className="mt-6 flex justify-end gap-2">
          <button type="button" onClick={onClose} className="rounded-full px-5 py-2.5 text-sm text-muted-foreground hover:bg-secondary">
            Cancelar
          </button>
          <button type="submit" className="rounded-full bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground hover:opacity-90">
            Cadastrar
          </button>
        </div>
      </form>
    </div>
  );
}

const input =
  "w-full rounded-lg border border-border bg-input/60 px-3 py-2 text-sm outline-none focus:border-primary";

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="block">
      <span className="mb-1 block text-xs uppercase tracking-wider text-muted-foreground">
        {label}
      </span>
      {children}
    </label>
  );
}
