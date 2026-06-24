import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { AuthenticatedApp } from "./auth/AuthenticatedApp";
import { AuthProvider } from "./auth/AuthContext";
import "./styles.css";

const root = document.getElementById("root");

if (!root) {
  throw new Error("Élément racine introuvable.");
}

createRoot(root).render(
  <StrictMode>
    <AuthProvider>
      <AuthenticatedApp />
    </AuthProvider>
  </StrictMode>,
);
