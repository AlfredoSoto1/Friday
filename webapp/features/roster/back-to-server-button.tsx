"use client";

import { useRouter } from "next/navigation";
import { ArrowLeft } from "lucide-react";

import { Button } from "@/components/ui/button";

export function BackToServerButton(): React.ReactElement {
  const router = useRouter();

  return (
    <Button variant="outline" size="sm" onClick={(): void => router.back()}>
      <ArrowLeft />
      Back to server
    </Button>
  );
}
