# Manual Security & Branch Protection Setup

After merging this PR, repository owners must manually apply the following security and branch protection settings in the GitHub UI. This repository intentionally has no CI/CD or automated builds—users are responsible for all builds and deployments locally via Docker or .NET.

## 1. Enable Secret Scanning & Push Protection
- Go to **Settings → Security and analysis**
- Enable **Secret scanning**
- Enable **Push protection for secrets**

## 2. Enable Dependabot Alerts & Security Updates
- Go to **Settings → Security and analysis**
- Enable **Dependabot alerts**
- Enable **Dependabot security updates**

## 3. Configure Branch Protection for `main`
- Go to **Settings → Branches → Branch protection rules**
- Add or edit a rule for `main` with these settings:
  - Require a pull request before merging
  - Require at least 1 approval
  - Dismiss stale pull request approvals when new commits are pushed
  - Require branches to be up to date before merging
  - Restrict who can push to matching branches (no direct pushes)
  - Include administrators
  - **Do NOT require status checks** (since there are no workflows)
- (Optional) Require signed commits if you use GPG consistently

## 4. No CI/CD or Automation
- This repository intentionally does **not** use GitHub Actions, CI/CD, or any automated build/deployment pipelines.
- All builds and deployments are performed manually by users in their own environment (e.g., using Docker or `dotnet` CLI).

## 5. Additional Recommendations
- Review and update the **SECURITY.md** policy as needed
- Regularly review repository access and permissions
- Monitor Dependabot and secret scanning alerts

---

**For more details, see [SECURITY.md](../SECURITY.md).**
