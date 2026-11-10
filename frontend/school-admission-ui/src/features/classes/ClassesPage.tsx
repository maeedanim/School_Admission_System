
import { useEffect, useState } from "react";
import {
  GraduationCap,
  BookOpen,
  CheckCircle2,
  AlertCircle,
  X,
} from "lucide-react";
import PageLayout from "../../components/layout/PageLayout";
import StudentIdentity from "../../components/layout/StudentIdentity";
import ClassList from "./ClassList";
import { createPortal } from "react-dom";
import { useStudentId } from "../../hooks/useStudentId";
import { useAdmissionMutations } from "../admissions/useAdmissionMutations";
import { getApiErrorMessage } from "../../utils/getApiErrorMessage";

interface ToastNotification {
  type: "success" | "error";
  message: string;
}

const ClassesPage = () => {
  const { studentId, setStudentId, isValid } = useStudentId();
  const { createHoldMutation } = useAdmissionMutations();

  const [toast, setToast] = useState<ToastNotification | null>(null);

  useEffect(() => {
    if (!toast) return;

    const timeoutId = window.setTimeout(() => {
      setToast(null);
    }, 5000);

    return () => window.clearTimeout(timeoutId);
  }, [toast]);

  const handleApply = (classId: string) => {
    if (!isValid || createHoldMutation.isPending) return;

    createHoldMutation.mutate(
      { classId, studentId },
      {
        onSuccess: () => {
          setToast({
            type: "success",
            message: "Your seat has been held successfully.",
          });
        },
        onError: (error) => {
          setToast({
            type: "error",
            message: getApiErrorMessage(error),
          });
        },
      }
    );
  };

  return (
    <PageLayout>
      <section className="hero-section">
        <div className="hero-content">
          <span className="hero-label">
            <GraduationCap size={16} />
            SCHOOL ADMISSION 2026
          </span>

          <h1>
            Your Journey to
            <span> Excellence Starts Here.</span>
          </h1>

          <p>
            Explore available classes, reserve your seat, and complete
            your school admission through a simple online process.
          </p>

          <a href="#available-classes" className="hero-button">
            Explore Classes
            <BookOpen size={18} />
          </a>
        </div>

        <div className="hero-decoration">
          <GraduationCap size={150} strokeWidth={1} />
        </div>
      </section>

      <section className="dashboard-section" id="student">
        <div className="section-heading">
          <div>
            <span className="section-eyebrow">STEP 01</span>
            <h2>Identify Yourself</h2>
            <p>
              Enter your student identification details to continue.
            </p>
          </div>
        </div>

        <StudentIdentity
          studentId={studentId}
          onStudentIdChange={setStudentId}
          isValid={isValid}
        />
      </section>

      <section
        className="dashboard-section"
        id="available-classes"
      >
        <div className="section-heading">
          <div>
            <span className="section-eyebrow">STEP 02</span>
            <h2>Available Admission Classes</h2>
            <p>
              Choose a class to reserve your admission seat.
              Availability updates automatically.
            </p>
          </div>
        </div>

        {/* Page-relative notification */}

{toast &&
  createPortal(
    <div
      className={`toast-notification toast-${toast.type}`}
      role={toast.type === "error" ? "alert" : "status"}
      aria-live={toast.type === "error" ? "assertive" : "polite"}
    >
      <div className="toast-icon">
        {toast.type === "success" ? (
          <CheckCircle2 size={22} />
        ) : (
          <AlertCircle size={22} />
        )}
      </div>

      <div className="toast-content">
        <strong>
          {toast.type === "success"
            ? "Application submitted successfully"
            : "Application unsuccessful"}
        </strong>

        <p>{toast.message}</p>
      </div>

      <button
        type="button"
        className="toast-close"
        onClick={() => setToast(null)}
        aria-label="Dismiss notification"
      >
        <X size={18} />
      </button>
    </div>,
    document.body
  )}


        <ClassList
          onApply={handleApply}
          disabled={!isValid || createHoldMutation.isPending}
        />

        {createHoldMutation.isPending && (
          <p className="selection-message" role="status">
            Reserving your seat...
          </p>
        )}
      </section>
    </PageLayout>
  );
};

export default ClassesPage;
