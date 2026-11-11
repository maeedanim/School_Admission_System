
import { useQuery } from "@tanstack/react-query";

import { getAllApplications } from "../../services/admin.service";

export const useAdminApplications = () => {
  return useQuery({
    queryKey: ["admin-applications"],
    queryFn: getAllApplications,
    refetchInterval: 10000,
    refetchIntervalInBackground: false,
  });
};
