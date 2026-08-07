import { useEffect, useRef, useState } from "react";
import { ImagePlus, Upload, X } from "lucide-react";
import { cn } from "@/lib/cn";
import { Label } from "@/ui";

interface ImageInputProps {
  label: string;
  /** The API's create validators mark the image NotNull, so creates require one. */
  required?: boolean;
  error?: string;
  /** Existing picture when editing. */
  currentUrl?: string | null;
  value?: File | null;
  onChange: (file: File | null) => void;
  accept?: string;
  /** The server's own cap, repeated here so the user is not told after upload. */
  maxMb?: number;
  className?: string;
}

export function ImageInput({
  label,
  required,
  error,
  currentUrl,
  value,
  onChange,
  accept = "image/*",
  maxMb = 5,
  className,
}: ImageInputProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [localError, setLocalError] = useState<string | null>(null);

  useEffect(() => {
    if (!value) {
      setPreview(null);
      return;
    }

    const url = URL.createObjectURL(value);
    setPreview(url);

    // Object URLs stay alive until revoked; without this each re-pick leaks one.
    return () => URL.revokeObjectURL(url);
  }, [value]);

  function handle(file: File | null) {
    setLocalError(null);

    if (file && file.size > maxMb * 1024 * 1024) {
      setLocalError(`Fayl ${maxMb} MB-dan böyük ola bilməz.`);
      return;
    }

    onChange(file);
  }

  const shown = preview ?? currentUrl ?? null;
  const message = error ?? localError;

  return (
    <div className={cn("flex flex-col gap-1.5", className)}>
      <Label required={required}>{label}</Label>

      <div className="flex items-center gap-3">
        <div
          className={cn(
            "flex size-20 shrink-0 items-center justify-center overflow-hidden rounded-md border border-border bg-muted",
            message && "border-danger",
          )}
        >
          {shown ? (
            <img src={shown} alt="" className="size-full object-cover" />
          ) : (
            <ImagePlus className="size-5 text-muted-foreground" aria-hidden />
          )}
        </div>

        <div className="flex flex-col gap-1.5">
          <button
            type="button"
            onClick={() => inputRef.current?.click()}
            className="inline-flex h-8 items-center gap-2 rounded-md border border-border px-3 text-xs font-medium hover:bg-muted"
          >
            <Upload className="size-3.5" />
            {shown ? "Dəyiş" : "Şəkil seç"}
          </button>

          {value && (
            <button
              type="button"
              onClick={() => handle(null)}
              className="inline-flex items-center gap-1 text-xs text-muted-foreground hover:text-danger"
            >
              <X className="size-3" />
              Seçimi ləğv et
            </button>
          )}

          <p className="text-[11px] text-muted-foreground">
            JPG / PNG · maks. {maxMb} MB
          </p>
        </div>
      </div>

      <input
        ref={inputRef}
        type="file"
        accept={accept}
        className="sr-only"
        onChange={(event) => handle(event.target.files?.[0] ?? null)}
      />

      {message && <p className="text-xs text-danger">{message}</p>}
    </div>
  );
}
