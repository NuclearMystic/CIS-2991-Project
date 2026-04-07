# Contributing to DefaultGameTemplate

Thank you for contributing to a Liminal Arcane Studio project!

## Getting Started

1. Clone the repository
2. Configure git hooks:
   ```bash
   git config core.hooksPath hooks
   ```
3. Open in Unity Hub using the correct LTS version

## Workflow

- **Never commit directly to `main`** — it is branch-protected
- Create a feature branch: `git checkout -b feature/my-feature`
- Keep commits small and focused
- Write clear commit messages (see below)
- Open a Pull Request when ready for review

## Commit Message Format

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
type: short description

Optional longer body explaining the why.
```

Common types: `feat`, `fix`, `chore`, `refactor`, `docs`, `test`, `style`

Examples:
```
feat: add player dash ability
fix: resolve camera clipping through terrain
chore: update URP settings for mobile
```

## Unity Best Practices

- Use Prefabs for all reusable objects
- Store configuration in ScriptableObjects under `Assets/Data/`
- Keep scenes clean — use prefabs, not raw GameObjects
- Do not commit `.meta` files for assets in `.gitignore`
- Test in both editor and build before opening a PR

## Pull Requests

- Fill in the PR template completely
- Link any related issues
- Assign at least one reviewer
- Resolve all review comments before merging

## Questions?

Open an issue or reach out to the team.
