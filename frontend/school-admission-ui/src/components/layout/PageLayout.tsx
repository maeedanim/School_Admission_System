import type { ReactNode } from "react";
import Navbar from "./Navbar";

interface PageLayoutProps {
  children: ReactNode;
}

const PageLayout = ({ children }: PageLayoutProps) => {
  return (
    <>
      <Navbar />

      <main className="page-container">
        {children}
      </main>

      <footer className="footer">
        <p>© {new Date().getFullYear()} EduApply. All rights reserved.</p>
        <p>School Admission Management System</p>
      </footer>
    </>
  );
};

export default PageLayout;