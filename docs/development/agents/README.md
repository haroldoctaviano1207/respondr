# Respondr Project Agents

Respondr uses two project-level review agents before pull requests:

- `review`: Lead Developer review agent.
- `quality-check`: Staff QA / Principal-level quality agent.

The reusable agent prompts are stored in `.codex/agents/`.

The human-readable descriptions are stored in this folder.

## Suggested Pre-PR Flow

1. Ask `review` to inspect the branch for architecture, maintainability, project boundaries, API shape, and build/test risks.
2. Ask `quality-check` to inspect acceptance criteria, test coverage, regression risk, edge cases, and release readiness.
3. Address blocking findings before opening the pull request.
4. Include verification commands in the PR description.
