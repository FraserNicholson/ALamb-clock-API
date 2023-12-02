# ALamb-clock-API

## Running the Worker locally

Build docker image locally

```bash
docker build . -f .\src\Worker\Dockerfile -t worker --build-arg newrelicProfile={{$newRelicProfile}} --build-arg newrelicLicenseKey={{$newRelicLicenseKey}} --build-arg newrelicEnvironment=local
```