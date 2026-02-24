# Copilot Instructions

## Commit Message Format

Always use conventional commits format when creating commits. Follow this structure:

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

### Types

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation only changes
- **style**: Changes that do not affect the meaning of the code (white-space, formatting, etc)
- **refactor**: A code change that neither fixes a bug nor adds a feature
- **perf**: A code change that improves performance
- **test**: Adding missing tests or correcting existing tests
- **build**: Changes that affect the build system or external dependencies
- **ci**: Changes to CI configuration files and scripts
- **chore**: Other changes that don't modify src or test files

### Scope

The scope should indicate the package or area affected (e.g., `abstractions`, `http`, `serialization-json`).

### Examples

```
feat(abstractions): add support for pattern properties
fix(serialization-json): updates boolean serialization
docs(README): update installation instructions
ci(release): configure automated release workflow
```

### Breaking Changes

If a commit introduces a breaking change, add `BREAKING CHANGE:` in the footer or append `!` after the type/scope:

```
feat(abstractions)!: change output format for models

BREAKING CHANGE: The emitter now generates TypeScript interfaces instead of types
```
