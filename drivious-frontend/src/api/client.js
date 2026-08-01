import axios from "axios";

const TOKEN_KEY = "drivious.accessToken";
const REFRESH_KEY = "drivious.refreshToken";

export const tokenStore = {
  get access() {
    return localStorage.getItem(TOKEN_KEY);
  },
  get refresh() {
    return localStorage.getItem(REFRESH_KEY);
  },
  save(accessToken, refreshToken) {
    localStorage.setItem(TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_KEY, refreshToken);
  },
  clear() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
  },
};

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

api.interceptors.request.use((config) => {
  const token = tokenStore.access;

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

// A refresh in flight. Without this, five requests failing at once would each
// start their own refresh, and four of them would present a token the first one
// already rotated away.
let refreshing = null;

// Called when the refresh itself fails, so the app can send the user to /login.
let onSessionLost = () => {};

export function setSessionLostHandler(handler) {
  onSessionLost = handler;
}

async function refreshAccessToken() {
  const refreshToken = tokenStore.refresh;

  if (!refreshToken) {
    throw new Error("No refresh token stored.");
  }

  // A bare axios call - the instance's interceptor would attach the expired
  // access token and retry this very request on failure.
  const { data } = await axios.post(
    `${import.meta.env.VITE_API_URL}/api/auths/refresh`,
    { refreshToken }
  );

  tokenStore.save(data.data.accessToken, data.data.refreshToken);

  return data.data.accessToken;
}

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;

    // 403 means the role is not allowed and refreshing changes nothing.
    // Only an expired token is worth retrying, and only once.
    if (error.response?.status !== 401 || original._retried) {
      return Promise.reject(error);
    }

    // The refresh endpoint answering 401 means the refresh token is gone too.
    if (original.url?.includes("/api/auths/refresh")) {
      tokenStore.clear();
      onSessionLost();
      return Promise.reject(error);
    }

    original._retried = true;

    try {
      refreshing = refreshing ?? refreshAccessToken().finally(() => {
        refreshing = null;
      });

      const token = await refreshing;

      original.headers.Authorization = `Bearer ${token}`;

      return api(original);
    } catch (refreshError) {
      tokenStore.clear();
      onSessionLost();
      return Promise.reject(refreshError);
    }
  }
);

/**
 * Turns a plain object into FormData for the three resources the API accepts as
 * multipart - vehicles, drivers and vehicle documents. Fields left undefined or
 * null are skipped, so a PATCH only carries what actually changed.
 */
export function toFormData(values) {
  const form = new FormData();

  Object.entries(values).forEach(([key, value]) => {
    if (value === undefined || value === null || value === "") return;

    form.append(key, value instanceof File ? value : String(value));
  });

  return form;
}

/**
 * The API always answers { success, message, data }. Unwraps it, and turns a
 * failure into a thrown Error carrying the message the API wrote - which is
 * already written for a person to read.
 */
export function unwrap(response) {
  const body = response.data;

  if (body?.success === false) {
    throw new Error(body.message);
  }

  return body?.data;
}

/**
 * The message to show when a call fails. FluentValidation answers with a
 * problem-details body whose `errors` object holds one array per field; a
 * business rule answers with our own `message`.
 */
export function errorMessage(error) {
  const data = error.response?.data;

  if (data?.errors) {
    return Object.values(data.errors).flat().join(" ");
  }

  return data?.message ?? error.message ?? "Something went wrong.";
}
