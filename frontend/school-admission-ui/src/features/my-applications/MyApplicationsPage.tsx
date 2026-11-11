
import PageLayout from "../../components/layout/PageLayout";
import MyApplications from "./MyApplications";

const MyApplicationsPage = () => {
  return (
    <PageLayout>
      <section className="applications-hero">
        <span className="section-eyebrow">STUDENT DASHBOARD</span>

        <h1>My Applications</h1>

        <p>
          Track your admission applications, monitor seat holds,
          and complete your admission confirmation.
        </p>
      </section>

      <section
        className="dashboard-section"
        id="my-applications"
      >
        <div className="section-heading">
          <div>
            <span className="section-eyebrow">APPLICATION HISTORY</span>
            <h2>Your Applications</h2>
            <p>
              Review your applications and manage your active
              admission seat holds.
            </p>
          </div>
        </div>

        <MyApplications />
      </section>
    </PageLayout>
  );
};

export default MyApplicationsPage;
