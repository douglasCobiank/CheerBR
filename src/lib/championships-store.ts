import { useState, useCallback, useEffect } from "react";
import type { Championship } from "./types";
import { api } from "./api";

const STORAGE_KEY = "cheerbr_championships";

function loadLocal(): Championship[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : [];
  } catch {
    return [];
  }
}

function saveLocal(list: Championship[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(list));
}

function generateId() {
  return `local_${Date.now()}_${Math.random().toString(36).slice(2, 8)}`;
}

// Tenta API primeiro, fallback para localStorage
export function useChampionships() {
  const [championships, setChampionships] = useState<Championship[]>(loadLocal);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    api
      .getChampionships()
      .then((data) => {
        setChampionships(data);
        setIsLoading(false);
      })
      .catch(() => {
        setChampionships(loadLocal());
        setIsLoading(false);
      });
  }, []);

  const createChampionship = useCallback(async (nome: string): Promise<Championship> => {
    try {
      const created = await api.createChampionship(nome);
      setChampionships((prev) => [...prev, created]);
      return created;
    } catch {
      const local: Championship = { id: generateId(), nome };
      setChampionships((prev) => {
        const next = [...prev, local];
        saveLocal(next);
        return next;
      });
      return local;
    }
  }, []);

  const deleteChampionship = useCallback(async (id: string) => {
    try {
      await api.deleteChampionship(id);
    } catch {
      // local-only fallback
    }
    setChampionships((prev) => {
      const next = prev.filter((c) => c.id !== id);
      saveLocal(next);
      return next;
    });
  }, []);

  return {
    championships,
    isLoading,
    createChampionship,
    deleteChampionship,
  };
}
