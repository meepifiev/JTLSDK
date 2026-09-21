import { build } from "esbuild";
import { fileURLToPath } from "node:url";
import path from "node:path";

const root = path.dirname(fileURLToPath(import.meta.url));
const targets = {
  yandex: "../Runtime/Platforms/YandexGames/Plugins/WebGL/jtlsdk.yandex.jspre",
  youtube: "../Runtime/Platforms/YouTubePlayables/Plugins/WebGL/jtlsdk.youtube.jspre",
};

const requested = process.argv.slice(2);
const names = requested.length > 0 ? requested : Object.keys(targets);

for (const name of names) {
  const outfile = targets[name];

  if (outfile === undefined) {
    throw new Error(`Unknown bridge target: ${name}`);
  }

  await build({
    entryPoints: [path.join(root, "src", "entry", `${name}.ts`)],
    bundle: true,
    format: "iife",
    target: "es2017",
    outfile: path.join(root, outfile),
    banner: { js: `// JTL SDK bridge (${name}). Generated from Bridge~/src, do not edit.` },
    legalComments: "none",
    logLevel: "info",
  });
}
