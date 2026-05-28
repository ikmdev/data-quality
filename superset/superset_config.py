# Superset custom config for running under /superset/
APPLICATION_ROOT = '/superset/'
WTF_CSRF_TIME_LIMIT = None
ENABLE_PROXY_FIX = True

# Prevent redirect loops
WTF_CSRF_ENABLED = True
SESSION_COOKIE_HTTPONLY = True
SESSION_COOKIE_SECURE = False
SESSION_COOKIE_SAMESITE = 'Lax'

# Base URL config
SUPERSET_WEBSERVER_PROTOCOL = 'http'