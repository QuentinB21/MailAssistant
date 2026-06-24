import { createContext, useContext } from "react";

export interface AuthSession {
  initialized: boolean;
  authenticated: boolean;
  displayName: string | null;
  login: () => Promise<void>;
  logout: () => Promise<void>;
  getAccessToken: () => Promise<string>;
}

export const AuthContext = createContext<AuthSession | null>(null);

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return context;
}
