import { createFileRoute } from "@tanstack/react-router";
import { useState } from "react";
import { useChampionships } from "@/lib/championships-store";
import { Plus, X } from "lucide-react";
import { INPUT_CLASS } from "@/lib/constants";

export const Route = createFileRoute("/campeonatos")({
  component: CampeonatosPage,
});

function CampeonatosPage() {
  const {
    championships = [],
    isLoading,
    createChampionship,
    deleteChampionship,
  } = useChampionships();
  const [nome, setNome] = useState("");

  const handleAdd = async () => {
    if (!nome.trim()) return;
    await createChampionship(nome.trim());
    setNome("");
  };

  if (isLoading) return <div className="p-10 text-center">Carregando...</div>;

  return (
    <main className="mx-auto max-w-3xl px-4 py-10 sm:px-6">
      <h1 className="font-display text-5xl">Campeonatos</h1>
      <p className="mt-1 text-sm text-muted-foreground">
        {championships.length} campeonato{championships.length !== 1 ? "s" : ""} cadastrado
        {championships.length !== 1 ? "s" : ""}
      </p>

      <div className="mt-6 flex gap-2">
        <input
          className={INPUT_CLASS}
          placeholder="Nome do campeonato"
          value={nome}
          onChange={(e) => setNome(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && handleAdd()}
        />
        <button
          onClick={handleAdd}
          className="inline-flex shrink-0 items-center gap-2 rounded-full bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground hover:opacity-90"
        >
          <Plus className="h-4 w-4" /> Adicionar
        </button>
      </div>

      {championships.length === 0 ? (
        <div className="mt-8 rounded-2xl border border-dashed border-border p-10 text-center text-muted-foreground">
          Nenhum campeonato cadastrado. Adicione o primeiro acima.
        </div>
      ) : (
        <div className="mt-6 divide-y divide-border overflow-hidden rounded-2xl border border-border bg-card">
          {championships.map((c) => (
            <div key={c.id} className="flex items-center justify-between px-5 py-3">
              <span className="text-sm font-medium">{c.nome}</span>
              <button
                onClick={() => deleteChampionship(c.id)}
                className="rounded-full p-1.5 text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
                title="Excluir"
              >
                <X className="h-4 w-4" />
              </button>
            </div>
          ))}
        </div>
      )}
    </main>
  );
}
