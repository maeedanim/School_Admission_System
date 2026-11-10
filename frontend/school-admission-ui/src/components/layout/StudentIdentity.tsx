import { useState } from "react";
import { ShieldCheck, UserRound } from "lucide-react";

interface StudentIdentityProps {
  studentId: string;
  onStudentIdChange: (value: string) => void;
  isValid: boolean;
}

const StudentIdentity = ({
  studentId,
  onStudentIdChange,
  isValid,
}: StudentIdentityProps) => {
  const [editing, setEditing] = useState(!isValid);

  const handleChange = (value: string) => {
    onStudentIdChange(value.replace(/[^0-9]/g, "").slice(0, 17));
  };

  return (
    <section className="student-identity">
      <div className="identity-heading">
        <div className="identity-icon">
          <UserRound size={22} />
        </div>

        <div>
          <h2>Student Identification</h2>
          <p>Enter your 17-digit birth registration number.</p>
        </div>
      </div>

      {editing ? (
        <div className="identity-form">
          <input
            type="text"
            inputMode="numeric"
            maxLength={17}
            placeholder="Enter 17-digit birth registration number"
            value={studentId}
            onChange={(event) => handleChange(event.target.value)}
          />

          <button
            type="button"
            disabled={!isValid}
            onClick={() => setEditing(false)}
          >
            Save ID
          </button>
        </div>
      ) : (
        <div className="identity-saved">
          <div>
            <ShieldCheck size={20} />
            <span>Student ID saved</span>
          </div>

          <span className="masked-id">
            {studentId.slice(0, 4)}{"*".repeat(9)}{studentId.slice(-4)}
          </span>

          <button type="button" onClick={() => setEditing(true)}>
            Change ID
          </button>
        </div>
      )}

      {editing && studentId.length > 0 && !isValid && (
        <p className="input-error">
          Student ID must contain exactly 17 digits.
        </p>
      )}
    </section>
  );
};

export default StudentIdentity;