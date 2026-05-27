#!/usr/bin/env sh
set -eu

HTML_DIR="/usr/share/nginx/html"
CONFIG_FILE="$HTML_DIR/env.js"

mkdir -p "$HTML_DIR"

cat > "$CONFIG_FILE" <<EOF
// Generated at container startup
window.APP_CONFIG = {
  API_URL: "${API_URL:-http://localhost:5025/PIQI/ScoreAuditMessage}",
  PIQI_MODEL_MNEMONIC: "${PIQI_MODEL_MNEMONIC:-PAT_CLINICAL_V1}",
  EVALUATION_RUBRIC_MNEMONIC: "${EVALUATION_RUBRIC_MNEMONIC:-Basic_VA_Lab}"
};
EOF

echo "Wrote runtime config to $CONFIG_FILE"

exec nginx -g "daemon off;"