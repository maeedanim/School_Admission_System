import { Users, CheckCircle2, Clock, Armchair } from "lucide-react";
import type { AdmissionClass } from "../../types/class";

interface ClassCardProps {
  admissionClass: AdmissionClass;
  onApply: (classId: string) => void;
  disabled?: boolean;
}

const ClassCard = ({
  admissionClass,
  onApply,
  disabled = false,
}: ClassCardProps) => {
  const {
    id,
    name,
    totalSeats,
    heldSeats,
    confirmedSeats,
    availableSeats,
  } = admissionClass;

  const isFull = availableSeats <= 0;

  return (
    <article className="class-card">
      <div className="class-card-header">
        <div className="class-icon">
          <Users size={22} />
        </div>

        <span className={`availability-badge ${isFull ? "full" : "available"}`}>
          {isFull ? "Full" : "Available"}
        </span>
      </div>

      <h3>{name}</h3>

      <p className="total-seats">
        {totalSeats} total seats
      </p>

      <div className="seat-statistics">
        <div className="seat-stat">
          <Clock size={17} />
          <span>Held</span>
          <strong>{heldSeats}</strong>
        </div>

        <div className="seat-stat">
          <CheckCircle2 size={17} />
          <span>Confirmed</span>
          <strong>{confirmedSeats}</strong>
        </div>

        <div className="seat-stat">
          <Armchair size={17} />
          <span>Available</span>
          <strong>{availableSeats}</strong>
        </div>
      </div>

      <div className="seat-progress">
        <div
          className="seat-progress-bar"
          style={{
            width: `${Math.min(
              100,
              ((heldSeats + confirmedSeats) / totalSeats) * 100
            )}%`,
          }}
        />
      </div>

      <button
        type="button"
        className="apply-button"
        disabled={disabled || isFull}
        onClick={() => onApply(id)}
      >
        {isFull ? "No Seats Available" : "Apply for Admission"}
      </button>
    </article>
  );
};

export default ClassCard;