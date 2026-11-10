
import PageLayout from "../../components/layout/PageLayout";
import StudentIdentity from "../../components/layout/StudentIdentity";
import MyApplications from "./MyApplications";

import { useStudentId } from "../../hooks/useStudentId";

const MyApplicationsPage = () => {
  const { studentId, setStudentId, isValid } = useStudentId();

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

      <section className="dashboard-section" id="student">
        <div className="section-heading">
          <div>
            <span className="section-eyebrow">STEP 01</span>
            <h2>Student Identification</h2>
            <p>
              Your saved student identification is used to retrieve
              your admission applications.
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
        id="my-applications"
      >
        <div className="section-heading">
          <div>
            <span className="section-eyebrow">STEP 02</span>
            <h2>Application History</h2>
            <p>
              Review your applications and manage your active
              admission seat holds.
            </p>
          </div>
        </div>

        <MyApplications studentId={studentId} />
      </section>
    </PageLayout>
  );
};

export default MyApplicationsPage;
