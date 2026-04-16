# tsa-submissions-coding-fallback

A fallback ASP.NET Core MVC app for viewing TSA coding problems and collecting submissions.

## Run locally

```bash
cd TsaSubmissions.Web
dotnet run
```

Default seeded accounts:

- Participant: `participant` / `participant123!`
- Judge: `judge` / `judge123!`

## Kubernetes (K3s)

Deploy PostgreSQL and app manifests from `./k8s`.
Update `k8s/secrets.yaml` with environment-specific credentials before applying manifests.
