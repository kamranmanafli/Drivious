import axios, { type AxiosResponse } from "axios";
import { toast } from "sonner";
import type { ApiResponse } from "./types";
import { translate } from "./messages";

const TOKEN_KEY = "drivious.accessToken";
const REFRESH_KEY = "drivious.refreshToken";

export const baseURL: string = import.meta.env.VITE_API_URL ?? "";

export const tokenStore = {
  get access() {
    return localStorage.getItem(TOKEN_KEY);
  },
  get refresh() {
    return localStorage.getItem(REFRESH_KEY);
  },
  save(accessToken: string, refreshToken: string) {
    localStorage.setItem(TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_KEY, refreshToken);
  },
  clear() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
  },
};

// Long, but deliberate. The API sleeps when idle on its current plan and the
// request that wakes it waits for the whole start-up. Anything near the usual
// few seconds would abort exactly the request a returning visitor makes first.
const REQUEST_TIMEOUT_MS = 60_000;

/** How long a request may run before the wake-up notice appears. */
const SLOW_AFTER_MS = 4_000;

export const api = axios.create({ baseURL, timeout: REQUEST_TIMEOUT_MS });

/**
 * A cold start looks exactly like a frozen page: nothing moves for twenty
 * seconds. This says what is happening instead of leaving the screen silent —
 * counted across requests, so a screen firing five of them shows one notice.
 */
let inFlight = 0;
let slowTimer: ReturnType<typeof setTimeout> | null = null;
let slowToast: string | number | null = null;

function requestStarted() {
  inFlight += 1;

  if (slowTimer === null && slowToast === null) {
    slowTimer = setTimeout(() => {
      slowTimer = null;

      if (inFlight > 0) {
        slowToast = toast.loading("Server oyanır — bu, bir neçə saniyə çəkə bilər.");
      }
    }, SLOW_AFTER_MS);
  }
}

function requestSettled() {
  inFlight = Math.max(0, inFlight - 1);

  if (inFlight > 0) return;

  if (slowTimer !== null) {
    clearTimeout(slowTimer);
    slowTimer = null;
  }

  if (slowToast !== null) {
    toast.dismiss(slowToast);
    slowToast = null;
  }
}

api.interceptors.request.use((config) => {
  const token = tokenStore.access;

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  requestStarted();

  return config;
});

/** A refresh already in flight. Without this, five requests failing at once each
 *  start their own, and four of them present a token the first already rotated. */
let refreshing: Promise<string> | null = null;

let onSessionLost: () => void = () => {};

export function setSessionLostHandler(handler: () => void) {
  onSessionLost = handler;
}

async function refreshAccessToken(): Promise<string> {
  const refreshToken = tokenStore.refresh;

  if (!refreshToken) throw new Error("No refresh token stored.");

  // A bare call, not `api` — its request interceptor would attach the expired
  // access token, and its response interceptor would try to refresh the refresh.
  const { data } = await axios.post<ApiResponse<{ accessToken: string; refreshToken: string }>>(
    `${baseURL}/api/auths/refresh`,
    { refreshToken },
  );

  const tokens = data.data;
  if (!tokens) throw new Error(data.message);

  tokenStore.save(tokens.accessToken, tokens.refreshToken);

  return tokens.accessToken;
}

api.interceptors.response.use(
  (response) => {
    requestSettled();
    return response;
  },
  async (error) => {
    requestSettled();

    const original = error.config as (typeof error.config & { _retried?: boolean }) | undefined;

    // 403 means the role is not allowed; a fresh token would be refused too.
    if (error.response?.status !== 401 || !original || original._retried) {
      return Promise.reject(error);
    }

    // The refresh endpoint answering 401 means the refresh token is gone as well.
    if (original.url?.includes("/api/auths/refresh")) {
      tokenStore.clear();
      onSessionLost();
      return Promise.reject(error);
    }

    original._retried = true;

    try {
      refreshing =
        refreshing ??
        refreshAccessToken().finally(() => {
          refreshing = null;
        });

      const token = await refreshing;
      original.headers.Authorization = `Bearer ${token}`;

      return await api(original);
    } catch (refreshError) {
      tokenStore.clear();
      onSessionLost();
      return Promise.reject(refreshError);
    }
  },
);

/**
 * Builds the multipart body the three upload endpoints expect. Undefined, null
 * and empty values are skipped so a PATCH carries only what actually changed —
 * sending an empty string would overwrite the stored value with one.
 */
export function toFormData(values: Record<string, unknown>): FormData {
  const form = new FormData();

  for (const [key, value] of Object.entries(values)) {
    if (value === undefined || value === null || value === "") continue;

    if (value instanceof File) {
      form.append(key, value);
    } else if (value instanceof Date) {
      form.append(key, value.toISOString());
    } else {
      form.append(key, String(value));
    }
  }

  return form;
}

/** Unwraps { success, message, data }; a `success: false` body becomes a throw. */
export function unwrap<T>(response: AxiosResponse<ApiResponse<T>>): T {
  const body = response.data;

  if (body?.success === false) {
    throw new ApiError(translate(body.message), body.message);
  }

  return body?.data as T;
}

/** Returns the confirmation text, already translated. */
export function unwrapMessage(response: AxiosResponse<ApiResponse<unknown>>): string {
  const body = response.data;

  if (body?.success === false) {
    throw new ApiError(translate(body.message), body.message);
  }

  return translate(body?.message);
}

export class ApiError extends Error {
  /** The untranslated text, kept for logs and bug reports. */
  readonly original: string;

  constructor(message: string, original: string) {
    super(message);
    this.name = "ApiError";
    this.original = original;
  }
}

/**
 * The sentence to show when a call fails.
 *
 * FluentValidation answers with a problem-details body whose `errors` object
 * holds one array per field; the services answer with their own `message`.
 * Both are translated, and a 403 is spelled out because the raw body for one
 * is empty.
 */
export function errorMessage(error: unknown): string {
  if (error instanceof ApiError) return error.message;

  if (axios.isAxiosError(error)) {
    const status = error.response?.status;
    const data = error.response?.data as
      | { errors?: Record<string, string[]>; message?: string; title?: string }
      | undefined;

    if (data?.errors) {
      return Object.values(data.errors).flat().map(translate).join(" ");
    }

    if (data?.message) return translate(data.message);

    if (status === 401) return "Sessiya bitib. Yenidən daxil olun.";
    if (status === 403) return "Bu əməliyyat üçün icazəniz yoxdur.";
    if (status === 404) return "Məlumat tapılmadı.";
    if (status === 413) return "Fayl həddindən böyükdür.";
    if (status && status >= 500) return "Serverdə xəta baş verdi.";

    if (error.code === "ECONNABORTED") {
      return "Server vaxtında cavab vermədi. Bir azdan yenidən yoxlayın.";
    }

    if (error.code === "ERR_NETWORK") {
      // Empty when the site proxies /api through its own origin, and naming an
      // empty string helps nobody.
      return baseURL
        ? `Serverə qoşulmaq mümkün olmadı (${baseURL}). API işləyirmi?`
        : "Serverə qoşulmaq mümkün olmadı. Bir azdan yenidən cəhd edin.";
    }

    if (data?.title) return translate(data.title);
  }

  if (error instanceof Error) return translate(error.message);

  return "Naməlum xəta baş verdi.";
}

/**
 * Absolute URLs are built by the API from its own request host, so an image
 * saved while the API answered on localhost keeps that host. Rewriting them
 * onto the configured base keeps pictures working when the origin changes.
 */
export function assetUrl(url: string | null | undefined): string | undefined {
  if (!url) return undefined;

  try {
    const parsed = new URL(url);
    return `${baseURL}${parsed.pathname}`;
  } catch {
    return url.startsWith("/") ? `${baseURL}${url}` : url;
  }
}
