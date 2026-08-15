import { useQuery } from "@tanstack/react-query";
import api from "../services/api";

export function useSetTypes(){
    const { data = [] } = useQuery({
        queryKey: ["setTypes"],
        queryFn: () => api.get("/sets/types").then((res) => res.data),
        staleTime: Infinity,
    });
    return data;
}