import { useQuery } from "@tanstack/react-query";
import { getClasses } from "../../services/admission.service";

export const useClasses = () => {
  return useQuery({
    queryKey: ["classes"],
    queryFn: getClasses,
    refetchInterval: 5000,
    refetchIntervalInBackground: false,
  });
};