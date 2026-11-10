import { useState } from "react";

const STORAGE_KEY = "school_admission_student_id";

export const useStudentId = () => {
  const [studentId, setStudentIdState] = useState(
    () => localStorage.getItem(STORAGE_KEY) ?? ""
  );

  const setStudentId = (value: string) => {
    setStudentIdState(value);

    if (value) {
      localStorage.setItem(STORAGE_KEY, value);
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  };

  const isValid = /^[0-9]{17}$/.test(studentId);

  return {
    studentId,
    setStudentId,
    isValid,
  };
};