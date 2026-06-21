import { createFileRoute } from "@tanstack/react-router";
import { useMemo, useState } from "react";
import { useTeams } from "@/lib/teams-store";
import { Trophy, Medal, Award } from "lucide-react";
import { CATEGORIAS } from "@/lib/constants";

export const Route = createFileRoute("/ranking")({
  head: () => ({
    meta: [
      { title: "Ranking — Cheer BR" },
      {
        name: "description",
        content:
          "Ranking estilo FIFA das equipes de cheerleading do Paraná baseado em nível, status, programa e presença digital.",
      },
    ],
  }),
  component: RankingPage,
});

function RankingPage() {
  const { teams = [] } = useTeams();
  const [cat, setCat] = useState<string>("");

  const ranked = useMemo(() => {
    const list = cat ? teams.filter((t) => t.categoria === cat) : teams;
    return [...list].sort((a, b) => b.score - a.score);
  }, [teams, cat]);

  const podium = ranked.slice(0, 3);
  const rest = ranked.slice(3);

  return (
    <main className="mx-auto max-w-6xl px-4 py-10 sm:px-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="font-display text-5xl">Ranking Cheer BR</h1>
          <p className="max-w-xl text-sm text-muted-foreground">
            Score calculado a partir de nível técnico, status, programa, presença digital e
            categoria — inspirado no overall da FIFA.
          </p>
        </div>
        <select
          value={cat}
          onChange={(e) => setCat(e.target.value)}
          className="rounded-full border border-border bg-input/60 px-4 py-2.5 text-sm outline-none focus:border-primary"
        >
          <option value="">Todas as categorias</option>
          {CATEGORIAS.filter((c) => c !== "Outro").map((c) => (
            <option key={c}>{c}</option>
          ))}
        </select>
      </div>

      {/* Podium */}
      <section className="mt-10 grid gap-4 sm:grid-cols-3">
        {podium.map((t, i) => {
          const Icon = i === 0 ? Trophy : i === 1 ? Medal : Award;
          const color =
            i === 0
              ? "text-[var(--gold)] border-[var(--gold)]/40"
              : i === 1
                ? "text-[var(--silver)] border-[var(--silver)]/40"
                : "text-[var(--bronze)] border-[var(--bronze)]/40";
          return (
            <div
              key={t.id}
              className={`relative overflow-hidden rounded-2xl border bg-[image:var(--gradient-card)] p-6 ${color} ${i === 0 ? "sm:-mt-4 sm:row-span-2" : ""}`}
            >
              <Icon className="h-8 w-8" />
              <div className="mt-3 font-display text-7xl text-foreground">#{i + 1}</div>
              <h3 className="mt-1 font-display text-3xl text-foreground">{t.nome}</h3>
              <p className="text-sm text-muted-foreground">
                {t.cidade} · {t.categoria}
              </p>
              <div className="mt-4 inline-flex items-baseline gap-1">
                <span className="font-display text-5xl text-primary">{t.score}</span>
                <span className="text-xs uppercase tracking-wider text-muted-foreground">
                  overall
                </span>
              </div>
            </div>
          );
        })}
      </section>

      {/* Como funciona */}
      <section className="mt-12 rounded-2xl border border-border/60 bg-card p-6">
        <h2 className="font-display text-2xl">Como o score é calculado?</h2>
        <p className="mt-2 text-sm text-muted-foreground">
          O score de cada equipe é a soma de pontos de todos os resultados registrados. Cada
          resultado é calculado com a fórmula:
        </p>
        <div className="mt-3 rounded-lg bg-secondary/40 px-4 py-3 font-mono text-xs leading-relaxed text-muted-foreground">
          pontos = colocacao × importancia × nivel × tipo_categoria × peso_ano
        </div>
        <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <div>
            <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
              Colocação
            </h3>
            <table className="mt-1 w-full text-xs">
              <tbody>
                <tr>
                  <td className="pr-3 text-muted-foreground">1º</td>
                  <td>100 pts</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">2º</td>
                  <td>70 pts</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">3º</td>
                  <td>50 pts</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">4º</td>
                  <td>30 pts</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">5º</td>
                  <td>20 pts</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">6º+</td>
                  <td>10 pts</td>
                </tr>
              </tbody>
            </table>
          </div>
          <div>
            <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
              Importância
            </h3>
            <table className="mt-1 w-full text-xs">
              <tbody>
                <tr>
                  <td className="pr-3 text-muted-foreground">Internacional</td>
                  <td>×3.0</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">Nacional</td>
                  <td>×2.5</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">Estadual</td>
                  <td>×2.0</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">Regional</td>
                  <td>×1.7</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">Municipal</td>
                  <td>×1.5</td>
                </tr>
              </tbody>
            </table>
          </div>
          <div>
            <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
              Nível
            </h3>
            <table className="mt-1 w-full text-xs">
              <tbody>
                <tr>
                  <td className="pr-3 text-muted-foreground">Nível 1</td>
                  <td>×1.1</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">Nível 2</td>
                  <td>×1.2</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">Nível 3</td>
                  <td>×1.3</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">Nível 4</td>
                  <td>×1.4</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">Nível 5</td>
                  <td>×1.5</td>
                </tr>
              </tbody>
            </table>
          </div>
          <div>
            <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
              Tipo de Categoria
            </h3>
            <table className="mt-1 w-full text-xs">
              <tbody>
                <tr>
                  <td className="pr-3 text-muted-foreground">Team Cheer</td>
                  <td>×1.5</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">Grupo</td>
                  <td>×1.2</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">Partner/Duplas</td>
                  <td>×1.1</td>
                </tr>
                <tr>
                  <td className="pr-3 text-muted-foreground">Skills Individuais</td>
                  <td>×0.9</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <div className="mt-4 text-xs text-muted-foreground">
          <strong>Peso do ano:</strong> resultados perdem 10% do valor a cada ano desde o
          campeonato. Ex: um resultado de 2024 vale 90% em 2025, 80% em 2026, e assim por diante.
        </div>
      </section>

      {/* Table */}
      <section className="mt-12 overflow-hidden rounded-2xl border border-border/60 bg-card">
        <table className="w-full text-sm">
          <thead className="bg-secondary/60 text-left text-xs uppercase tracking-wider text-muted-foreground">
            <tr>
              <th className="px-4 py-3">#</th>
              <th className="px-4 py-3">Equipe</th>
              <th className="px-4 py-3 hidden md:table-cell">Cidade</th>
              <th className="px-4 py-3 hidden sm:table-cell">Categoria</th>
              <th className="px-4 py-3 text-center">Nível</th>
              <th className="px-4 py-3 text-right">Overall</th>
            </tr>
          </thead>
          <tbody>
            {rest.map((t, i) => (
              <tr key={t.id} className="border-t border-border/60 transition hover:bg-secondary/40">
                <td className="px-4 py-3 font-display text-lg text-muted-foreground">{i + 4}</td>
                <td className="px-4 py-3 font-medium">{t.nome}</td>
                <td className="px-4 py-3 text-muted-foreground hidden md:table-cell">{t.cidade}</td>
                <td className="px-4 py-3 hidden sm:table-cell">
                  <span className="rounded-full bg-secondary px-2 py-0.5 text-[10px] uppercase tracking-wider">
                    {t.categoria}
                  </span>
                </td>
                <td className="px-4 py-3 text-center">{t.nivel ?? "—"}</td>
                <td className="px-4 py-3 text-right font-display text-xl text-primary">
                  {t.score}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </main>
  );
}
