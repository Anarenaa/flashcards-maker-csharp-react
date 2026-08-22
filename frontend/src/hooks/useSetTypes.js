import { useQuery } from "@tanstack/react-query";
import api from "../services/api";

const EMPTY_TYPES = []; // stable reference — avoids a fresh [] literal on every render while data is still loading

export function useSetTypes(){
    const { data = EMPTY_TYPES } = useQuery({
        queryKey: ["setTypes"],
        queryFn: () => api.get("/sets/types").then((res) => res.data),
        staleTime: Infinity,
    });
    return data;
}