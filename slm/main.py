import logging
import os
from typing import Any

import httpx
from dotenv import load_dotenv
from fastapi import FastAPI
from pydantic import BaseModel, Field

load_dotenv()

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s %(levelname)s %(name)s %(message)s",
)

logger = logging.getLogger("friday.slm")

BACKEND_URL = os.getenv("BACKEND_URL", "http://localhost:8080").rstrip("/")
OLLAMA_URL = os.getenv("SLM_OLLAMA_URL", "http://localhost:11434").rstrip("/")
MODEL = os.getenv("SLM_MODEL", "gemma3:4b")

app = FastAPI(title="Friday SLM", version="0.1.0")


class AnalyzeRequest(BaseModel):
    text: str = Field(min_length=1, max_length=4000)


class AnalyzeResponse(BaseModel):
    model: str
    response: str
    backend_context_available: bool


@app.get("/health")
async def health() -> dict[str, str]:
    return {"service": "slm", "status": "ok"}


@app.post("/analyze", response_model=AnalyzeResponse)
async def analyze(request: AnalyzeRequest) -> AnalyzeResponse:
    context, context_available = await load_backend_context()
    prompt = build_prompt(request.text, context)
    response = await ask_ollama(prompt)

    return AnalyzeResponse(
        model=MODEL,
        response=response,
        backend_context_available=context_available,
    )


async def load_backend_context() -> tuple[dict[str, Any], bool]:
    try:
        async with httpx.AsyncClient(timeout=5.0) as client:
            response = await client.get(f"{BACKEND_URL}/api/context")
            response.raise_for_status()
            return response.json(), True
    except Exception as exc:
        logger.warning("Could not load backend context: %s", exc)
        return {"availableTables": []}, False


def build_prompt(user_text: str, context: dict[str, Any]) -> str:
    tables = ", ".join(context.get("availableTables", [])) or "none"
    guidance = context.get("guidance", "Answer clearly for an incoming university student.")

    return (
        "You are Friday, a helpful assistant for incoming university students.\n"
        f"Database tables available: {tables}\n"
        f"Guidance: {guidance}\n"
        "Answer directly, cite missing information honestly, and keep the tone professional.\n\n"
        f"Student message: {user_text}"
    )


async def ask_ollama(prompt: str) -> str:
    payload = {
        "model": MODEL,
        "prompt": prompt,
        "stream": False,
    }

    try:
        async with httpx.AsyncClient(timeout=60.0) as client:
            response = await client.post(f"{OLLAMA_URL}/api/generate", json=payload)
            response.raise_for_status()
            body = response.json()
            generated = body.get("response", "").strip()
            if generated:
                return generated
    except Exception as exc:
        logger.warning("Ollama request failed: %s", exc)

    return "I could not reach the local language model yet. The request was received and the backend context path is configured."
