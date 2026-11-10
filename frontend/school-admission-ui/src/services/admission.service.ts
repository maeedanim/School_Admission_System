import api from "./api";

import type {
  AdmissionClass,
  CreateClassRequest,
  CreateClassResponse,
} from "../types/class";

import type {
  CreateHoldResponse,
  HoldResponse,
  ConfirmHoldResponse,
  MyHold,
} from "../types/hold";

// 1. Create a new admission class
export const createClass = async (
  data: CreateClassRequest
): Promise<CreateClassResponse> => {
  const response = await api.post<CreateClassResponse>("/classes", data);
  return response.data;
};

// 2. Get all admission classes
export const getClasses = async (): Promise<AdmissionClass[]> => {
  const response = await api.get<AdmissionClass[]>("/classes");
  return response.data;
};

// 3. Get a specific admission class
export const getClassById = async (
  classId: string
): Promise<AdmissionClass> => {
  const response = await api.get<AdmissionClass>(`/classes/${classId}`);
  return response.data;
};

// 4. Create a seat hold
export const createHold = async (
  classId: string,
  studentId: string
): Promise<CreateHoldResponse> => {
  const response = await api.post<CreateHoldResponse>(
    `/classes/${classId}/holds`,
    null,
    {
      headers: {
        "X-Student-Id": studentId,
      },
    }
  );

  return response.data;
};

// 5. Get hold details
export const getHoldById = async (
  holdId: string
): Promise<HoldResponse> => {
  const response = await api.get<HoldResponse>(`/holds/${holdId}`);
  return response.data;
};

// 6. Confirm a seat hold
export const confirmHold = async (
  holdId: string,
  studentId: string
): Promise<ConfirmHoldResponse> => {
  const response = await api.post<ConfirmHoldResponse>(
    `/holds/${holdId}/confirm`,
    null,
    {
      headers: {
        "X-Student-Id": studentId,
      },
    }
  );

  return response.data;
};

// 7. Cancel a seat hold
export const cancelHold = async (
  holdId: string,
  studentId: string
): Promise<void> => {
  await api.delete(`/holds/${holdId}`, {
    headers: {
      "X-Student-Id": studentId,
    },
  });
};

// 8. Get the current student's applications
export const getMyHolds = async (
  studentId: string
): Promise<MyHold[]> => {
  const response = await api.get<MyHold[]>("/me/holds", {
    headers: {
      "X-Student-Id": studentId,
    },
  });

  return response.data;
};