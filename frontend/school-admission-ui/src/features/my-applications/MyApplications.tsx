
import { useEffect, useState } from "react";
import { createPortal } from "react-dom";
import {
  Clock,
  CheckCircle2,
  XCircle,
  CreditCard,
  Ban,
  LoaderCircle,
  AlertCircle,
  X,
  Copy,
  Check,
} from "lucide-react";

import { useMyHolds } from "./useMyHolds";
import { useAdmissionMutations } from "../admissions/useAdmissionMutations";
import { useAuth } from "../auth/AuthContext";
import { getApiErrorMessage } from "../../utils/getApiErrorMessage";
import type { HoldStatus } from "../../types/hold";

interface ActionToast {
  type: "success" | "error";
  message: string;
}

const formatRemainingTime = (milliseconds: number): string => {
  const totalSeconds = Math.max(0, Math.ceil(milliseconds / 1000));

  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;

  return `${String(minutes).padStart(2, "0")}:${String(seconds).padStart(2, "0")}`;
};

const MyApplications = () => {
  const { user } = useAuth();

  const {
    data: applications,
    isLoading,
    isError,
    error,
    refetch,
  } = useMyHolds(user?.userId ?? null);

  const {
    confirmHoldMutation,
    cancelHoldMutation,
  } = useAdmissionMutations();

  const [currentTime, setCurrentTime] = useState(Date.now());
  const [actionToast, setActionToast] = useState<ActionToast | null>(null);
  const [copiedHoldId, setCopiedHoldId] = useState<string | null>(null);

  // Update the countdown every second.
  useEffect(() => {
    const intervalId = window.setInterval(() => {
      setCurrentTime(Date.now());
    }, 1000);

    return () => window.clearInterval(intervalId);
  }, []);

  // Automatically dismiss notifications.
  useEffect(() => {
    if (!actionToast) return;

    const timeoutId = window.setTimeout(() => {
      setActionToast(null);
    }, 5000);

    return () => window.clearTimeout(timeoutId);
  }, [actionToast]);

  // Automatically reset the copied indicator.
  useEffect(() => {
    if (!copiedHoldId) return;

    const timeoutId = window.setTimeout(() => {
      setCopiedHoldId(null);
    }, 2000);

    return () => window.clearTimeout(timeoutId);
  }, [copiedHoldId]);

  const handleCopyHoldId = async (holdId: string) => {
    try {
      await navigator.clipboard.writeText(holdId);
      setCopiedHoldId(holdId);
    } catch {
      setActionToast({
        type: "error",
        message: "Unable to copy the application ID.",
      });
    }
  };

  const handleConfirm = (holdId: string) => {
    confirmHoldMutation.mutate(
      { holdId },
      {
        onSuccess: () => {
          setActionToast({
            type: "success",
            message: "Your admission has been confirmed successfully.",
          });
        },
        onError: (error) => {
          setActionToast({
            type: "error",
            message: getApiErrorMessage(error),
          });
        },
      }
    );
  };

  const handleCancel = (holdId: string) => {
    cancelHoldMutation.mutate(
      { holdId },
      {
        onSuccess: () => {
          setActionToast({
            type: "success",
            message: "Your application has been cancelled successfully.",
          });
        },
        onError: (error) => {
          setActionToast({
            type: "error",
            message: getApiErrorMessage(error),
          });
        },
      }
    );
  };

  const isActionPending =
    confirmHoldMutation.isPending || cancelHoldMutation.isPending;

  // Render notifications independently of the application-list state.
  const actionToastElement = actionToast
    ? createPortal(
        <div
          className={`toast-notification toast-${actionToast.type}`}
          role={actionToast.type === "error" ? "alert" : "status"}
          aria-live={actionToast.type === "error" ? "assertive" : "polite"}
        >
          <div className="toast-icon">
            {actionToast.type === "success" ? (
              <CheckCircle2 size={22} />
            ) : (
              <AlertCircle size={22} />
            )}
          </div>

          <div className="toast-content">
            <strong>
              {actionToast.type === "success"
                ? "Operation successful"
                : "Operation unsuccessful"}
            </strong>

            <p>{actionToast.message}</p>
          </div>

          <button
            type="button"
            className="toast-close"
            onClick={() => setActionToast(null)}
            aria-label="Dismiss notification"
          >
            <X size={18} />
          </button>
        </div>,
        document.body
      )
    : null;

  if (isLoading) {
    return (
      <>
        <div className="state-message">
          <LoaderCircle className="spin" size={24} />
          <p>Loading your applications...</p>
        </div>

        {actionToastElement}
      </>
    );
  }

  if (isError) {
    return (
      <>
        <div className="state-message error-state">
          <AlertCircle size={22} />
          <p>
            {getApiErrorMessage(error, "Unable to load your applications.")}
          </p>
          <button type="button" onClick={() => refetch()}>
            Try Again
          </button>
        </div>

        {actionToastElement}
      </>
    );
  }

  if (!applications || applications.length === 0) {
    return (
      <>
        <div className="state-message">
          <CreditCard size={24} />
          <p>You haven't submitted any admission applications yet.</p>
        </div>

        {actionToastElement}
      </>
    );
  }

  return (
    <>
      <div className="applications-list">
        {applications.map((application) => {
          const expiresAt = new Date(application.expiresAt).getTime();

          const isLocallyExpired =
            application.status === "HELD" && expiresAt <= currentTime;

          const effectiveStatus: HoldStatus = isLocallyExpired
            ? "EXPIRED"
            : application.status;

          const remainingTime = expiresAt - currentTime;

          const canTakeAction = effectiveStatus === "HELD";

          return (
            <article
              className="application-card"
              key={application.holdId}
            >
              <div className="application-card-header">
                <div>
                  <span className="application-label">
                    ADMISSION APPLICATION
                  </span>

                  <h3>{application.className}</h3>
                </div>

                <span
                  className={`application-status status-${effectiveStatus.toLowerCase()}`}
                >
                  {effectiveStatus}
                </span>
              </div>

              <div className="application-details">
                <div className="application-detail">
                  <span>Expiry time</span>
                  <strong>
                    {new Date(application.expiresAt).toLocaleString()}
                  </strong>
                </div>
              </div>

              <details className="application-extra-details">
                <summary>Application Details</summary>

                <div className="application-id-row">
                  <div className="application-detail">
                    <span>Hold ID</span>
                    <strong>{application.holdId}</strong>
                  </div>

                  <button
                    type="button"
                    className="copy-id-button"
                    onClick={() => handleCopyHoldId(application.holdId)}
                  >
                    {copiedHoldId === application.holdId ? (
                      <>
                        <Check size={15} />
                        Copied
                      </>
                    ) : (
                      <>
                        <Copy size={15} />
                        Copy ID
                      </>
                    )}
                  </button>
                </div>
              </details>

              {canTakeAction && (
                <div className="hold-countdown">
                  <Clock size={18} />

                  <span>Time remaining</span>

                  <strong>
                    {formatRemainingTime(remainingTime)}
                  </strong>
                </div>
              )}

              {effectiveStatus === "EXPIRED" && (
                <div className="application-notice expired-notice">
                  <XCircle size={18} />
                  <span>This seat hold has expired.</span>
                </div>
              )}

              {effectiveStatus === "CONFIRMED" && (
                <div className="application-notice confirmed-notice">
                  <CheckCircle2 size={18} />
                  <span>Your admission is confirmed.</span>
                </div>
              )}

              {effectiveStatus === "CANCELLED" && (
                <div className="application-notice cancelled-notice">
                  <Ban size={18} />
                  <span>This application has been cancelled.</span>
                </div>
              )}

              {canTakeAction && (
                <div className="application-actions">
                  <button
                    type="button"
                    className="confirm-button"
                    disabled={isActionPending}
                    onClick={() => handleConfirm(application.holdId)}
                  >
                    <CheckCircle2 size={17} />

                    {confirmHoldMutation.isPending
                      ? "Processing..."
                      : "Confirm Admission"}
                  </button>

                  <button
                    type="button"
                    className="cancel-button"
                    disabled={isActionPending}
                    onClick={() => handleCancel(application.holdId)}
                  >
                    <Ban size={17} />
                    Cancel Application
                  </button>
                </div>
              )}
            </article>
          );
        })}
      </div>

      {actionToastElement}
    </>
  );
};

export default MyApplications;
