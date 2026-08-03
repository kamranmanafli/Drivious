/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Origin of the ASP.NET API. Must appear in its Cors:AllowedOrigins list. */
  readonly VITE_API_URL: string;
  /** "true" runs on in-memory sample data with no backend. */
  readonly VITE_DEMO: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
