/** @type {import('next').NextConfig} */
const nextConfig = {
  output: "standalone",
  async rewrites() {
    const backendUrl = (process.env.BACKEND_URL ?? "http://localhost:8080")
      .replace(/\/+$/, "");

    return [{
      source: "/api/:path*",
      destination: `${backendUrl}/api/:path*`,
    }];
  },
};

export default nextConfig;
