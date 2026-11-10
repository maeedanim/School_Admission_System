import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  createHold,
  confirmHold,
  cancelHold,
} from "../../services/admission.service";

interface CreateHoldVariables {
  classId: string;
  studentId: string;
}

interface HoldActionVariables {
  holdId: string;
  studentId: string;
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
    mutationFn: ({ classId, studentId }: CreateHoldVariables) =>
      createHold(classId, studentId),
    onSuccess: refreshAdmissionData,
  });

  const confirmHoldMutation = useMutation({
    mutationFn: ({ holdId, studentId }: HoldActionVariables) =>
      confirmHold(holdId, studentId),
    onSuccess: refreshAdmissionData,
  });

  const cancelHoldMutation = useMutation({
    mutationFn: ({ holdId, studentId }: HoldActionVariables) =>
      cancelHold(holdId, studentId),
    onSuccess: refreshAdmissionData,
  });

  return {
    createHoldMutation,
    confirmHoldMutation,
    cancelHoldMutation,
  };
};