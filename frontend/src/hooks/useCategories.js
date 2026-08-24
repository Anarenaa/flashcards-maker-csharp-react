import { useQuery } from "@tanstack/react-query";
import api from "../services/api";

const EMPTY_TYPES = []; // stable reference — avoids a fresh [] literal on every render while data is still loading

export function useCategories(){
  const { data = EMPTY_TYPES } = useQuery({
    queryKey: ["categories"],
    queryFn: () => api.get("/categories").then((res) => res.data),
    staleTime: 5 * 60 * 1000,
  });

  return data;
}