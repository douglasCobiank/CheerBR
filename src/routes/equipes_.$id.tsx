import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import type { Team, CompetitionResult } from "@/lib/types";
import { useTeams, useTeamResults, useUploadLogo } from "@/lib/teams-store";
import { TeamCard } from "@/components/team-card";
import { Field } from "@/components/ui/field";
import { Modal } from "@/components/modal";
import { ArrowLeft, Edit, Save, Plus, X, Image as ImageIcon, Trash2 } from "lucide-react";
import {
  CATEGORIAS,
  STATUSES,
  IMPORTANCIAS,
  TIPOS_CATEGORIA,
  INPUT_CLASS,
  NIVEIS,
} from "@/lib/constants";
import {
  teamSchema,
  resultSchema,
  type TeamFormValues,
  type ResultFormValues,
} from "@/lib/schemas";
import { ResultForm } from "@/components/result-form";

export const Route = createFileRoute("/equipes_/$id")({
  component: TeamDetailsPage,
});

function TeamDetailsPage() {
  const { id } = Route.useParams();
  const navigate = useNavigate();
  const { teams = [], updateTeam, isLoading } = useTeams();
  const { addResult, updateResult, deleteResult } = useTeamResults(id);
  const { uploadLogo, isUploading } = useUploadLogo(id);

  const team = teams.find((t) => t.id === id);
  const [isEditing, setIsEditing] = useState(false);
  const [openResultModal, setOpenResultModal] = useState(false);
  const [editingResult, setEditingResult] = useState<CompetitionResult | null>(null);

  const editForm = useForm<TeamFormValues>({
    resolver: zodResolver(teamSchema),
    values: team
      ? {
          nome: team.nome,
          programa: team.programa ?? "",
          nivel: team.nivel,
          cidade: team.cidade,
          estado: team.estado,
          categoria: team.categoria as TeamFormValues["categoria"],
          instagram: team.instagram ?? "",
          facebook: team.facebook ?? "",
          coach: team.coach ?? "",
          fundacao: team.fundacao ?? "",
          status: team.status as TeamFormValues["status"],
          logoUrl: team.logoUrl,
        }
      : undefined,
  });

  if (isLoading) return <div className="p-10 text-center">Carregando...</div>;

  if (!team) {
    return (
      <div className="p-10 text-center">
        <h2>Equipe não encontrada.</h2>
        <button onClick={() => navigate({ to: "/equipes" })} className="mt-4 text-primary">
          Voltar
        </button>
      </div>
    );
  }

  const handleUpdateTeam = editForm.handleSubmit(async (data) => {
    await updateTeam({ id, data });
    setIsEditing(false);
  });

  const handleAddResult = async (resultData: ResultFormValues) => {
    await addResult(resultData);
    setOpenResultModal(false);
  };

  const handleUpdateResult = async (resultData: ResultFormValues) => {
    if (!editingResult) return;
    await updateResult({ resultId: editingResult.id, data: resultData });
    setEditingResult(null);
  };

  const handleDeleteResult = async (resultId: string) => {
    if (!confirm("Tem certeza que deseja excluir este resultado?")) return;
    await deleteResult(resultId);
  };

  const results: CompetitionResult[] = team.results ?? [];
  results.sort((a, b) => {
    if (b.ano !== a.ano) return b.ano - a.ano;
    const impOrder = (imp: string) => {
      const idx = IMPORTANCIAS.indexOf(imp as (typeof IMPORTANCIAS)[number]);
      return idx !== -1 ? idx : 99;
    };
    if (impOrder(a.importancia) !== impOrder(b.importancia)) {
      return impOrder(a.importancia) - impOrder(b.importancia);
    }
    if (a.colocacao !== b.colocacao) return a.colocacao - b.colocacao;
    return (a.tipoCategoria || "").localeCompare(b.tipoCategoria || "");
  });

  return (
    <main className="mx-auto max-w-7xl px-4 py-10 sm:px-6">
      <div className="mb-6">
        <Link
          to="/equipes"
          className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" /> Voltar para Equipes
        </Link>
      </div>

      <div className="grid gap-8 lg:grid-cols-[1fr_2fr]">
        <div>
          {!isEditing ? (
            <div className="space-y-4">
              <TeamCard team={team} />
              <button
                onClick={() => setIsEditing(true)}
                className="inline-flex w-full items-center justify-center gap-2 rounded-full border border-border bg-card px-5 py-2.5 text-sm font-semibold hover:bg-secondary"
              >
                <Edit className="h-4 w-4" /> Editar Informações
              </button>
            </div>
          ) : (
            <form
              onSubmit={handleUpdateTeam}
              className="rounded-2xl border border-border bg-card p-6 shadow-[var(--shadow-card)]"
            >
              <h3 className="mb-4 font-display text-2xl">Editar Equipe</h3>

              <div className="space-y-4">
                <Field label="Logo da Equipe (Opcional)">
                  <div className="flex items-center gap-4 rounded-lg border border-border bg-input/60 p-2">
                    {editForm.watch("logoUrl") ? (
                      <img
                        src={editForm.watch("logoUrl") ?? ""}
                        alt="Logo"
                        width={48}
                        height={48}
                        loading="lazy"
                        className="h-12 w-12 shrink-0 rounded-xl border border-border object-cover"
                        onError={(e) => {
                          (e.target as HTMLImageElement).style.display = "none";
                        }}
                      />
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
                          editForm.setValue("logoUrl", res.LogoUrl);
                        }
                      }}
                      className="cursor-pointer text-sm file:mr-4 file:rounded-full file:border-0 file:bg-primary file:px-4 file:py-2 file:text-sm file:font-semibold file:text-primary-foreground hover:file:opacity-90"
                    />
                    {isUploading && (
                      <span className="animate-pulse text-xs text-primary">Enviando...</span>
                    )}
                  </div>
                </Field>
                <Field label="Nome *" error={editForm.formState.errors.nome?.message}>
                  <input className={INPUT_CLASS} {...editForm.register("nome")} />
                </Field>
                <Field label="Programa / Ginásio">
                  <input className={INPUT_CLASS} {...editForm.register("programa")} />
                </Field>
                <Field label="Cidade *" error={editForm.formState.errors.cidade?.message}>
                  <input className={INPUT_CLASS} {...editForm.register("cidade")} />
                </Field>
                <Field label="Estado">
                  <input className={INPUT_CLASS} {...editForm.register("estado")} />
                </Field>
                <Field label="Categoria" error={editForm.formState.errors.categoria?.message}>
                  <select className={INPUT_CLASS} {...editForm.register("categoria")}>
                    {CATEGORIAS.map((c) => (
                      <option key={c} value={c}>
                        {c}
                      </option>
                    ))}
                  </select>
                </Field>
                <Field label="Nível">
                  <select
                    className={INPUT_CLASS}
                    {...editForm.register("nivel", { valueAsNumber: true })}
                  >
                    <option value="">—</option>
                    {NIVEIS.map((n) => (
                      <option key={n} value={n}>
                        Nível {n}
                      </option>
                    ))}
                  </select>
                </Field>
                <Field label="Coach">
                  <input className={INPUT_CLASS} {...editForm.register("coach")} />
                </Field>
                <Field label="Instagram">
                  <input className={INPUT_CLASS} {...editForm.register("instagram")} />
                </Field>
                <Field label="Facebook">
                  <input className={INPUT_CLASS} {...editForm.register("facebook")} />
                </Field>
                <Field label="Fundação">
                  <input className={INPUT_CLASS} {...editForm.register("fundacao")} />
                </Field>
                <Field label="Status" error={editForm.formState.errors.status?.message}>
                  <select className={INPUT_CLASS} {...editForm.register("status")}>
                    {STATUSES.map((s) => (
                      <option key={s} value={s}>
                        {s}
                      </option>
                    ))}
                  </select>
                </Field>
              </div>

              <div className="mt-6 flex gap-2">
                <button
                  type="submit"
                  className="flex-1 rounded-full bg-primary py-2.5 text-sm font-semibold text-primary-foreground hover:opacity-90"
                >
                  <Save className="mr-2 inline h-4 w-4" /> Salvar
                </button>
                <button
                  type="button"
                  onClick={() => setIsEditing(false)}
                  className="rounded-full bg-secondary px-5 py-2.5 text-sm font-semibold hover:bg-secondary/80"
                >
                  Cancelar
                </button>
              </div>
            </form>
          )}
        </div>

        <div>
          <div className="mb-6 flex items-center justify-between">
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
              Nenhum campeonato registrado. O score da equipe no Ranking ProCheer será 0 até que
              resultados sejam adicionados.
            </div>
          ) : (
            <div className="overflow-hidden rounded-2xl border border-border bg-card">
              <table className="w-full text-left text-sm">
                <thead className="border-b border-border bg-secondary/60 text-xs uppercase tracking-wider text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3">Ano</th>
                    <th className="px-4 py-3">Campeonato</th>
                    <th className="px-4 py-3">Importância</th>
                    <th className="px-4 py-3">Nível</th>
                    <th className="px-4 py-3">Categoria</th>
                    <th className="px-4 py-3 text-center">Posição</th>
                    <th className="px-4 py-3 text-center">Ações</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {results.map((r) => (
                    <tr key={r.id} className="transition hover:bg-secondary/40">
                      <td className="px-4 py-3 font-semibold">{r.ano}</td>
                      <td className="px-4 py-3">{r.nomeCampeonato}</td>
                      <td className="px-4 py-3 text-muted-foreground">{r.importancia}</td>
                      <td className="px-4 py-3">N{r.nivel}</td>
                      <td className="px-4 py-3">{r.tipoCategoria}</td>
                      <td className="px-4 py-3 text-center font-display text-lg text-primary">
                        {r.colocacao}º
                      </td>
                      <td className="px-4 py-3 text-center">
                        <div className="flex items-center justify-center gap-1">
                          <button
                            type="button"
                            onClick={() => setEditingResult(r)}
                            className="rounded-full p-1.5 text-muted-foreground hover:bg-secondary hover:text-foreground"
                            title="Editar"
                          >
                            <Edit className="h-4 w-4" />
                          </button>
                          <button
                            type="button"
                            onClick={() => handleDeleteResult(r.id)}
                            className="rounded-full p-1.5 text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
                            title="Excluir"
                          >
                            <Trash2 className="h-4 w-4" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {openResultModal && (
        <ResultForm onClose={() => setOpenResultModal(false)} onSubmit={handleAddResult} />
      )}

      {editingResult && (
        <ResultForm
          initial={{
            ano: editingResult.ano,
            nomeCampeonato: editingResult.nomeCampeonato,
            importancia: editingResult.importancia,
            nivel: editingResult.nivel,
            tipoCategoria: editingResult.tipoCategoria,
            colocacao: editingResult.colocacao,
            championshipId: editingResult.championshipId ?? null,
          }}
          onClose={() => setEditingResult(null)}
          onSubmit={handleUpdateResult}
        />
      )}
    </main>
  );
}
