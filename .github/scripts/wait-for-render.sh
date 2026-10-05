#!/usr/bin/env bash
# Usage: wait-for-render.sh <service-id> <deploy-id> <health-url>
#
# With RENDER_API_KEY set, polls the Render API until the deploy is live and
# fails the job if the build or release fails. Without it, falls back to
# waiting for the health endpoint to answer.
set -euo pipefail

SERVICE_ID="$1"
DEPLOY_ID="$2"
HEALTH_URL="$3"
TIMEOUT_SECONDS=${TIMEOUT_SECONDS:-900}
deadline=$(( $(date +%s) + TIMEOUT_SECONDS ))

if [ -n "${RENDER_API_KEY:-}" ] && [ -n "$DEPLOY_ID" ]; then
  echo "Waiting for Render deploy $DEPLOY_ID on $SERVICE_ID..."
  while [ "$(date +%s)" -lt "$deadline" ]; do
    status=$(curl -fsS \
      -H "Authorization: Bearer $RENDER_API_KEY" \
      -H "Accept: application/json" \
      "https://api.render.com/v1/services/$SERVICE_ID/deploys/$DEPLOY_ID" | jq -r '.status')
    echo "  status: $status"
    case "$status" in
      live) break ;;
      build_failed|update_failed|pre_deploy_failed|canceled|deactivated)
        echo "::error::Render deploy $DEPLOY_ID ended with status '$status'."
        exit 1 ;;
    esac
    sleep 15
  done
  if [ "$status" != "live" ]; then
    echo "::error::Timed out waiting for Render deploy $DEPLOY_ID."
    exit 1
  fi
else
  echo "::warning::RENDER_API_KEY not set; only checking $HEALTH_URL (cannot confirm the new version is the one answering)."
fi

# Free-plan services sleep when idle, so allow time for a cold start
echo "Checking $HEALTH_URL..."
while [ "$(date +%s)" -lt "$deadline" ]; do
  if curl -fsS --max-time 30 "$HEALTH_URL" > /dev/null; then
    echo "Healthy."
    exit 0
  fi
  sleep 10
done

echo "::error::$HEALTH_URL did not become healthy in time."
exit 1
