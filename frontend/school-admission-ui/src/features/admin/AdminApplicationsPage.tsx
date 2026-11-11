
import PageLayout from "../../components/layout/PageLayout";
import AdminApplications from "./AdminApplications";

const AdminApplicationsPage = () => {
  return (
    <PageLayout>
      <section className="applications-hero">
        <span className="section-eyebrow">ADMIN DASHBOARD</span>

        <h1>Admission Applications</h1>

        <p>
          Monitor student admission applications, review their
          current statuses, and track admission activity.
        </p>
      </section>

      <section className="dashboard-section" id="admin-applications">
        <div className="section-heading">
          <div>
            <span className="section-eyebrow">APPLICATION MANAGEMENT</span>
            <h2>All Applications</h2>
            <p>
              Review admission records across all students and classes.
            </p>
          </div>
        </div>

        <AdminApplications />
      </section>
    </PageLayout>
  );
};

export default AdminApplicationsPage;
