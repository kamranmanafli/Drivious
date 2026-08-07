/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Origin of the ASP.NET API. Must appear in its Cors:AllowedOrigins list. */
  readonly VITE_API_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
