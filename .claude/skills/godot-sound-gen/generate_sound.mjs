#!/usr/bin/env node
// Generate a retro SFX .wav via jsfxr (local, offline — no API key/cost).
// Mirrors godot-sprite-gen/generate_sprite.py: one repeatable command → asset in Assets/.
import { execFileSync } from "node:child_process";
import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const SKILL_DIR = dirname(fileURLToPath(import.meta.url));
const PRESETS = [
  "pickupCoin", "laserShoot", "explosion", "powerUp",
  "hitHurt", "jump", "blipSelect", "synth", "tone", "click", "random",
];

main();

async function main() {
  const args = parseArgs(process.argv.slice(2));
  if (args.help || !args.name || (!args.preset && !args.params)) {
    usage();
    process.exit(args.help ? 0 : 1);
  }

  const sfxr = await loadSfxr();
  if (args.seed !== undefined) Math.random = mulberry32(Number(args.seed));

  let params;
  if (args.params) {
    // b58 encodes only the p_* params + wave_type, not render config — backfill jsfxr defaults.
    params = sfxr.b58decode(args.params);
    params.sample_rate ??= 44100;
    params.sample_size ??= 8;
    params.sound_vol ??= 0.25;
  } else {
    params = sfxr.generate(args.preset);
  }
  const bytes = Buffer.from(sfxr.toWave(params).dataURI.split(",")[1], "base64");

  const outDir = resolve(args["out-dir"] ?? "Assets/Sounds");
  mkdirSync(outDir, { recursive: true });
  const outPath = join(outDir, `${args.name}.wav`);
  writeFileSync(outPath, bytes);

  console.log(`wrote: ${outPath} (${bytes.length} bytes)`);
  console.log(`params (reproduce/tweak via --params): ${sfxr.b58encode(params)}`);
}

// jsfxr resolves from this script's dir (SKILL_DIR/node_modules). Lazy-install on first run.
async function loadSfxr() {
  try {
    return (await import("jsfxr")).sfxr;
  } catch {
    console.error("jsfxr not found — installing into skill dir (one-time)...");
    execFileSync("npm", ["install", "--no-audit", "--no-fund"], { cwd: SKILL_DIR, stdio: "inherit" });
    return (await import("jsfxr")).sfxr;
  }
}

// Seeded PRNG so --seed makes runs byte-reproducible (jsfxr.generate uses Math.random).
function mulberry32(seed) {
  let a = seed >>> 0;
  return () => {
    a = (a + 0x6d2b79f5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

function parseArgs(argv) {
  const args = {};
  for (let i = 0; i < argv.length; i++) {
    const a = argv[i];
    if (a === "--help" || a === "-h") args.help = true;
    else if (a.startsWith("--")) args[a.slice(2)] = argv[++i];
  }
  return args;
}

function usage() {
  console.log(`Generate a retro SFX .wav via jsfxr.

Usage:
  node generate_sound.mjs --preset <preset> --name <base> [--seed <int>] [--out-dir <dir>]
  node generate_sound.mjs --params <b58> --name <base> [--out-dir <dir>]

Flags:
  --preset   one of: ${PRESETS.join(", ")}
  --name     output filename base -> <out-dir>/<name>.wav   (required)
  --seed     integer; makes generation byte-reproducible
  --params   b58 param string (from a prior run) to reproduce/tweak; overrides --preset
  --out-dir  default: Assets/Sounds

Prints the b58 param string so a good sound can be reproduced via --params.`);
}
