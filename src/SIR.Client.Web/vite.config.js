import { defineConfig } from "vite";
import { resolve } from "node:path";

export default defineConfig({
  root: resolve(import.meta.dirname),
  base: "./",
  build: {
    minify: "terser",
    terserOptions: {
      compress: { passes: 3 },
      mangle: { properties: false },
      format: { comments: false },
    },
    manifest: true,
    outDir: resolve(import.meta.dirname, "../../artifacts/client"),
    emptyOutDir: true,
    rollupOptions: {
      output: {
        entryFileNames: (chunk) =>
          chunk.name === "index"
            ? "content/sir-client/v1/app.js"
            : "content/sir-client/v1/[name]-[hash].js",
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
        entryFileNames:
          "engines/0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20/worker.js",
        chunkFileNames:
          "engines/0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20/[name]-[hash].js",
      },
    },
  },
});
