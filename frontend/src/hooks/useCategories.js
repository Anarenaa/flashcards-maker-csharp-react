import { useQuery } from "@tanstack/react-query";
import api from "../services/api";

export function useCategories(){
  const { data = [] } = useQuery({
    queryKey: ["categories"],
    queryFn: () => api.get("/categories").then((res) => res.data),
    staleTime: 5 * 60 * 1000,
  });

  return data;
}