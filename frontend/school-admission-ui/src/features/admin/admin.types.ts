
export type ApplicationStatus =
  | "HELD"
  | "CONFIRMED"
  | "EXPIRED"
  | "CANCELLED";

export interface AdminApplication {
  applicationId: string;
  studentName: string | null;
  studentEmail: string | null;
  studentId: string;
  classId: string;
  className: string;
  status: ApplicationStatus;
  createdAt: string;
  expiresAt: string;
  confirmedAt: string | null;
  cancelledAt: string | null;
}
