import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Field } from "@/components/ui/field";
import { Modal } from "@/components/modal";
import { X } from "lucide-react";
import { IMPORTANCIAS, TIPOS_CATEGORIA, INPUT_CLASS } from "@/lib/constants";
import { useChampionships } from "@/lib/championships-store";
import { resultSchema, type ResultFormValues } from "@/lib/schemas";

export function ResultForm({
  onClose,
  onSubmit,
  initial,
}: {
  onClose: () => void;
  onSubmit: (data: ResultFormValues) => void | Promise<void>;
  initial?: ResultFormValues;
}) {
  const { championships, createChampionship } = useChampionships();
  const [showNewChamp, setShowNewChamp] = useState(false);
  const [newChamp, setNewChamp] = useState("");

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm<ResultFormValues>({
    resolver: zodResolver(resultSchema),
    defaultValues: initial ?? {
      ano: new Date().getFullYear(),
      nomeCampeonato: "",
      importancia: "Estadual",
      nivel: 2,
      tipoCategoria: "Team Cheer",
      colocacao: 1,
      championshipId: null,
    },
  });

  const handleAddChampionship = async () => {
    if (!newChamp.trim()) return;
    try {
      const created = await createChampionship(newChamp.trim());
      setValue("nomeCampeonato", created.nome);
      setNewChamp("");
      setShowNewChamp(false);
    } catch {
      // toast error is handled in the hook
    }
  };

  return (
    <Modal open onClose={onClose}>
      <form
        onSubmit={handleSubmit(onSubmit)}
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
          <Field
            label="Campeonato *"
            error={errors.nomeCampeonato?.message}
            className="sm:col-span-2"
          >
            <div className="flex gap-2">
              <select
                className={INPUT_CLASS}
                {...register("nomeCampeonato")}
                onChange={(e) => {
                  if (e.target.value === "__new__") {
                    setShowNewChamp(true);
                  } else {
                    register("nomeCampeonato").onChange(e);
                  }
                }}
              >
                <option value="">Selecione um campeonato</option>
                <option value="__new__">+ Cadastrar novo campeonato</option>
                {championships.map((c) => (
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

          <Field label="Ano *" error={errors.ano?.message}>
            <input
              type="number"
              className={INPUT_CLASS}
              {...register("ano", { valueAsNumber: true })}
            />
          </Field>

          <Field label="Importância" error={errors.importancia?.message}>
            <select className={INPUT_CLASS} {...register("importancia")}>
              {IMPORTANCIAS.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </Field>

          <Field label="Tipo de Categoria" error={errors.tipoCategoria?.message}>
            <select className={INPUT_CLASS} {...register("tipoCategoria")}>
              {TIPOS_CATEGORIA.map((c) => (
                <option key={c} value={c}>
                  {c}
                </option>
              ))}
            </select>
          </Field>

          <Field label="Nível (1-6)" error={errors.nivel?.message}>
            <input
              type="number"
              min="1"
              max="6"
              className={INPUT_CLASS}
              {...register("nivel", { valueAsNumber: true })}
            />
          </Field>

          <Field label="Colocação Final (1 = Ouro)" error={errors.colocacao?.message}>
            <input
              type="number"
              min="1"
              className={INPUT_CLASS}
              {...register("colocacao", { valueAsNumber: true })}
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
