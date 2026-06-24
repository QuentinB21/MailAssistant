import Keycloak from "keycloak-js";

export const keycloak = new Keycloak({
  url:
    import.meta.env.VITE_KEYCLOAK_URL ??
    `${window.location.protocol}//${window.location.hostname}:8080`,
  realm: import.meta.env.VITE_KEYCLOAK_REALM ?? "mailassistant",
  clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID ?? "mailassistant-web",
});

let initialization: Promise<boolean> | null = null;

export function initializeKeycloak() {
  initialization ??= keycloak.init({
    onLoad: "check-sso",
    pkceMethod: "S256",
    checkLoginIframe: false,
  });

  return initialization;
}
