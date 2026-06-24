import { type FormEvent, useEffect, useMemo, useState } from "react";
import { api } from "../api/apiClient";
import type { Project, ProjectAlias, ProjectInput } from "../api/types";
import type { AuthSession } from "../auth/auth";
import { StatusMessage } from "./StatusMessage";

interface ProjectManagerProps {
  session: AuthSession;
  organizationId: string;
  projects: Project[];
  canManage: boolean;
  onProjectsChange: (projects: Project[]) => void;
}

const emptyProject: ProjectInput = {
  name: "",
  classificationTargetName: "",
  description: null,
};

export function ProjectManager({
  session,
  organizationId,
  projects,
  canManage,
  onProjectsChange,
}: ProjectManagerProps) {
  const [selectedProjectId, setSelectedProjectId] = useState<string | null>(
    projects[0]?.id ?? null,
  );
  const [creating, setCreating] = useState(false);
  const [draft, setDraft] = useState<ProjectInput>(emptyProject);
  const [aliasValue, setAliasValue] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const selectedProject = useMemo(
    () => projects.find((project) => project.id === selectedProjectId) ?? null,
    [projects, selectedProjectId],
  );

  useEffect(() => {
    if (selectedProject && !creating) {
      setDraft({
        name: selectedProject.name,
        classificationTargetName: selectedProject.classificationTargetName,
        description: selectedProject.description,
      });
    }
  }, [creating, selectedProject]);

  const selectProject = (project: Project) => {
    setSelectedProjectId(project.id);
    setCreating(false);
    setDraft({
      name: project.name,
      classificationTargetName: project.classificationTargetName,
      description: project.description,
    });
    setError(null);
  };

  const startCreation = () => {
    setCreating(true);
    setSelectedProjectId(null);
    setDraft(emptyProject);
    setError(null);
  };

  const saveProject = async (event: FormEvent) => {
    event.preventDefault();
    if (!draft.name.trim() || !draft.classificationTargetName.trim()) {
      setError("Le nom et la cible de classement sont obligatoires.");
      return;
    }

    setBusy(true);
    setError(null);
    try {
      if (creating) {
        const created = await api.createProject(session, organizationId, {
          ...draft,
          name: draft.name.trim(),
          classificationTargetName: draft.classificationTargetName.trim(),
          description: draft.description?.trim() || null,
        });
        onProjectsChange([...projects, created]);
        selectProject(created);
      } else if (selectedProject) {
        const updated = await api.updateProject(
          session,
          organizationId,
          selectedProject.id,
          {
            ...draft,
            name: draft.name.trim(),
            classificationTargetName: draft.classificationTargetName.trim(),
            description: draft.description?.trim() || null,
            isActive: selectedProject.isActive,
          },
        );
        replaceProject(updated);
        selectProject(updated);
      }
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Impossible d’enregistrer le projet.",
      );
    } finally {
      setBusy(false);
    }
  };

  const replaceProject = (project: Project) => {
    onProjectsChange(
      projects.map((existing) =>
        existing.id === project.id ? project : existing,
      ),
    );
  };

  const toggleProject = async () => {
    if (!selectedProject) {
      return;
    }

    setBusy(true);
    setError(null);
    try {
      const updated = await api.updateProject(
        session,
        organizationId,
        selectedProject.id,
        {
          name: selectedProject.name,
          classificationTargetName: selectedProject.classificationTargetName,
          description: selectedProject.description,
          isActive: !selectedProject.isActive,
        },
      );
      replaceProject(updated);
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Impossible de modifier le statut.",
      );
    } finally {
      setBusy(false);
    }
  };

  const deleteProject = async () => {
    if (
      !selectedProject ||
      !window.confirm(`Supprimer le projet « ${selectedProject.name} » ?`)
    ) {
      return;
    }

    setBusy(true);
    try {
      await api.deleteProject(session, organizationId, selectedProject.id);
      const remaining = projects.filter(
        (project) => project.id !== selectedProject.id,
      );
      onProjectsChange(remaining);
      setSelectedProjectId(remaining[0]?.id ?? null);
      setCreating(false);
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Impossible de supprimer le projet.",
      );
    } finally {
      setBusy(false);
    }
  };

  const addAlias = async (event: FormEvent) => {
    event.preventDefault();
    if (!selectedProject || !aliasValue.trim()) {
      return;
    }

    setBusy(true);
    setError(null);
    try {
      const alias = await api.addAlias(
        session,
        organizationId,
        selectedProject.id,
        aliasValue.trim(),
      );
      replaceProject({
        ...selectedProject,
        aliases: [...selectedProject.aliases, alias],
      });
      setAliasValue("");
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Impossible d’ajouter l’alias.",
      );
    } finally {
      setBusy(false);
    }
  };

  const updateAlias = async (alias: ProjectAlias) => {
    if (!selectedProject) {
      return;
    }

    setBusy(true);
    try {
      const updated = await api.updateAlias(
        session,
        organizationId,
        selectedProject.id,
        alias,
      );
      replaceProject({
        ...selectedProject,
        aliases: selectedProject.aliases.map((existing) =>
          existing.id === updated.id ? updated : existing,
        ),
      });
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Impossible de modifier l’alias.",
      );
    } finally {
      setBusy(false);
    }
  };

  const deleteAlias = async (alias: ProjectAlias) => {
    if (!selectedProject) {
      return;
    }

    setBusy(true);
    try {
      await api.deleteAlias(
        session,
        organizationId,
        selectedProject.id,
        alias.id,
      );
      replaceProject({
        ...selectedProject,
        aliases: selectedProject.aliases.filter(
          (existing) => existing.id !== alias.id,
        ),
      });
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Impossible de supprimer l’alias.",
      );
    } finally {
      setBusy(false);
    }
  };

  return (
    <section aria-labelledby="projects-title">
      <header className="page-header">
        <div>
          <p className="eyebrow">Règles métier</p>
          <h2 id="projects-title">Projets et alias</h2>
          <p>Définissez les termes qui seront recherchés dans les objets.</p>
        </div>
        {canManage && (
          <button className="primary-action" type="button" onClick={startCreation}>
            Nouveau projet
          </button>
        )}
      </header>

      {error && <StatusMessage kind="error">{error}</StatusMessage>}

      <div className="project-layout">
        <aside className="project-list" aria-label="Liste des projets">
          {projects.length === 0 && (
            <div className="empty-state compact">
              <h3>Aucun projet</h3>
              <p>Créez votre première règle de classement.</p>
            </div>
          )}
          {projects.map((project) => (
            <button
              className={`project-list-item ${
                selectedProjectId === project.id ? "selected" : ""
              }`}
              type="button"
              key={project.id}
              onClick={() => selectProject(project)}
            >
              <span>
                <strong>{project.name}</strong>
                <small>{project.aliases.length} alias</small>
              </span>
              <span
                className={`status-dot ${project.isActive ? "active" : "inactive"}`}
                aria-label={project.isActive ? "Actif" : "Inactif"}
              />
            </button>
          ))}
        </aside>

        <div className="project-editor">
          {creating || selectedProject ? (
            <>
              <form className="content-card form-card" onSubmit={saveProject}>
                <div className="card-heading">
                  <div>
                    <h3>{creating ? "Nouveau projet" : selectedProject?.name}</h3>
                    <p>
                      {creating
                        ? "Configurez son nom et sa destination."
                        : "Modifiez les informations et le statut."}
                    </p>
                  </div>
                  {selectedProject && canManage && (
                    <button
                      className={`toggle-button ${
                        selectedProject.isActive ? "enabled" : ""
                      }`}
                      type="button"
                      onClick={toggleProject}
                      disabled={busy}
                    >
                      {selectedProject.isActive ? "Actif" : "Inactif"}
                    </button>
                  )}
                </div>

                <div className="form-grid">
                  <label>
                    Nom du projet
                    <input
                      value={draft.name}
                      onChange={(event) =>
                        setDraft({ ...draft, name: event.target.value })
                      }
                      disabled={!canManage}
                      maxLength={200}
                    />
                  </label>
                  <label>
                    Label ou dossier cible
                    <input
                      value={draft.classificationTargetName}
                      onChange={(event) =>
                        setDraft({
                          ...draft,
                          classificationTargetName: event.target.value,
                        })
                      }
                      disabled={!canManage}
                      maxLength={200}
                    />
                  </label>
                  <label className="full-width">
                    Description
                    <textarea
                      rows={3}
                      value={draft.description ?? ""}
                      onChange={(event) =>
                        setDraft({
                          ...draft,
                          description: event.target.value,
                        })
                      }
                      disabled={!canManage}
                      maxLength={2000}
                    />
                  </label>
                </div>

                {canManage && (
                  <div className="form-actions">
                    {!creating && (
                      <button
                        className="danger-action"
                        type="button"
                        onClick={deleteProject}
                        disabled={busy}
                      >
                        Supprimer
                      </button>
                    )}
                    <button
                      className="primary-action"
                      type="submit"
                      disabled={busy}
                    >
                      {busy ? "Enregistrement…" : "Enregistrer"}
                    </button>
                  </div>
                )}
              </form>

              {selectedProject && (
                <article className="content-card alias-card">
                  <div className="card-heading">
                    <div>
                      <h3>Alias de détection</h3>
                      <p>
                        Ajoutez les noms courts, codes projet ou variantes
                        usuelles.
                      </p>
                    </div>
                  </div>

                  <div className="alias-list">
                    {selectedProject.aliases.length === 0 && (
                      <p className="muted">Aucun alias configuré.</p>
                    )}
                    {selectedProject.aliases.map((alias) => (
                      <div className="alias-row" key={alias.id}>
                        <span>{alias.value}</span>
                        <div className="alias-actions">
                          <button
                            className={`mini-toggle ${alias.isActive ? "enabled" : ""}`}
                            type="button"
                            onClick={() =>
                              updateAlias({
                                ...alias,
                                isActive: !alias.isActive,
                              })
                            }
                            disabled={!canManage || busy}
                          >
                            {alias.isActive ? "Actif" : "Inactif"}
                          </button>
                          {canManage && (
                            <button
                              className="icon-button"
                              type="button"
                              onClick={() => deleteAlias(alias)}
                              disabled={busy}
                              aria-label={`Supprimer l’alias ${alias.value}`}
                            >
                              ×
                            </button>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>

                  {canManage && (
                    <form className="inline-form" onSubmit={addAlias}>
                      <input
                        value={aliasValue}
                        onChange={(event) => setAliasValue(event.target.value)}
                        placeholder="Ex. APOLLO-24"
                        aria-label="Nouvel alias"
                        maxLength={200}
                      />
                      <button
                        className="secondary-action"
                        type="submit"
                        disabled={busy || !aliasValue.trim()}
                      >
                        Ajouter
                      </button>
                    </form>
                  )}
                </article>
              )}
            </>
          ) : (
            <div className="content-card empty-state">
              <span className="empty-icon">+</span>
              <h3>Sélectionnez un projet</h3>
              <p>Consultez ses règles ou créez un nouveau projet.</p>
            </div>
          )}
        </div>
      </div>
    </section>
  );
}
