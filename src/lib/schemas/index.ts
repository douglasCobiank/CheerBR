import { z } from "zod";
import { CATEGORIAS, STATUSES, IMPORTANCIAS, TIPOS_CATEGORIA, NIVEL_MAX } from "@/lib/constants";

export const teamSchema = z.object({
  nome: z.string().min(1, "Nome é obrigatório").max(150),
  programa: z.string().max(150).optional(),
  nivel: z.coerce.number().int().min(1).max(NIVEL_MAX).optional().nullable(),
  cidade: z.string().min(1, "Cidade é obrigatória").max(100),
  estado: z.string().length(2, "Estado deve ter 2 caracteres"),
  categoria: z.string().min(1, "Categoria é obrigatória"),
  instagram: z.string().max(100).optional(),
  facebook: z.string().max(100).optional(),
  coach: z.string().max(150).optional(),
  fundacao: z.string().max(20).optional(),
  status: z.string().min(1, "Status é obrigatório"),
  logoUrl: z.string().nullable().optional(),
});

export type TeamFormValues = z.infer<typeof teamSchema>;

export const resultSchema = z.object({
  ano: z.coerce.number().int().min(1900, "Ano inválido").max(2100, "Ano inválido"),
  nomeCampeonato: z.string().min(1, "Campeonato é obrigatório").max(200),
  importancia: z.string().min(1, "Importância é obrigatória"),
  nivel: z.coerce.number().int().min(1).max(NIVEL_MAX),
  tipoCategoria: z.string().min(1, "Tipo de categoria é obrigatório"),
  colocacao: z.coerce.number().int().min(1, "Colocação deve ser positiva"),
  championshipId: z.string().nullable().optional(),
});

export type ResultFormValues = z.infer<typeof resultSchema>;
