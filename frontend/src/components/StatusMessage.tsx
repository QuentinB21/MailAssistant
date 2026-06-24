import type { ReactNode } from "react";

interface StatusMessageProps {
  kind: "error" | "success" | "info";
  children: ReactNode;
}

export function StatusMessage({ kind, children }: StatusMessageProps) {
  return (
    <div className={`status-message status-${kind}`} role="status">
      {children}
    </div>
  );
}
