# Contributing to Getir Mobile

First off, thank you for considering contributing to Getir Mobile! It's people like you that make Getir Mobile such a great project.

## Code of Conduct

By participating in this project, you are expected to uphold our [Code of Conduct](CODE_OF_CONDUCT.md).

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues to avoid duplicates. When creating a bug report, include:

- **Environment details**: Flutter/Dart version, OS, device
- **Expected behavior**: What should happen?
- **Actual behavior**: What actually happens?
- **Steps to reproduce**: Detailed steps to reproduce the issue
- **Screenshots/videos**: If applicable
- **Logs**: Relevant error logs

**Template:**

```markdown
**Environment:**
- Flutter: 3.19.0
- Dart: 3.3.0
- OS: Windows 11
- Device: Android Emulator (API 33)

**Expected Behavior:**
[Description]

**Actual Behavior:**
[Description]

**Steps to Reproduce:**
1. ...
2. ...
3. ...

**Screenshots:**
[Attach screenshots]

**Logs:**
```
[Paste logs]
```
```

### Suggesting Features

Feature suggestions are welcome! Please:

- Check if the feature already exists or is planned
- Provide a clear, detailed description
- Explain the use case and benefits
- Consider implementation complexity

### Pull Requests

#### Before Submitting

1. **Create an issue** first for significant changes
2. **Fork the repository**
3. **Create a feature branch**: `git checkout -b feature/your-feature-name`
4. **Follow code style**: Run `flutter analyze` and `dart format`
5. **Write tests**: Add tests for new features
6. **Update documentation**: Update README/docs as needed

#### PR Checklist

- [ ] Code follows project style guidelines
- [ ] All tests pass (`flutter test`)
- [ ] No linter warnings (`flutter analyze`)
- [ ] Code is formatted (`dart format`)
- [ ] Tests added for new functionality
- [ ] Documentation updated
- [ ] PR title follows conventional commits format

#### Commit Message Format

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding/updating tests
- `chore`: Maintenance tasks

**Examples:**

```
feat(auth): add biometric authentication

Add fingerprint and face recognition support for login

Closes #123
```

```
fix(cart): resolve cart item duplication bug

Cart items were being duplicated when adding same product multiple times

Fixes #456
```

#### PR Workflow

1. **Update your branch** with latest `main`:
   ```bash
   git checkout main
   git pull origin main
   git checkout feature/your-feature-name
   git merge main
   ```

2. **Push your changes**:
   ```bash
   git push origin feature/your-feature-name
   ```

3. **Create Pull Request** on GitHub
   - Title: Use conventional commit format
   - Description: Explain the changes and reference issues
   - Assign reviewers

4. **Respond to feedback** and update PR as needed

5. **Once approved**, maintainers will merge

## Development Setup

See [README.md](README.md#getting-started) for detailed setup instructions.

### Quick Start

```bash
# Clone repository
git clone https://github.com/your-org/getir-mobile.git
cd getir-mobile/getir_mobile

# Install dependencies
flutter pub get

# Run code generation
dart run build_runner build --delete-conflicting-outputs

# Run tests
flutter test

# Run app
flutter run
```

### Code Style

- Follow [Dart Style Guide](https://dart.dev/guides/language/effective-dart/style)
- Use `flutter analyze` to check code quality
- Use `dart format` to format code
- Maximum line length: 80 characters

### Testing Guidelines

- Write unit tests for business logic
- Write widget tests for UI components
- Write integration tests for critical flows
- Aim for minimum 60% code coverage
- Use AAA pattern (Arrange, Act, Assert)

### Architecture

Follow **Clean Architecture** principles:

1. **Presentation Layer**: UI, BLoCs, Widgets
2. **Domain Layer**: Entities, Use Cases, Repository Interfaces
3. **Data Layer**: Repository Implementations, Data Sources, DTOs

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for details.

## Questions?

Feel free to:
- Open an issue for questions
- Contact maintainers via email
- Join our Slack channel

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

