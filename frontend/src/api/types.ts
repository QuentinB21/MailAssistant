export type OrganizationRole = "Owner" | "Admin" | "Member";
export type MatchOutcome = "NoMatch" | "Matched" | "Conflict";
export type MatchSource = "ProjectName" | "Alias";

export interface Organization {
  id: string;
  name: string;
  role: OrganizationRole;
  createdAt: string;
  updatedAt: string;
}

export interface OrganizationSettings {
  organizationId: string;
  multipleMatchBehavior: "MarkAsConflict";
  noMatchBehavior: "Ignore";
  archiveGmailAfterClassification: boolean;
}

export interface ProjectAlias {
  id: string;
  value: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Project {
  id: string;
  organizationId: string;
  name: string;
  description: string | null;
  isActive: boolean;
  classificationTargetName: string;
  createdAt: string;
  updatedAt: string;
  aliases: ProjectAlias[];
}

export interface ProjectInput {
  name: string;
  classificationTargetName: string;
  description: string | null;
}

export interface ProjectUpdate extends ProjectInput {
  isActive: boolean;
}

export interface ProjectMatch {
  projectId: string;
  projectName: string;
  matchedValue: string;
  source: MatchSource;
}

export interface MatchingResult {
  outcome: MatchOutcome;
  normalizedSubject: string;
  selectedProjectId: string | null;
  matches: ProjectMatch[];
}

export interface GmailAccount {
  id: string;
  organizationId: string;
  emailAddress: string;
  isAutomaticClassificationEnabled: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface GmailAuthorization {
  authorizationUrl: string;
}

export interface GmailManualClassification {
  outcome: MatchOutcome;
  normalizedSubject: string;
  projectId: string | null;
  projectName: string | null;
  labelName: string | null;
  labelApplied: boolean;
  archived: boolean;
}
