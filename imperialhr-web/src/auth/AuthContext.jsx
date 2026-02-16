/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useContext, useMemo, useState } from "react";

const AuthContext = createContext(null);
const STORAGE_KEY = "imperialhr.session.v2";

const USERS = [
    {
        id: "u_emperor",
        email: "emperor@imperial.hr",
        password: "1234",
        roleKey: "EMPEROR",
        roleTitle: "Імператор",
        name: "Палпатін",
        callSign: "Emperor Palpatine",
        allowed: 30,
        hoursWorked: 165,
    },
    {
        id: "u_lord",
        email: "lord@imperial.hr",
        password: "1234",
        roleKey: "LORD",
        roleTitle: "Лорд",
        name: "Дарт Вейдер",
        callSign: "Darth Vader",
        allowed: 30,
        hoursWorked: 165,
    },
    {
        id: "u_trooper",
        email: "trooper@imperial.hr",
        password: "1234",
        roleKey: "TROOPER",
        roleTitle: "Штурмовик",
        name: "TK-421",
        callSign: "Stormtrooper TK-421",
        allowed: 30,
        hoursWorked: 149,
    },
];

function safeParse(json) {
    try { return JSON.parse(json); } catch { return null; }
}
function readSession() {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    return safeParse(raw);
}

export function AuthProvider({ children }) {
    const [user, setUser] = useState(() => readSession());

    function login(email, password) {
        const e = String(email || "").trim().toLowerCase();
        const p = String(password || "");

        const found = USERS.find((u) => u.email.toLowerCase() === e && u.password === p);
        if (!found) throw new Error("Невірний email або пароль.");

        const sessionUser = {
            id: found.id,
            email: found.email,
            roleKey: found.roleKey,
            roleTitle: found.roleTitle,
            name: found.name,
            callSign: found.callSign,
            allowed: found.allowed,
            hoursWorked: found.hoursWorked,
        };

        localStorage.setItem(STORAGE_KEY, JSON.stringify(sessionUser));
        setUser(sessionUser);
    }

    function logout() {
        localStorage.removeItem(STORAGE_KEY);
        setUser(null);
    }

    const value = useMemo(() => ({ user, login, logout }), [user]);
    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error("useAuth must be used within AuthProvider");
    return ctx;
}
