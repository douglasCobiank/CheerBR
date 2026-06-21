import { useState } from "react";
import { Field } from "@/components/ui/field";
import { Modal } from "@/components/modal";
import { X } from "lucide-react";
import { IMPORTANCIAS, TIPOS_CATEGORIA, INPUT_CLASS } from "@/lib/constants";
import { useChampionships } from "@/lib/championships-store";

export type ResultFormData = {
  ano: number;
  nomeCampeonato: string;
  importancia: string;
  nivel: number;
  tipoCategoria: string;
  colocacao: number;
};

export function ResultForm({
  onClose,
  onSubmit,
  initial,
}: {
  onClose: () => void;
  onSubmit: (data: ResultFormData) => void | Promise<void>;
  initial?: ResultFormData;
}) {
  const { championships, createChampionship } = useChampionships();
  const [form, setForm] = useState<ResultFormData>(
    initial ?? {
      ano: new Date().getFullYear(),
      nomeCampeonato: "",
      importancia: "Estadual",
      nivel: 2,
      tipoCategoria: "Team Cheer",
      colocacao: 1,
    },
  );
  const [newChamp, setNewChamp] = useState("");
  const [showNewChamp, setShowNewChamp] = useState(false);

  const set = <K extends keyof ResultFormData>(k: K, v: ResultFormData[K]) =>
    setForm((f) => ({ ...f, [k]: v }));

  const handleAddChampionship = async () => {
    if (!newChamp.trim()) return;
    await createChampionship(newChamp.trim());
    setNewChamp("");
    setShowNewChamp(false);
  };

  const filteredChamps = championships.filter((c) =>
    c.nome.toLowerCase().includes(newChamp.toLowerCase()),
  );

  return (
    <Modal open onClose={onClose}>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          if (!form.nomeCampeonato) return;
          onSubmit(form);
        }}
        className="w-full max-w-xl rounded-2xl border border-border bg-card p-6"
      >
        <div className="mb-5 flex items-center justify-between">
          <h2 className="font-display text-3xl">
            {initial ? "Editar Resultado" : "Lançar Resultado"}
          </h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-full p-1.5 text-muted-foreground hover:bg-secondary"
          >
            <X className="h-4 w-4" />
          </button>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Field label="Campeonato *" className="sm:col-span-2">
            <div className="flex gap-2">
              <select
                required
                value={form.nomeCampeonato}
                onChange={(e) => {
                  if (e.target.value === "__new__") {
                    setShowNewChamp(true);
                  } else {
                    set("nomeCampeonato", e.target.value);
                  }
                }}
                className={INPUT_CLASS}
              >
                <option value="">Selecione um campeonato</option>
                <option value="__new__">+ Cadastrar novo campeonato</option>
                {filteredChamps.map((c) => (
                  <option key={c.id} value={c.nome}>
                    {c.nome}
                  </option>
                ))}
              </select>
            </div>
            {showNewChamp && (
              <div className="mt-2 flex gap-2">
                <input
                  className={INPUT_CLASS}
                  placeholder="Nome do campeonato"
                  value={newChamp}
                  onChange={(e) => setNewChamp(e.target.value)}
                />
                <button
                  type="button"
                  onClick={handleAddChampionship}
                  className="shrink-0 rounded-full bg-primary px-3 py-1 text-xs font-semibold text-primary-foreground"
                >
                  Adicionar
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setShowNewChamp(false);
                    setNewChamp("");
                  }}
                  className="shrink-0 rounded-full bg-secondary px-3 py-1 text-xs"
                >
                  Cancelar
                </button>
              </div>
            )}
          </Field>
          <Field label="Ano *">
            <input
              type="number"
              required
              value={form.ano}
              onChange={(e) => set("ano", Number(e.target.value))}
              className={INPUT_CLASS}
            />
          </Field>
          <Field label="Importância">
            <select
              value={form.importancia}
              onChange={(e) => set("importancia", e.target.value)}
              className={INPUT_CLASS}
            >
              {IMPORTANCIAS.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </Field>
          <Field label="Tipo de Categoria">
            <select
              value={form.tipoCategoria}
              onChange={(e) => set("tipoCategoria", e.target.value)}
              className={INPUT_CLASS}
            >
              {TIPOS_CATEGORIA.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </Field>
          <Field label="Nível (1-5)">
            <input
              type="number"
              min="1"
              max="5"
              required
              value={form.nivel}
              onChange={(e) => set("nivel", Number(e.target.value))}
              className={INPUT_CLASS}
            />
          </Field>
          <Field label="Colocação Final (1 = Ouro)">
            <input
              type="number"
              min="1"
              required
              value={form.colocacao}
              onChange={(e) => set("colocacao", Number(e.target.value))}
              className={INPUT_CLASS}
            />
          </Field>
        </div>

        <div className="mt-6 rounded-xl bg-secondary/50 p-4 text-sm text-muted-foreground">
          <p>
            <strong>Nota ProCheer:</strong> O sistema calculará os pontos automaticamente usando o
            peso do ano, importância, nível e categoria.
          </p>
        </div>

        <div className="mt-6 flex justify-end gap-2">
          <button
            type="button"
            onClick={onClose}
            className="rounded-full px-5 py-2.5 text-sm text-muted-foreground hover:bg-secondary"
          >
            Cancelar
          </button>
          <button
            type="submit"
            className="rounded-full bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground hover:opacity-90"
          >
            {initial ? "Salvar Alterações" : "Salvar Resultado"}
          </button>
        </div>
      </form>
    </Modal>
  );
}
