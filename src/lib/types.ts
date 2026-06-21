export type Team = {
  id: string;
  nome: string;
  programa: string | null;
  nivel: number | null;
  cidade: string;
  estado: string;
  categoria: string;
  instagram: string | null;
  facebook: string | null;
  coach: string | null;
  fundacao: string | null;
  status: string;
  logoUrl: string | null;
  score: number;
};

export type TeamFormData = {
  nome: string;
  programa: string | null;
  nivel: number | null;
  cidade: string;
  estado: string;
  categoria: string;
  instagram: string | null;
  facebook: string | null;
  coach: string | null;
  fundacao: string | null;
  status: string;
  logoUrl: string | null;
  score: number;
};

export type CompetitionResult = {
  id: string;
  ano: number;
  nomeCampeonato: string;
  importancia: string;
  nivel: number;
  tipoCategoria: string;
  colocacao: number;
};
