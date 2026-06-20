import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Friday Dashboard",
  description: "University Discord operations dashboard"
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en" className="dark">
      <body>{children}</body>
    </html>
  );
}
