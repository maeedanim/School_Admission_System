
import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  createHold,
  confirmHold,
  cancelHold,
} from "../../services/admission.service";

interface CreateHoldVariables {
  classId: string;
}

interface HoldActionVariables {
  holdId: string;
}

export const useAdmissionMutations = () => {
  const queryClient = useQueryClient();

  const refreshAdmissionData = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ["classes"] }),
      queryClient.invalidateQueries({ queryKey: ["my-holds"] }),
    ]);
  };

  const createHoldMutation = useMutation({
    mutationFn: ({ classId }: CreateHoldVariables) =>
      createHold(classId),
    onSuccess: refreshAdmissionData,
  });

  const confirmHoldMutation = useMutation({
    mutationFn: ({ holdId }: HoldActionVariables) =>
      confirmHold(holdId),
    onSuccess: refreshAdmissionData,
  });

  const cancelHoldMutation = useMutation({
    mutationFn: ({ holdId }: HoldActionVariables) =>
      cancelHold(holdId),
    onSuccess: refreshAdmissionData,
  });

  return {
    createHoldMutation,
    confirmHoldMutation,
    cancelHoldMutation,
  };
};
