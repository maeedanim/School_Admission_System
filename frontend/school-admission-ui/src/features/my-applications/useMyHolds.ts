import { useQuery } from "@tanstack/react-query";
import { getMyHolds } from "../../services/admission.service";

export const useMyHolds = (studentId: string) => {
  return useQuery({
    queryKey: ["my-holds", studentId],
    queryFn: () => getMyHolds(studentId),
    enabled: /^[0-9]{17}$/.test(studentId),
    refetchInterval: 5000,
    refetchIntervalInBackground: false,
  });
};