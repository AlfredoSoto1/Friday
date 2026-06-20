import type { Config } from "tailwindcss";

const config: Config = {
  darkMode: "class",
  content: [
    "./app/**/*.{ts,tsx}",
    "./components/**/*.{ts,tsx}",
    "./features/**/*.{ts,tsx}",
    "./hooks/**/*.{ts,tsx}",
    "./lib/**/*.{ts,tsx}",
    "./widgets/**/*.{ts,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        border: "#30363d",
        background: "#0d1117",
        foreground: "#f0f6fc",
        muted: "#21262d",
        "muted-foreground": "#8b949e",
        card: "#161b22",
        "card-foreground": "#f0f6fc",
        popover: "#161b22",
        "popover-foreground": "#f0f6fc",
        primary: "#5865f2",
        "primary-foreground": "#ffffff",
        secondary: "#21262d",
        "secondary-foreground": "#f0f6fc",
        accent: "#21262d",
        "accent-foreground": "#f0f6fc",
        destructive: "#f85149",
        input: "#30363d",
        ring: "#5865f2",
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
