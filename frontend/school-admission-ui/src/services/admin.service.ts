
import api from "./api";

import type { AdminApplication } from "../features/admin/admin.types";

// Retrieve all admission applications (Admin only).
export const getAllApplications = async (): Promise<
  AdminApplication[]
> => {
  const response = await api.get<AdminApplication[]>(
    "/admin/applications"
  );

  return response.data;
};
