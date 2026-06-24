import {
  type PropsWithChildren,
  useEffect,
  useMemo,
  useState,
} from "react";
import { AuthContext, type AuthSession } from "./auth";
import { initializeKeycloak, keycloak } from "./keycloak";

export function AuthProvider({ children }: PropsWithChildren) {
  const [initialized, setInitialized] = useState(false);
  const [authenticated, setAuthenticated] = useState(false);

  useEffect(() => {
    let active = true;

    initializeKeycloak()
      .then((isAuthenticated) => {
        if (active) {
          setAuthenticated(isAuthenticated);
          setInitialized(true);
        }
      })
      .catch(() => {
        if (active) {
          setAuthenticated(false);
          setInitialized(true);
        }
      });

    return () => {
      active = false;
    };
  }, []);

  const session = useMemo<AuthSession>(
    () => ({
      initialized,
      authenticated,
      displayName:
        keycloak.tokenParsed?.name ??
        keycloak.tokenParsed?.preferred_username ??
        null,
      login: async () => {
        await keycloak.login();
      },
      logout: async () => {
        await keycloak.logout({
          redirectUri: window.location.origin,
        });
      },
      getAccessToken: async () => {
        if (!keycloak.authenticated) {
          throw new Error("Authentication is required.");
        }

        await keycloak.updateToken(30);
        if (!keycloak.token) {
          throw new Error("Access token is unavailable.");
        }

        return keycloak.token;
      },
    }),
    [authenticated, initialized],
  );

  return <AuthContext.Provider value={session}>{children}</AuthContext.Provider>;
}
