import os, time, re
from fastapi import FastAPI, Request, Response, Depends, Body, Header
from starlette.staticfiles import StaticFiles
from starlette.middleware.cors import CORSMiddleware
from prometheus_client import (
    Counter, Histogram, Summary, Gauge,
    generate_latest, CONTENT_TYPE_LATEST
)

# -------------------------
# Create app FIRST
# -------------------------
app = FastAPI(title="LearnSphere Backend", version="0.4.0")

# CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

# -------------------------
# Config / env
# -------------------------
ASSET_DIR = os.getenv("ASSET_DIR", "/app/assets")
ASSET_BASE_URL = os.getenv("ASSET_BASE_URL", "/assets")
MODEL_RELATIVE_PATH = os.getenv("MODEL_RELATIVE_PATH", "models/test.glb")
MODEL_URL = f"{ASSET_BASE_URL}/{MODEL_RELATIVE_PATH}".replace("//", "/")

CONFIG = {
    "asset_base_url": ASSET_BASE_URL,
    "model_url": MODEL_URL,
    "notes": "Change MODEL_RELATIVE_PATH in .env to switch the model file"
}

# Static files
app.mount("/assets", StaticFiles(directory=ASSET_DIR), name="assets")

# Optional API token
API_TOKEN = os.getenv("API_TOKEN", "")

def require_auth(authorization: str | None = Header(default=None)):
    if API_TOKEN:
        if not authorization or authorization.replace("Bearer ", "") != API_TOKEN:
            return Response(status_code=401, content=b'{"error":"unauthorized"}', media_type="application/json")
    return True

# -------------------------
# Prometheus metrics
# -------------------------
REQ_COUNT = Counter("ls_http_requests_total", "HTTP requests", ["route","method","code"])
REQ_LAT   = Histogram("ls_http_request_seconds", "HTTP request latency seconds", ["route","method"])

# Client KPIs (existing)
TTFR       = Summary("ls_client_ttfr_seconds", "Unity time to first render (s)", ["client_id"])
FPS_GAUGE  = Gauge("ls_client_fps", "Last reported FPS from Unity", ["client_id"])
EVENTS_TOT = Counter("ls_client_events_total", "Client events total", ["event_name"])

# NEW: Client network KPIs (Unity)
CLIENT_RTT = Histogram(
    "ls_client_http_rtt_seconds",
    "Client end-to-end HTTP RTT (seconds)",
    ["client_id"]
)

CLIENT_JITTER = Gauge(
    "ls_client_http_jitter_seconds",
    "Client HTTP jitter (seconds)",
    ["client_id"]
)

CLIENT_ERRORS = Counter(
    "ls_client_http_errors_total",
    "Client HTTP errors (packet loss proxy)",
    ["client_id"]
)

# Bytes sent (existing)
RESP_BYTES = Counter(
    "ls_http_response_bytes_total",
    "Total bytes sent in HTTP responses",
    ["route","method","code","ext"]
)

# -------------------------
# Middleware
# -------------------------
@app.middleware("http")
async def metrics_middleware(request: Request, call_next):
    t0 = time.time()
    response = await call_next(request)
    dt = time.time() - t0

    route  = request.url.path
    method = request.method
    code   = response.status_code

    REQ_LAT.labels(route=route, method=method).observe(dt)
    REQ_COUNT.labels(route=route, method=method, code=code).inc()

    bytes_sent = 0
    if method == "GET":
        cr = response.headers.get("content-range")
        if cr and code == 206:
            m = re.match(r"bytes (\d+)-(\d+)/(\d+)", cr)
            if m:
                start, end = int(m.group(1)), int(m.group(2))
                bytes_sent = max(0, end - start + 1)
        else:
            cl = response.headers.get("content-length")
            if cl and cl.isdigit():
                bytes_sent = int(cl)

        if bytes_sent == 0 and route.startswith(ASSET_BASE_URL + "/"):
            rel = route[len(ASSET_BASE_URL) + 1:]
            fs_path = os.path.normpath(os.path.join(ASSET_DIR, rel))
            if fs_path.startswith(os.path.normpath(ASSET_DIR)) and os.path.isfile(fs_path):
                try:
                    bytes_sent = os.path.getsize(fs_path)
                except OSError:
                    pass

    if bytes_sent > 0:
        ext = os.path.splitext(route)[1].lower() or ""
        RESP_BYTES.labels(route=route, method=method, code=str(code), ext=ext).inc(bytes_sent)

    return response

# -------------------------
# Endpoints
# -------------------------
@app.get("/healthz")
def health():
    return {"status": "ok"}

@app.get("/config")
def get_config():
    return CONFIG

@app.get("/metrics")
def metrics():
    return Response(content=generate_latest(), media_type=CONTENT_TYPE_LATEST)

@app.post("/api/v1/events")
async def post_event(payload: dict = Body(...), auth=Depends(require_auth)):
    client_id  = str(payload.get("client_id", "unknown"))
    event_name = str(payload.get("event_name", "unknown")).lower()

    EVENTS_TOT.labels(event_name=event_name).inc()

    if event_name == "ttfr":
        try:
            TTFR.labels(client_id=client_id).observe(float(payload.get("value", 0)))
        except (TypeError, ValueError):
            pass

    if event_name == "heartbeat" and "fps" in payload:
        try:
            FPS_GAUGE.labels(client_id=client_id).set(float(payload["fps"]))
        except (TypeError, ValueError):
            pass

    if event_name == "http_rtt_seconds":
        try:
            CLIENT_RTT.labels(client_id=client_id).observe(float(payload.get("value", 0)))
        except (TypeError, ValueError):
            pass

    if event_name == "http_jitter_seconds":
        try:
            CLIENT_JITTER.labels(client_id=client_id).set(float(payload.get("value", 0)))
        except (TypeError, ValueError):
            pass

    if event_name == "http_error_total":
        CLIENT_ERRORS.labels(client_id=client_id).inc()

    return {"ok": True}
