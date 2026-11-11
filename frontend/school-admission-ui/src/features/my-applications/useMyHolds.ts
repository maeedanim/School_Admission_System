
import { useQuery } from "@tanstack/react-query";
import { getMyHolds } from "../../services/admission.service";

export const useMyHolds = (userId: string | null) => {
  return useQuery({
    queryKey: ["my-holds", userId],
    queryFn: getMyHolds,
    enabled: Boolean(userId),
    refetchInterval: 5000,
    refetchIntervalInBackground: false,
  });
};
