import { createFileRoute } from "@tanstack/react-router";
import { useMemo, useState } from "react";
import { useTeams, type Team } from "@/lib/teams-store";
import { TeamCard } from "@/components/team-card";
import { Plus, Search, X } from "lucide-react";

export const Route = createFileRoute("/equipes/")({
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
          className="inline-flex items-center gap-2 rounded-full bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground hover:opacity-90"
        >
          <Plus className="h-4 w-4" /> Nova equipe
        </button>
      </div>

      {/* filtros */}
      <div className="mt-6 flex flex-wrap gap-3">
        <div className="relative flex-1 min-w-[220px]">
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
          <option value="">Todas</option>
          {CATEGORIAS.map((c) => (
            <option key={c}>{c}</option>
          ))}
        </select>
      </div>

      {/* lista */}
      <div className="mt-8 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
        {filtered.map((t) => (
          <div key={t.id} className="relative group">
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

/* =========================
   FORM TYPE (CORRIGIDO)
   ========================= */

type FormState = {
  nome: string;
  programa: string;
  nivel: number;
  cidade: string;
  estado: string;
  categoria: string;
  instagram: string;
  facebook: string;
  coach: string;
  fundacao: string;
  status: string;
  logoUrl: string | null;
};

/* =========================
   MODAL
   ========================= */

function AddTeamDialog({
  onClose,
  onSubmit,
}: {
  onClose: () => void;
  onSubmit: (t: any) => void;
}) {
  const [form, setForm] = useState<FormState>({
    logoUrl: null,
    nome: "",
    programa: "",
    nivel: 2,
    cidade: "",
    estado: "PR",
    categoria: "Universitário",
    instagram: "",
    facebook: "",
    coach: "",
    fundacao: "",
    status: "Ativo",
  });

  const set = <K extends keyof FormState>(k: K, v: FormState[K]) =>
    setForm((f) => ({ ...f, [k]: v }));

  return (
    <div
      className="fixed inset-0 z-50 grid place-items-center bg-black/40"
      onClick={onClose}
    >
      <form
        onClick={(e) => e.stopPropagation()}
        onSubmit={(e) => {
          e.preventDefault();

          if (!form.nome || !form.cidade) return;

          // 🔥 CONVERSÃO LIMPA PRA API
          onSubmit({
            ...form,
            programa: form.programa || null,
            coach: form.coach || null,
            instagram: form.instagram || null,
            facebook: form.facebook || null,
            fundacao: form.fundacao || null,
          });
        }}
        className="w-full max-w-xl rounded-2xl bg-white p-6"
      >
        <h2 className="text-xl font-bold mb-4">Nova equipe</h2>

        <div className="grid grid-cols-2 gap-3">
          <Field label="Nome">
            <input
              name="nome"
              value={form.nome}
              onChange={(e) => set("nome", e.target.value)}
            />
          </Field>

          <Field label="Cidade">
            <input
              name="cidade"
              value={form.cidade}
              onChange={(e) => set("cidade", e.target.value)}
            />
          </Field>

          <Field label="Programa">
            <input
              name="programa"
              value={form.programa}
              onChange={(e) => set("programa", e.target.value)}
            />
          </Field>

          <Field label="Coach">
            <input
              name="coach"
              value={form.coach}
              onChange={(e) => set("coach", e.target.value)}
            />
          </Field>

          <Field label="Instagram">
            <input
              name="instagram"
              value={form.instagram}
              onChange={(e) => set("instagram", e.target.value)}
            />
          </Field>

          <Field label="Facebook">
            <input
              name="facebook"
              value={form.facebook}
              onChange={(e) => set("facebook", e.target.value)}
            />
          </Field>
        </div>

        <div className="flex justify-end gap-2 mt-4">
          <button type="button" onClick={onClose}>
            Cancelar
          </button>
          <button type="submit">Salvar</button>
        </div>
      </form>
    </div>
  );
}

/* =========================
   FIELD
   ========================= */

function Field({
  label,
  children,
}: {
  label: string;
  children: React.ReactNode;
}) {
  return (
    <label className="flex flex-col gap-1 text-sm">
      <span>{label}</span>
      {children}
    </label>
  );
}