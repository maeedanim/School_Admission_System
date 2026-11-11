import PageLayout from "../../components/layout/PageLayout";
import AdminClasses from "./AdminClasses";

const AdminClassesPage = () => {
  return (
    <PageLayout>
      <section className="applications-hero">
        <span className="section-eyebrow">ADMIN DASHBOARD</span>

        <h1>Class Management</h1>

        <p>
          Create admission classes, define seat capacity, and
          monitor class availability.
        </p>
      </section>

      <section className="dashboard-section" id="admin-classes">
        <div className="section-heading">
          <div>
            <span className="section-eyebrow">CLASS MANAGEMENT</span>
            <h2>Manage Admission Classes</h2>
            <p>
              Create new classes and monitor their seat allocation.
            </p>
          </div>
        </div>

        <AdminClasses />
      </section>
    </PageLayout>
  );
};

export default AdminClassesPage;