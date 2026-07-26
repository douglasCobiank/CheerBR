import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { Field } from "@/components/ui/field";
import { Modal } from "@/components/modal";
import { INPUT_CLASS, CATEGORIAS, STATUSES } from "@/lib/constants";
import { teamSchema, type TeamFormValues } from "@/lib/schemas";

export function AddTeamDialog({
  onClose,
  onSubmit,
  isLoading,
}: {
  onClose: () => void;
  onSubmit: (data: TeamFormValues) => Promise<void>;
  isLoading?: boolean;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<TeamFormValues>({
    resolver: zodResolver(teamSchema),
    defaultValues: {
      nome: "",
      cidade: "",
      nivel: 2,
      categoria: "Universitário",
      estado: "PR",
      programa: "",
      coach: "",
      instagram: "",
      facebook: "",
      fundacao: "",
      status: "Ativo",
      logoUrl: null,
    },
  });

  const onFormSubmit = async (data: TeamFormValues) => {
    try {
      await onSubmit(data);
      toast.success("Equipe criada com sucesso");
      onClose();
    } catch {
      toast.error("Erro ao criar equipe");
    }
  };

  return (
    <Modal open onClose={onClose}>
      <form
        onSubmit={handleSubmit(onFormSubmit)}
        className="w-full max-w-xl rounded-2xl border border-border bg-card p-6"
      >
        <h2 className="font-display text-3xl">Nova equipe</h2>

        <div className="mt-4 grid gap-3 sm:grid-cols-2">
          <Field label="Nome" error={errors.nome?.message}>
            <input className={INPUT_CLASS} {...register("nome")} />
          </Field>

          <Field label="Cidade" error={errors.cidade?.message}>
            <input className={INPUT_CLASS} {...register("cidade")} />
          </Field>

          <Field label="Estado" error={errors.estado?.message}>
            <input className={INPUT_CLASS} {...register("estado")} />
          </Field>

          <Field label="Categoria" error={errors.categoria?.message}>
            <select className={INPUT_CLASS} {...register("categoria")}>
              {CATEGORIAS.map((c) => (
                <option key={c}>{c}</option>
              ))}
            </select>
          </Field>

          <Field label="Programa / Ginásio">
            <input className={INPUT_CLASS} {...register("programa")} />
          </Field>

          <Field label="Nível" error={errors.nivel?.message}>
            <input
              type="number"
              min="1"
              max="6"
              className={INPUT_CLASS}
              {...register("nivel", { valueAsNumber: true })}
            />
          </Field>

          <Field label="Coach">
            <input className={INPUT_CLASS} {...register("coach")} />
          </Field>

          <Field label="Instagram">
            <input className={INPUT_CLASS} {...register("instagram")} />
          </Field>

          <Field label="Facebook">
            <input className={INPUT_CLASS} {...register("facebook")} />
          </Field>

          <Field label="Fundação">
            <input className={INPUT_CLASS} {...register("fundacao")} />
          </Field>

          <Field label="Status" error={errors.status?.message}>
            <select className={INPUT_CLASS} {...register("status")}>
              {STATUSES.map((s) => (
                <option key={s}>{s}</option>
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
