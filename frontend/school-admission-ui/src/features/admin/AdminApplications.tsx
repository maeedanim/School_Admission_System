
import { useEffect, useMemo, useRef, useState } from "react";
import { RefreshCw } from "lucide-react";

import { useAdminApplications } from "./useAdminApplications";
import type { ApplicationStatus } from "./admin.types";

type StatusFilter = "ALL" | ApplicationStatus;

const statusOptions: { label: string; value: StatusFilter }[] = [
  { label: "All Applications", value: "ALL" },
  { label: "Held", value: "HELD" },
  { label: "Confirmed", value: "CONFIRMED" },
  { label: "Expired", value: "EXPIRED" },
  { label: "Cancelled", value: "CANCELLED" },
];

const formatDate = (date: string | null): string => {
  if (!date) {
    return "—";
  }

  const parsedDate = new Date(date);

  if (Number.isNaN(parsedDate.getTime())) {
    return "—";
  }

  return new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(parsedDate);
};

const AdminApplications = () => {
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("ALL");
  const [searchTerm, setSearchTerm] = useState("");

  const topScrollRef = useRef<HTMLDivElement>(null);
  const topScrollContentRef = useRef<HTMLDivElement>(null);
  const tableContainerRef = useRef<HTMLDivElement>(null);
  const tableRef = useRef<HTMLTableElement>(null);

  const {
    data: applications = [],
    isLoading,
    isError,
    isFetching,
    refetch,
  } = useAdminApplications();

  const statistics = useMemo(() => {
    return {
      total: applications.length,
      held: applications.filter((item) => item.status === "HELD").length,
      confirmed: applications.filter(
        (item) => item.status === "CONFIRMED"
      ).length,
      expired: applications.filter(
        (item) => item.status === "EXPIRED"
      ).length,
      cancelled: applications.filter(
        (item) => item.status === "CANCELLED"
      ).length,
    };
  }, [applications]);

  const filteredApplications = useMemo(() => {
    const normalizedSearch = searchTerm.trim().toLowerCase();

    return applications.filter((application) => {
      const matchesStatus =
        statusFilter === "ALL" ||
        application.status === statusFilter;

      const searchableValues = [
        application.studentName,
        application.studentEmail,
        application.studentId,
        application.applicationId,
        application.className,
        application.classId,
      ];

      const matchesSearch =
        normalizedSearch.length === 0 ||
        searchableValues.some((value) =>
          value?.toLowerCase().includes(normalizedSearch)
        );

      return matchesStatus && matchesSearch;
    });
  }, [applications, statusFilter, searchTerm]);

  // Keep the top scrollbar synchronized with the applications table.
  useEffect(() => {
    const topScroll = topScrollRef.current;
    const topContent = topScrollContentRef.current;
    const tableContainer = tableContainerRef.current;
    const table = tableRef.current;

    if (!topScroll || !topContent || !tableContainer || !table) {
      return;
    }

    const updateScrollWidth = () => {
      topContent.style.width = `${table.scrollWidth}px`;
    };

    const handleTopScroll = () => {
      if (tableContainer.scrollLeft !== topScroll.scrollLeft) {
        tableContainer.scrollLeft = topScroll.scrollLeft;
      }
    };

    const handleTableScroll = () => {
      if (topScroll.scrollLeft !== tableContainer.scrollLeft) {
        topScroll.scrollLeft = tableContainer.scrollLeft;
      }
    };

    updateScrollWidth();

    topScroll.addEventListener("scroll", handleTopScroll);
    tableContainer.addEventListener("scroll", handleTableScroll);

    const resizeObserver = new ResizeObserver(updateScrollWidth);
    resizeObserver.observe(table);
    resizeObserver.observe(tableContainer);

    window.addEventListener("resize", updateScrollWidth);

    return () => {
      topScroll.removeEventListener("scroll", handleTopScroll);
      tableContainer.removeEventListener("scroll", handleTableScroll);
      resizeObserver.disconnect();
      window.removeEventListener("resize", updateScrollWidth);
    };
  }, [filteredApplications.length]);

  if (isLoading) {
    return (
      <div className="state-message">
        <RefreshCw size={25} className="spin" />
        <p>Loading admission applications...</p>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="state-message error-state">
        <p>Unable to load admission applications.</p>

        <button type="button" onClick={() => refetch()}>
          Try Again
        </button>
      </div>
    );
  }

  return (
    <div className="admin-applications">
      {/* Summary statistics */}
      <div className="admin-statistics">
        <div className="admin-stat-card">
          <span>Total Applications</span>
          <strong>{statistics.total}</strong>
        </div>

        <div className="admin-stat-card">
          <span>Held</span>
          <strong>{statistics.held}</strong>
        </div>

        <div className="admin-stat-card">
          <span>Confirmed</span>
          <strong>{statistics.confirmed}</strong>
        </div>

        <div className="admin-stat-card">
          <span>Expired</span>
          <strong>{statistics.expired}</strong>
        </div>

        <div className="admin-stat-card">
          <span>Cancelled</span>
          <strong>{statistics.cancelled}</strong>
        </div>
      </div>

      {/* Search, filter and refresh */}
      <div className="admin-toolbar">
        <div className="admin-toolbar-controls">
          <div className="admin-filter">
            <label htmlFor="application-status-filter">
              Filter by status
            </label>

            <select
              id="application-status-filter"
              value={statusFilter}
              onChange={(event) =>
                setStatusFilter(event.target.value as StatusFilter)
              }
            >
              {statusOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          <div className="admin-search">
            <label htmlFor="application-search">
              Search applications
            </label>

            <input
              id="application-search"
              type="search"
              value={searchTerm}
              onChange={(event) => setSearchTerm(event.target.value)}
              placeholder="Name, ID, email or class..."
            />
          </div>
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

      {/* Applications table */}
      {filteredApplications.length === 0 ? (
        <div className="state-message">
          <p>
            {applications.length === 0
              ? "No admission applications have been submitted yet."
              : "No applications match your search or selected status."}
          </p>
        </div>
      ) : (
        <>
          {/* Horizontal scrollbar positioned above the table */}
          <div
            className="admin-table-scroll-top"
            ref={topScrollRef}
            aria-label="Scroll applications table horizontally"
          >
            <div
              className="admin-table-scroll-top-inner"
              ref={topScrollContentRef}
            />
          </div>

          <div
            className="admin-table-container"
            ref={tableContainerRef}
          >
            <table
              className="admin-applications-table"
              ref={tableRef}
            >
              <thead>
                <tr>
                  <th>Student</th>
                  <th>Student ID</th>
                  <th>Class</th>
                  <th>Status</th>
                  <th>Submitted</th>
                  <th>Expires</th>
                  <th>Confirmed</th>
                  <th>Cancelled</th>
                </tr>
              </thead>

              <tbody>
                {filteredApplications.map((application) => (
                  <tr key={application.applicationId}>
                    <td>
                      <div className="admin-student-cell">
                        <strong>
                          {application.studentName || "Unknown Student"}
                        </strong>

                        <span>
                          {application.studentEmail || "No email available"}
                        </span>
                      </div>
                    </td>

                    <td className="admin-student-id">
                      {application.studentId}
                    </td>

                    <td>{application.className}</td>

                    <td>
                      <span
                        className={`application-status status-${application.status.toLowerCase()}`}
                      >
                        {application.status}
                      </span>
                    </td>

                    <td>{formatDate(application.createdAt)}</td>

                    <td>{formatDate(application.expiresAt)}</td>

                    <td>{formatDate(application.confirmedAt)}</td>

                    <td>{formatDate(application.cancelledAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      <div className="admin-table-footer">
        <span>
          Showing {filteredApplications.length} of {applications.length}{" "}
          applications
        </span>
      </div>
    </div>
  );
};

export default AdminApplications;
