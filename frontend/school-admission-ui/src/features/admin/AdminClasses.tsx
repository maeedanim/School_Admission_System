import { useState, type FormEvent } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  Plus,
  Users,
  RefreshCw,
  CheckCircle2,
  AlertCircle,
} from "lucide-react";

import { createClass } from "../../services/admission.service";
import { useClasses } from "../classes/useClasses";
import { getApiErrorMessage } from "../../utils/getApiErrorMessage";

const AdminClasses = () => {
  const queryClient = useQueryClient();

  const [name, setName] = useState("");
  const [totalSeats, setTotalSeats] = useState("30");
  const [successMessage, setSuccessMessage] = useState("");

  const {
    data: classes = [],
    isLoading,
    isError,
    isFetching,
    refetch,
  } = useClasses();

  const createClassMutation = useMutation({
    mutationFn: createClass,

    onSuccess: async (createdClass) => {
      await queryClient.invalidateQueries({
        queryKey: ["classes"],
      });

      setName("");
      setTotalSeats("30");
      setSuccessMessage(
        `${createdClass.name} was created successfully.`
      );
    },

    onError: () => {
      setSuccessMessage("");
    },
  });

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSuccessMessage("");

    const trimmedName = name.trim();
    const seats = Number(totalSeats);

    if (!trimmedName) {
      return;
    }

    if (
      !Number.isSafeInteger(seats) ||
      seats < 1 ||
      seats > 2147483647
    ) {
      return;
    }

    createClassMutation.mutate({
      name: trimmedName,
      totalSeats: seats,
    });
  };

  return (
    <div className="admin-classes">
      <section className="admin-class-create-card">
        <div className="admin-class-card-heading">
          <div className="admin-class-icon">
            <Plus size={21} />
          </div>

          <div>
            <h3>Create a New Class</h3>
            <p>
              Add an admission class and define its available seat capacity.
            </p>
          </div>
        </div>

        <form
          className="admin-class-form"
          onSubmit={handleSubmit}
        >
          <div className="admin-class-form-fields">
            <div className="admin-class-form-group">
              <label htmlFor="class-name">Class Name</label>

              <input
                id="class-name"
                type="text"
                value={name}
                onChange={(event) => setName(event.target.value)}
                placeholder="e.g. Class 6"
                maxLength={200}
                required
                disabled={createClassMutation.isPending}
              />
            </div>

            <div className="admin-class-form-group">
              <label htmlFor="class-total-seats">Total Seats</label>

              <input
                id="class-total-seats"
                type="number"
                min={1}
                max={2147483647}
                step={1}
                value={totalSeats}
                onChange={(event) =>
                  setTotalSeats(event.target.value)
                }
                required
                disabled={createClassMutation.isPending}
              />
            </div>
          </div>

          <button
            type="submit"
            className="admin-class-submit-button"
            disabled={
              createClassMutation.isPending ||
              !name.trim() ||
              !totalSeats
            }
          >
            {createClassMutation.isPending ? (
              <>
                <RefreshCw size={17} className="spin" />
                Creating Class...
              </>
            ) : (
              <>
                <Plus size={17} />
                Create Class
              </>
            )}
          </button>
        </form>

        {successMessage && (
          <div className="admin-class-feedback success" role="status">
            <CheckCircle2 size={18} />
            <span>{successMessage}</span>
          </div>
        )}

        {createClassMutation.isError && (
          <div className="admin-class-feedback error" role="alert">
            <AlertCircle size={18} />
            <span>
              {getApiErrorMessage(createClassMutation.error)}
            </span>
          </div>
        )}
      </section>

      <section className="admin-class-list-section">
        <div className="admin-class-list-heading">
          <div>
            <span className="section-eyebrow">CLASS OVERVIEW</span>
            <h3>Admission Classes</h3>
            <p>
              View all classes and monitor their current seat availability.
            </p>
          </div>

          <button
            type="button"
            className="admin-refresh-button"
            onClick={() => refetch()}
            disabled={isFetching}
          >
            <RefreshCw
              size={15}
              className={isFetching ? "spin" : ""}
            />
            {isFetching ? "Refreshing..." : "Refresh"}
          </button>
        </div>

        {isLoading ? (
          <div className="state-message">
            <RefreshCw size={24} className="spin" />
            <p>Loading admission classes...</p>
          </div>
        ) : isError ? (
          <div className="state-message error-state">
            <AlertCircle size={24} />
            <p>Unable to load admission classes.</p>

            <button type="button" onClick={() => refetch()}>
              Try Again
            </button>
          </div>
        ) : classes.length === 0 ? (
          <div className="state-message">
            <p>No admission classes have been created yet.</p>
          </div>
        ) : (
          <div className="admin-class-grid">
            {classes.map((admissionClass) => (
              <article
                className="admin-class-item"
                key={admissionClass.id}
              >
                <div className="admin-class-item-header">
                  <div className="admin-class-icon">
                    <Users size={20} />
                  </div>

                  <span className="admin-class-capacity">
                    {admissionClass.availableSeats > 0
                      ? "Available"
                      : "Full"}
                  </span>
                </div>

                <h4>{admissionClass.name}</h4>

                <p>
                  {admissionClass.totalSeats} total seats
                </p>

                <div className="admin-class-seat-details">
                  <span>
                    Held: <strong>{admissionClass.heldSeats}</strong>
                  </span>

                  <span>
                    Confirmed:{" "}
                    <strong>{admissionClass.confirmedSeats}</strong>
                  </span>

                  <span>
                    Available:{" "}
                    <strong>{admissionClass.availableSeats}</strong>
                  </span>
                </div>
              </article>
            ))}
          </div>
        )}

        <div className="admin-class-list-footer">
          Total classes: <strong>{classes.length}</strong>
        </div>
      </section>
    </div>
  );
};

export default AdminClasses;