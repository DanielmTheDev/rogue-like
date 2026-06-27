#!/usr/bin/env python3
"""Generate a Godot game sprite via Gemini image API + ImageMagick chromakey.

Pipeline (see docs/ASSET_PIPELINE.md):
  1. Ask Gemini for a 16-bit pixel-art sprite on a solid #00FF00 background.
  2. Scale to NxN (nearest-neighbour) and key out the green with ImageMagick.
  3. Write the final transparent PNG to Assets/<Name>/<name>.png.

Usage:
  GEMINI_API_KEY=... python3 generate_sprite.py \
      --name Sword \
      --desc "a steel short sword, blade pointing up"

Optional:
  --size 32                 final sprite size (default 32)
  --out-dir Assets          assets root (default ./Assets)
  --model gemini-2.5-flash-image
  --fuzz 10%                chromakey tolerance (default 10%)
  --keep-raw PATH           also save the un-keyed raw PNG here for re-keying
"""
import argparse
import base64
import json
import os
import subprocess
import sys
import tempfile
import urllib.request

PROMPT_TEMPLATE = (
    "A pixel art sprite of {desc} for a roguelike dungeon crawler. "
    "32x32 pixels, top-down perspective. Retro 16-bit pixel art style, "
    "limited color palette. SOLID VIBRANT GREEN (#00FF00) BACKGROUND, "
    "no shadows, no effects, no text."
)


def gemini_image(prompt: str, api_key: str, model: str) -> bytes:
    """Call Gemini generateContent and return the first inline image bytes."""
    url = f"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent"
    body = json.dumps({"contents": [{"parts": [{"text": prompt}]}]}).encode()
    req = urllib.request.Request(
        url,
        data=body,
        headers={"Content-Type": "application/json", "x-goog-api-key": api_key},
    )
    try:
        with urllib.request.urlopen(req, timeout=120) as resp:
            payload = json.load(resp)
    except urllib.error.HTTPError as e:
        sys.exit(f"Gemini HTTP {e.code}: {e.read().decode(errors='replace')}")

    for cand in payload.get("candidates", []):
        for part in cand.get("content", {}).get("parts", []):
            inline = part.get("inlineData") or part.get("inline_data")
            if inline and inline.get("data"):
                return base64.b64decode(inline["data"])
    sys.exit(f"No image in response: {json.dumps(payload)[:800]}")


def detect_bg(raw_png: str) -> str:
    """Sample a corner pixel as the background color.

    Gemini ignores the exact #00FF00 we ask for and emits its own flat
    backdrop (often a muted lime ~#ACD160), so we key out whatever color
    actually sits in the corner rather than a hardcoded green.
    """
    out = subprocess.run(
        ["convert", raw_png, "-format", "%[pixel:p{0,0}]", "info:"],
        check=True, capture_output=True, text=True,
    )
    return out.stdout.strip()


def chromakey(raw_png: str, out_png: str, size: int, fuzz: str, bg: str) -> None:
    subprocess.run(
        ["convert", raw_png, "-scale", f"{size}x{size}",
         "-fuzz", fuzz, "-transparent", bg, out_png],
        check=True,
    )


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--name", required=True, help="Entity name -> Assets/<Name>/")
    ap.add_argument("--desc", required=True, help="Sprite description for the prompt")
    ap.add_argument("--size", type=int, default=32)
    ap.add_argument("--out-dir", default="Assets")
    ap.add_argument("--model", default="gemini-2.5-flash-image")
    ap.add_argument("--fuzz", default="20%")
    ap.add_argument("--bg", default=None,
                    help="Chromakey color (default: auto-detect from corner pixel)")
    ap.add_argument("--keep-raw", default=None)
    args = ap.parse_args()

    api_key = os.environ.get("GEMINI_API_KEY")
    if not api_key:
        sys.exit("GEMINI_API_KEY not set in environment")

    prompt = PROMPT_TEMPLATE.format(desc=args.desc)
    print(f"Prompt: {prompt}")
    image = gemini_image(prompt, api_key, args.model)

    out_dir = os.path.join(args.out_dir, args.name)
    os.makedirs(out_dir, exist_ok=True)
    out_png = os.path.join(out_dir, f"{args.name.lower()}.png")

    raw_path = args.keep_raw
    raw_ctx = open(raw_path, "wb") if raw_path else tempfile.NamedTemporaryFile(
        suffix=".png", delete=False)
    with raw_ctx as f:
        f.write(image)
        raw_file = f.name
    print(f"Raw image: {raw_file} ({len(image)} bytes)")

    bg = args.bg or detect_bg(raw_file)
    print(f"Chromakey color: {bg} (fuzz {args.fuzz})")
    chromakey(raw_file, out_png, args.size, args.fuzz, bg)
    print(f"Wrote {out_png}")
    print("Next: open in Godot, it auto-imports. Bind to a Sprite2D in a .tscn.")


if __name__ == "__main__":
    main()
