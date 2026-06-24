import { run } from "./process.mjs";

for (const file of [
  "tools/process.mjs",
  "tools/verify.mjs",
  "tools/dev.mjs",
  "tools/test-auth.mjs",
]) {
  run(process.execPath, ["--check", file]);
}

run("dotnet", ["restore", "MailAssistant.sln"]);
run("dotnet", [
  "build",
  "MailAssistant.sln",
  "--no-restore",
  "--configuration",
  "Release",
]);
run("dotnet", [
  "test",
  "MailAssistant.sln",
  "--no-build",
  "--configuration",
  "Release",
]);
run("dotnet", ["tool", "restore"]);
run("dotnet", [
  "format",
  "MailAssistant.sln",
  "--verify-no-changes",
  "--no-restore",
]);
run("dotnet", [
  "ef",
  "migrations",
  "has-pending-model-changes",
  "--no-build",
  "--configuration",
  "Release",
  "--project",
  "src/MailAssistant.Infrastructure",
  "--startup-project",
  "src/MailAssistant.Api",
]);

run("npm", ["ci", "--prefix", "frontend"]);
run("npm", ["run", "lint", "--prefix", "frontend"]);
run("npm", ["run", "test", "--prefix", "frontend"]);
run("npm", ["run", "build", "--prefix", "frontend"]);
run("npm", ["audit", "--audit-level=moderate", "--prefix", "frontend"]);

run("docker", ["compose", "config", "--quiet"]);

console.log("All verification checks passed.");
