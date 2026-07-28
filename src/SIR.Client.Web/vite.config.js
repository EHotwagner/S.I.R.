import { defineConfig } from "vite";
import { resolve } from "node:path";

export default defineConfig({
  root: resolve(import.meta.dirname),
  base: "./",
  build: {
    outDir: resolve(import.meta.dirname, "../../artifacts/client"),
    emptyOutDir: true,
    rollupOptions: {
      output: {
        entryFileNames: "content/sir-client/v1/app.js",
        chunkFileNames: "content/sir-client/v1/[name]-[hash].js",
        assetFileNames: (assetInfo) =>
          assetInfo.names.some((name) => name.endsWith(".css"))
            ? "content/sir-client/v1/styles.css"
            : "content/sir-client/v1/[name][extname]",
      },
    },
  },
  worker: {
    rollupOptions: {
      output: {
        entryFileNames: "content/sir-client/v1/worker.js",
        chunkFileNames: "content/sir-client/v1/[name]-[hash].js",
      },
    },
  },
});
