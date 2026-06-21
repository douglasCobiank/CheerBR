import { createFileRoute, Link, useParams, useNavigate } from "@tanstack/react-router";
import { useState } from "react";
import { useTeams, useTeamResults, useUploadLogo } from "@/lib/teams-store";
import { ArrowLeft, Edit, Save, Plus, X, Image as ImageIcon } from "lucide-react";
import { TeamCard } from "@/components/team-card";

export const Route = createFileRoute("/equipes_/$id")({
  component: TeamDetailsPage,
});

const CATEGORIAS = ["Universitário", "All star", "Escolar", "Outro"];
const STATUSES = ["Ativo", "Inativo", "Desconhecido"];
const IMPORTANCIAS = ["Internacional", "Nacional", "Estadual", "Regional", "Municipal"];
const TIPOS_CATEGORIA = ["Team Cheer", "Categoria de Grupo", "Partner / Duplas", "Skills Individuais"];

function TeamDetailsPage() {
  const { id } = Route.useParams();
  const navigate = useNavigate();
  const { teams = [], updateTeam, isLoading } = useTeams();
  const { addResult } = useTeamResults(id);
  const { uploadLogo, isUploading } = useUploadLogo(id);
  
  const team = teams.find((t) => t.id === id);
  const [isEditing, setIsEditing] = useState(false);
  const [openResultModal, setOpenResultModal] = useState(false);
  const [form, setForm] = useState<any>(team || {});

  if (isLoading) return <div className="p-10 text-center">Carregando...</div>;
  
  if (!team) {
    return (
      <div className="p-10 text-center">
        <h2>Equipe não encontrada.</h2>
        <button onClick={() => navigate({ to: "/equipes" })} className="mt-4 text-primary">Voltar</button>
      </div>
    );
  }

  const handleUpdateTeam = async (e: React.FormEvent) => {
    e.preventDefault();
    await updateTeam({ id, data: form });
    setIsEditing(false);
  };

  const handleAddResult = async (resultData: any) => {
    await addResult(resultData);
    setOpenResultModal(false);
  };

  // Ensure results is an array and sort them
  const results = ((team as any).results || []).sort((a: any, b: any) => {
    // 1. Ano (Descending)
    if (b.ano !== a.ano) return b.ano - a.ano; 
    
    // 2. Importancia
    const impOrder = (imp: string) => IMPORTANCIAS.indexOf(imp) !== -1 ? IMPORTANCIAS.indexOf(imp) : 99;
    if (impOrder(a.importancia) !== impOrder(b.importancia)) {
      return impOrder(a.importancia) - impOrder(b.importancia);
    }
    
    // 3. Colocacao (Ascending)
    if (a.colocacao !== b.colocacao) return a.colocacao - b.colocacao;
    
    // 4. Categoria (Alphabetical)
    return (a.tipoCategoria || "").localeCompare(b.tipoCategoria || "");
  });

  return (
    <main className="mx-auto max-w-7xl px-4 py-10 sm:px-6">
      <div className="mb-6">
        <Link to="/equipes" className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="h-4 w-4" /> Voltar para Equipes
        </Link>
      </div>

      <div className="grid gap-8 lg:grid-cols-[1fr_2fr]">
        {/* Left Column: Team Card & Edit */}
        <div>
          {!isEditing ? (
            <div className="space-y-4">
              <TeamCard team={team} />
              <button
                onClick={() => {
                  setForm(team);
                  setIsEditing(true);
                }}
                className="w-full inline-flex justify-center items-center gap-2 rounded-full border border-border bg-card px-5 py-2.5 text-sm font-semibold hover:bg-secondary"
              >
                <Edit className="h-4 w-4" /> Editar Informações
              </button>
            </div>
          ) : (
            <form onSubmit={handleUpdateTeam} className="rounded-2xl border border-border bg-card p-6 shadow-[var(--shadow-card)]">
              <h3 className="mb-4 font-display text-2xl">Editar Equipe</h3>
              
              <div className="space-y-4">
                <Field label="Logo da Equipe (Opcional)">
                  <div className="flex items-center gap-4 border border-border bg-input/60 p-2 rounded-lg">
                    {form.logoUrl ? (
                      <img src={form.logoUrl} alt="Logo" className="h-12 w-12 shrink-0 rounded-xl object-cover border border-border" />
                    ) : (
                      <div className="grid h-12 w-12 shrink-0 place-items-center rounded-xl bg-secondary text-secondary-foreground">
                        <ImageIcon className="h-5 w-5" />
                      </div>
                    )}
                    <input 
                      type="file" 
                      accept="image/*" 
                      onChange={async (e) => {
                        const file = e.target.files?.[0];
                        if (file) {
                          const res = await uploadLogo(file);
                          setForm({ ...form, logoUrl: res.logoUrl });
                        }
                      }} 
                      className="text-sm file:mr-4 file:rounded-full file:border-0 file:bg-primary file:px-4 file:py-2 file:text-sm file:font-semibold file:text-primary-foreground hover:file:opacity-90 cursor-pointer"
                    />
                    {isUploading && <span className="text-xs text-primary animate-pulse">Enviando...</span>}
                  </div>
                </Field>
                <Field label="Nome *">
                  <input required value={form.nome} onChange={(e) => setForm({ ...form, nome: e.target.value })} className={input} />
                </Field>
                <Field label="Programa / Ginásio">
                  <input value={form.programa ?? ""} onChange={(e) => setForm({ ...form, programa: e.target.value })} className={input} />
                </Field>
                <Field label="Cidade *">
                  <input required value={form.cidade} onChange={(e) => setForm({ ...form, cidade: e.target.value })} className={input} />
                </Field>
                <Field label="Categoria">
                  <select value={form.categoria} onChange={(e) => setForm({ ...form, categoria: e.target.value })} className={input}>
                    {CATEGORIAS.map((c) => <option key={c} value={c}>{c}</option>)}
                  </select>
                </Field>
                <Field label="Coach">
                  <input value={form.coach ?? ""} onChange={(e) => setForm({ ...form, coach: e.target.value })} className={input} />
                </Field>
                <Field label="Instagram">
                  <input value={form.instagram ?? ""} onChange={(e) => setForm({ ...form, instagram: e.target.value })} className={input} />
                </Field>
                <Field label="Status">
                  <select value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value })} className={input}>
                    {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
                  </select>
                </Field>
              </div>

              <div className="mt-6 flex gap-2">
                <button type="submit" className="flex-1 rounded-full bg-primary py-2.5 text-sm font-semibold text-primary-foreground hover:opacity-90">
                  <Save className="mr-2 inline h-4 w-4" /> Salvar
                </button>
                <button type="button" onClick={() => setIsEditing(false)} className="rounded-full bg-secondary px-5 py-2.5 text-sm font-semibold hover:bg-secondary/80">
                  Cancelar
                </button>
              </div>
            </form>
          )}
        </div>

        {/* Right Column: Titles / Ranking History */}
        <div>
          <div className="flex items-center justify-between mb-6">
            <h2 className="font-display text-3xl">Histórico de Títulos</h2>
            <button
              onClick={() => setOpenResultModal(true)}
              className="inline-flex items-center gap-2 rounded-full bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground hover:opacity-90"
            >
              <Plus className="h-4 w-4" /> Lançar Resultado
            </button>
          </div>

          {results.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-border p-10 text-center text-muted-foreground">
              Nenhum campeonato registrado. O score da equipe no Ranking ProCheer será 0 até que resultados sejam adicionados.
            </div>
          ) : (
            <div className="overflow-hidden rounded-2xl border border-border bg-card">
              <table className="w-full text-sm text-left">
                <thead className="bg-secondary/60 text-xs uppercase tracking-wider text-muted-foreground border-b border-border">
                  <tr>
                    <th className="px-4 py-3">Ano</th>
                    <th className="px-4 py-3">Campeonato</th>
                    <th className="px-4 py-3">Importância</th>
                    <th className="px-4 py-3">Nível</th>
                    <th className="px-4 py-3">Categoria</th>
                    <th className="px-4 py-3 text-center">Posição</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {results.map((r: any) => (
                    <tr key={r.id} className="hover:bg-secondary/40 transition">
                      <td className="px-4 py-3 font-semibold">{r.ano}</td>
                      <td className="px-4 py-3">{r.nomeCampeonato}</td>
                      <td className="px-4 py-3 text-muted-foreground">{r.importancia}</td>
                      <td className="px-4 py-3">N{r.nivel}</td>
                      <td className="px-4 py-3">{r.tipoCategoria}</td>
                      <td className="px-4 py-3 text-center font-display text-lg text-primary">{r.colocacao}º</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {openResultModal && (
        <AddResultDialog
          onClose={() => setOpenResultModal(false)}
          onSubmit={handleAddResult}
        />
      )}
    </main>
  );
}

function AddResultDialog({ onClose, onSubmit }: { onClose: () => void; onSubmit: (data: any) => void }) {
  const [form, setForm] = useState({
    ano: new Date().getFullYear(),
    nomeCampeonato: "",
    importancia: "Estadual",
    nivel: 2,
    tipoCategoria: "Team Cheer",
    colocacao: 1
  });

  const set = (k: string, v: any) => setForm((f) => ({ ...f, [k]: v }));

  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-background/80 p-4 backdrop-blur" onClick={onClose}>
      <form
        onClick={(e) => e.stopPropagation()}
        onSubmit={(e) => {
          e.preventDefault();
          if (!form.nomeCampeonato) return;
          onSubmit(form);
        }}
        className="w-full max-w-xl rounded-2xl border border-border bg-card p-6 shadow-[var(--shadow-card)]"
      >
        <div className="flex items-center justify-between mb-5">
          <h2 className="font-display text-3xl">Lançar Resultado</h2>
          <button type="button" onClick={onClose} className="rounded-full p-1.5 text-muted-foreground hover:bg-secondary">
            <X className="h-4 w-4" />
          </button>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Field label="Campeonato *">
            <input required value={form.nomeCampeonato} onChange={(e) => set("nomeCampeonato", e.target.value)} className={input} placeholder="Ex: Paranaense" />
          </Field>
          <Field label="Ano *">
            <input type="number" required value={form.ano} onChange={(e) => set("ano", Number(e.target.value))} className={input} />
          </Field>
          <Field label="Importância">
            <select value={form.importancia} onChange={(e) => set("importancia", e.target.value)} className={input}>
              {IMPORTANCIAS.map((c) => <option key={c} value={c}>{c}</option>)}
            </select>
          </Field>
          <Field label="Tipo de Categoria">
            <select value={form.tipoCategoria} onChange={(e) => set("tipoCategoria", e.target.value)} className={input}>
              {TIPOS_CATEGORIA.map((c) => <option key={c} value={c}>{c}</option>)}
            </select>
          </Field>
          <Field label="Nível (1-5)">
            <input type="number" min="1" max="5" required value={form.nivel} onChange={(e) => set("nivel", Number(e.target.value))} className={input} />
          </Field>
          <Field label="Colocação Final (1 = Ouro)">
            <input type="number" min="1" required value={form.colocacao} onChange={(e) => set("colocacao", Number(e.target.value))} className={input} />
          </Field>
        </div>

        <div className="mt-6 p-4 rounded-xl bg-secondary/50 text-sm text-muted-foreground">
          <p><strong>Nota ProCheer:</strong> O sistema calculará os pontos automaticamente usando o peso do ano, importância, nível e categoria.</p>
        </div>

        <div className="mt-6 flex justify-end gap-2">
          <button type="button" onClick={onClose} className="rounded-full px-5 py-2.5 text-sm text-muted-foreground hover:bg-secondary">
            Cancelar
          </button>
          <button type="submit" className="rounded-full bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground hover:opacity-90">
            Salvar Resultado
          </button>
        </div>
      </form>
    </div>
  );
}

const input = "w-full rounded-lg border border-border bg-input/60 px-3 py-2 text-sm outline-none focus:border-primary";

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
