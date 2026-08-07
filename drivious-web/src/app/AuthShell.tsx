import { ThemeToggle } from "@/theme";

/**
 * The frame the signed-out screens share: brand and theme switch on top, the
 * form centred under them, and a quiet panel on the right for wide viewports.
 * Login and registration differ only in the form, so only the form lives in
 * those files.
 */
export function AuthShell({ children }: { children: React.ReactNode }) {
  return (
    <div className="grid min-h-dvh lg:grid-cols-2">
      {/* Left: the form */}
      <div className="flex flex-col px-6 py-8 sm:px-10">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2 font-semibold tracking-tight">
            <span className="flex size-7 items-center justify-center rounded-md bg-primary text-primary-foreground">
              <svg viewBox="0 0 24 24" fill="none" className="size-4">
                <path
                  d="M4 16.5V13l1.6-4.4A2 2 0 0 1 7.5 7.2h9a2 2 0 0 1 1.9 1.4L20 13v3.5"
                  stroke="currentColor"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
                <circle cx="7.5" cy="16.5" r="1.6" stroke="currentColor" strokeWidth="1.8" />
                <circle cx="16.5" cy="16.5" r="1.6" stroke="currentColor" strokeWidth="1.8" />
              </svg>
            </span>
            Drivious
          </div>

          <ThemeToggle />
        </div>

        <div className="mx-auto flex w-full max-w-sm flex-1 flex-col justify-center py-10">
          {children}
        </div>
      </div>

      {/* Right: a quiet panel rather than a stock photo — it stays legible in
          both themes and does not compete with the form. */}
      <div className="relative hidden overflow-hidden border-l border-border bg-surface-sunken lg:block">
        <div
          className="absolute inset-0 opacity-[0.35]"
          style={{
            backgroundImage:
              "linear-gradient(to right, var(--border) 1px, transparent 1px), linear-gradient(to bottom, var(--border) 1px, transparent 1px)",
            backgroundSize: "44px 44px",
          }}
          aria-hidden
        />

        <div className="relative flex h-full flex-col justify-end p-12">
          <blockquote className="max-w-md">
            <p className="text-xl font-medium leading-relaxed tracking-tight">
              Maşınlar, sürücülər, xərclər və bitmə tarixləri — hamısı bir yerdə.
            </p>
            <footer className="mt-4 text-sm text-muted-foreground">
              Sığorta, vəsiqə, texniki baxış və servis tarixləri avtomatik izlənir; bitməzdən
              əvvəl sizə xəbər verilir.
            </footer>
          </blockquote>
        </div>
      </div>
    </div>
  );
}
