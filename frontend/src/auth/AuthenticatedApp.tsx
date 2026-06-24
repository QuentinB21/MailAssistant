import { App } from "../App";
import { useAuth } from "./auth";

export function AuthenticatedApp() {
  return <App session={useAuth()} />;
}
