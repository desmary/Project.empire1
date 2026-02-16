const API_BASE = import.meta.env.VITE_API_BASE ?? "https://localhost:7259";

function getToken() {
    return localStorage.getItem("token");
}

async function request(path, options = {}) {
    const headers = new Headers(options.headers || {});
    headers.set("Content-Type", "application/json");

    const token = getToken();
    if (token) {
        headers.set("Authorization", `Bearer ${token}`);
    }

    const res = await fetch(`${API_BASE}${path}`, {
        ...options,
        headers,
    });

    const text = await res.text();
    let data = null;
    try {
        data = text ? JSON.parse(text) : null;
    } catch {
        data = text;
    }

    if (!res.ok) {
        const msg =
            (data && (data.message || data.error || data.title)) ||
            `HTTP ${res.status}`;
        const err = new Error(msg);
        err.status = res.status;
        err.data = data;
        throw err;
    }

    return data;
}

export const http = {
    get: (path) => request(path, { method: "GET" }),
    post: (path, body) =>
        request(path, { method: "POST", body: JSON.stringify(body ?? {}) }),
};
