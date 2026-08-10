"use client";

import { Moon, Sun } from "lucide-react";
import { useTheme } from "next-themes";
import { Button } from "@/components/ui/button";
import { THEME_TOGGLE_TEXTS } from "./constants";

// Both icons are always in the DOM; visibility is pure CSS via the `dark:` (class) variant,
// so nothing depends on the theme during render — that avoids a hydration mismatch (the server
// can't know the user's stored theme). resolvedTheme is only read on click (client-only).
export function ThemeToggle() {
  const { resolvedTheme, setTheme } = useTheme();

  return (
    <Button
      variant="outline"
      size="icon"
      aria-label={THEME_TOGGLE_TEXTS.label}
      onClick={() => setTheme(resolvedTheme === "dark" ? "light" : "dark")}
    >
      <Sun className="hidden dark:block" />
      <Moon className="block dark:hidden" />
    </Button>
  );
}
