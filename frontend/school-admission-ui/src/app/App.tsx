
import { Navigate, Route, Routes, useLocation } from "react-router-dom";

import ClassesPage from "../features/classes/ClassesPage";
import MyApplicationsPage from "../features/my-applications/MyApplicationsPage";

const App = () => {
  const location = useLocation();

  return (
    <div key={location.pathname} className="page-transition">
      <Routes>
        <Route path="/" element={<Navigate to="/classes" replace />} />

        <Route path="/classes" element={<ClassesPage />} />

        <Route
          path="/my-applications"
          element={<MyApplicationsPage />}
        />

        <Route path="*" element={<Navigate to="/classes" replace />} />
      </Routes>
    </div>
  );
};

export default App;
