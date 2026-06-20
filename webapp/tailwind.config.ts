import type { Config } from "tailwindcss";

const config: Config = {
  darkMode: ["class"],
  content: ["./app/**/*.{ts,tsx}", "./components/**/*.{ts,tsx}", "./lib/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors: {
        border: "#30363d",
        background: "#0d1117",
        foreground: "#f0f6fc",
        muted: "#8b949e",
        panel: "#161b22",
        panelSoft: "#21262d",
        discord: "#5865f2",
        success: "#3fb950",
        warning: "#d29922",
        danger: "#f85149"
      },
      boxShadow: {
        panel: "0 0 0 1px rgba(48, 54, 61, 0.9)"
      }
    }
  },
  plugins: []
};

export default config;
