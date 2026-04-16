# tsa-submissions-coding-fallback

A fallback ASP.NET Core MVC app for viewing TSA coding problems and collecting submissions.

## Run locally

```bash
cd /home/runner/work/tsa-submissions-coding-fallback/tsa-submissions-coding-fallback/TsaSubmissions.Web
dotnet run
```

Default seeded accounts:

- Participant: `participant` / `participant123!`
- Judge: `judge` / `judge123!`

## Kubernetes (K3s)

Deploy PostgreSQL and app manifests from `/home/runner/work/tsa-submissions-coding-fallback/tsa-submissions-coding-fallback/k8s`.
