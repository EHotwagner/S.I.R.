import { defineConfig } from "vite";
import { resolve } from "node:path";

export default defineConfig({
  root: resolve(import.meta.dirname),
  base: "./",
  build: {
    outDir: resolve(import.meta.dirname, "../../artifacts/client"),
    emptyOutDir: true,
  },
});
