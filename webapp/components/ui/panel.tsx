import { cn } from "@/lib/utils";

type PanelProps = React.HTMLAttributes<HTMLDivElement>;

export function Panel({ className, ...props }: PanelProps) {
  return <div className={cn("rounded-md border border-border bg-panel shadow-panel", className)} {...props} />;
}
