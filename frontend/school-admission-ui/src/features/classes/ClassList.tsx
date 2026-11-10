import { useClasses } from "./useClasses";
import ClassCard from "./ClassCard";
import { LoaderCircle, AlertCircle } from "lucide-react";

interface ClassListProps {
  onApply: (classId: string) => void;
  disabled?: boolean;
}

const ClassList = ({ onApply, disabled = false }: ClassListProps) => {
  const { data: classes, isLoading, isError, refetch } = useClasses();

  if (isLoading) {
    return (
      <div className="state-message">
        <LoaderCircle className="spin" size={24} />
        <p>Loading admission classes...</p>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="state-message error-state">
        <AlertCircle size={24} />
        <p>Unable to load admission classes.</p>
        <button type="button" onClick={() => refetch()}>
          Try Again
        </button>
      </div>
    );
  }

  if (!classes || classes.length === 0) {
    return (
      <div className="state-message">
        <p>No admission classes are currently available.</p>
      </div>
    );
  }

  return (
    <div className="class-grid">
      {classes.map((admissionClass) => (
        <ClassCard
          key={admissionClass.id}
          admissionClass={admissionClass}
          onApply={onApply}
          disabled={disabled}
        />
      ))}
    </div>
  );
};

export default ClassList;