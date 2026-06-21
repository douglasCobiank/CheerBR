import { useState } from "react";
import { toast } from "sonner";
import { z } from "zod";

const schema = z.object({
  nome: z.string().min(2),
  cidade: z.string().min(2),
  nivel: z.number().min(1).max(6),
  categoria: z.string(),
  estado: z.string(),
  programa: z.string().nullable().optional(),
  coach: z.string().nullable().optional(),
  instagram: z.string().nullable().optional(),
  facebook: z.string().nullable().optional(),
  fundacao: z.string().nullable().optional(),
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
    status: "Ativo",
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const set = (k: keyof FormData, v: any) =>
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
    <div
      className="fixed inset-0 z-50 grid place-items-center bg-background/80 p-4 backdrop-blur"
      onClick={onClose}
    >
      <form
        onClick={(e) => e.stopPropagation()}
        onSubmit={handleSubmit}
        className="w-full max-w-xl rounded-2xl border border-border bg-card p-6"
      >
        <h2 className="font-display text-3xl">Nova equipe</h2>

        <div className="mt-4 grid gap-3 sm:grid-cols-2">
          <Field label="Nome" error={errors.nome}>
            <input
              className={input}
              value={form.nome}
              onChange={(e) => set("nome", e.target.value)}
            />
          </Field>

          <Field label="Cidade" error={errors.cidade}>
            <input
              className={input}
              value={form.cidade}
              onChange={(e) => set("cidade", e.target.value)}
            />
          </Field>

          <Field label="Nível">
            <input
              type="number"
              className={input}
              value={form.nivel}
              onChange={(e) => set("nivel", Number(e.target.value))}
            />
          </Field>

          <Field label="Categoria">
            <select
              className={input}
              value={form.categoria}
              onChange={(e) => set("categoria", e.target.value)}
            >
              <option>Universitário</option>
              <option>All star</option>
              <option>Escolar</option>
              <option>Outro</option>
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
    </div>
  );
}

function Field({
  label,
  error,
  children,
}: {
  label: string;
  error?: string;
  children: React.ReactNode;
}) {
  return (
    <label className="block">
      <span className="text-xs uppercase text-muted-foreground">
        {label}
      </span>
      {children}
      {error && <p className="text-xs text-red-400 mt-1">{error}</p>}
    </label>
  );
}

const input =
  "w-full rounded-lg border border-border bg-input/60 px-3 py-2 text-sm outline-none focus:border-primary";