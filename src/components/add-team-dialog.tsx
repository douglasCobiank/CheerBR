import { useState } from "react";
import { toast } from "sonner";
import { z } from "zod";
import { Field } from "@/components/ui/field";
import { Modal } from "@/components/modal";
import { INPUT_CLASS, CATEGORIAS } from "@/lib/constants";

const schema = z.object({
  nome: z.string().min(2),
  cidade: z.string().min(2),
  nivel: z.number().min(1).max(6),
  categoria: z.string(),
  estado: z.string(),
  programa: z.string().nullable(),
  coach: z.string().nullable(),
  instagram: z.string().nullable(),
  facebook: z.string().nullable(),
  fundacao: z.string().nullable(),
  logoUrl: z.string().nullable(),
  status: z.string(),
});

type FormData = z.infer<typeof schema>;

export function AddTeamDialog({
  onClose,
  onSubmit,
  isLoading,
}: {
  onClose: () => void;
  onSubmit: (data: FormData) => Promise<void>;
  isLoading?: boolean;
}) {
  const [form, setForm] = useState<FormData>({
    nome: "",
    cidade: "",
    nivel: 2,
    categoria: "Universitário",
    estado: "PR",
    programa: null,
    coach: null,
    instagram: null,
    facebook: null,
    fundacao: null,
    logoUrl: null,
    status: "Ativo",
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const set = <K extends keyof FormData>(k: K, v: FormData[K]) =>
    setForm((f) => ({ ...f, [k]: v }));

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    const parsed = schema.safeParse(form);

    if (!parsed.success) {
      const err: Record<string, string> = {};
      parsed.error.errors.forEach((e) => {
        if (e.path[0]) err[e.path[0] as string] = e.message;
      });

      setErrors(err);
      toast.error("Preencha os campos obrigatórios");
      return;
    }

    try {
      await onSubmit(parsed.data);
      toast.success("Equipe criada com sucesso 🚀");
      onClose();
    } catch {
      toast.error("Erro ao criar equipe");
    }
  }

  return (
    <Modal open onClose={onClose}>
      <form
        onSubmit={handleSubmit}
        className="w-full max-w-xl rounded-2xl border border-border bg-card p-6"
      >
        <h2 className="font-display text-3xl">Nova equipe</h2>

        <div className="mt-4 grid gap-3 sm:grid-cols-2">
          <Field label="Nome" error={errors.nome}>
            <input
              className={INPUT_CLASS}
              value={form.nome}
              onChange={(e) => set("nome", e.target.value)}
            />
          </Field>

          <Field label="Cidade" error={errors.cidade}>
            <input
              className={INPUT_CLASS}
              value={form.cidade}
              onChange={(e) => set("cidade", e.target.value)}
            />
          </Field>

          <Field label="Nível">
            <input
              type="number"
              className={INPUT_CLASS}
              value={form.nivel}
              onChange={(e) => set("nivel", Number(e.target.value))}
            />
          </Field>

          <Field label="Categoria">
            <select
              className={INPUT_CLASS}
              value={form.categoria}
              onChange={(e) => set("categoria", e.target.value)}
            >
              {CATEGORIAS.map((c) => (
                <option key={c}>{c}</option>
              ))}
            </select>
          </Field>
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
            disabled={isLoading}
            className="rounded-full bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground disabled:opacity-50"
          >
            {isLoading ? "Salvando..." : "Criar"}
          </button>
        </div>
      </form>
    </Modal>
  );
}
