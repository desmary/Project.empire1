import React, { useEffect, useMemo, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { useNavigate } from "react-router-dom";

const REQ_KEY = "imperialhr.requests.v3";
const NOTIF_KEY = "imperialhr.notifs.v3";
const LOG_KEY = "imperialhr.audit.v1";

function safeParse(json) {
    try { return JSON.parse(json); } catch { return null; }
}
function readLS(key, fallback) {
    const raw = localStorage.getItem(key);
    if (!raw) return fallback;
    const parsed = safeParse(raw);
    return parsed ?? fallback;
}
function writeLS(key, value) {
    localStorage.setItem(key, JSON.stringify(value));
}
function isoNow() { return new Date().toISOString(); }

function fmtDate(iso) {
    try {
        const d = new Date(iso);
        const dd = String(d.getDate()).padStart(2, "0");
        const mm = String(d.getMonth() + 1).padStart(2, "0");
        const yyyy = d.getFullYear();
        const hh = String(d.getHours()).padStart(2, "0");
        const mi = String(d.getMinutes()).padStart(2, "0");
        return `${dd}.${mm}.${yyyy} ${hh}:${mi}`;
    } catch { return iso; }
}

function calcDays(from, to) {
    const a = new Date(from);
    const b = new Date(to);
    const ms = b.getTime() - a.getTime();
    const days = Math.floor(ms / (1000 * 60 * 60 * 24)) + 1;
    return Number.isFinite(days) && days > 0 ? days : 1;
}

function makeId(prefix) {
    const c = globalThis.crypto;
    if (c?.randomUUID) return `${prefix}_${c.randomUUID()}`;
    return `${prefix}_${Math.random().toString(16).slice(2)}_${Date.now()}`;
}

function statusLabel(status) {
    if (status === "APPROVED") return { text: "APPROVED", cls: "ok" };
    if (status === "REJECTED") return { text: "REJECTED", cls: "bad" };
    return { text: "PENDING", cls: "pending" };
}

export default function Dashboard() {
    const { user, logout } = useAuth();
    const nav = useNavigate();

    const roleKey = user?.roleKey;

    const [requests, setRequests] = useState(() => readLS(REQ_KEY, []));
    const [toast, setToast] = useState(null);
    const [audit, setAudit] = useState(() => readLS(LOG_KEY, []));

    // Trooper form
    const [fromDate, setFromDate] = useState(() => new Date().toISOString().slice(0, 10));
    const [toDate, setToDate] = useState(() => new Date().toISOString().slice(0, 10));
    const [type, setType] = useState("Annual leave");
    const [comment, setComment] = useState("");

    useEffect(() => writeLS(REQ_KEY, requests), [requests]);
    useEffect(() => writeLS(LOG_KEY, audit), [audit]);

    function addAudit(message) {
        const row = { id: makeId("log"), at: isoNow(), message };
        setAudit((prev) => [row, ...prev].slice(0, 80));
    }

    // Used/remaining for Trooper
    const allowed = user?.allowed ?? 30;

    const usedDays = useMemo(() => {
        const mine = requests.filter((r) => r.employeeId === "u_trooper");
        return mine.filter((r) => r.status === "APPROVED").reduce((s, r) => s + (r.days || 0), 0);
    }, [requests]);

    const remaining = Math.max(0, allowed - usedDays);

    // Toast notifications for Trooper
    useEffect(() => {
        if (!user || roleKey !== "TROOPER") return;

        const notifs = readLS(NOTIF_KEY, []);
        const mineUnread = notifs.filter((n) => n.employeeId === user.id && !n.read);
        if (mineUnread.length === 0) return;

        const last = mineUnread[mineUnread.length - 1];
        setToast(last.message);

        const updated = notifs.map((n) => (n.id === last.id ? { ...n, read: true } : n));
        writeLS(NOTIF_KEY, updated);

        const t = setTimeout(() => setToast(null), 3200);
        return () => clearTimeout(t);
    }, [user, roleKey, requests]);

    function pushNotif(employeeId, message) {
        const notifs = readLS(NOTIF_KEY, []);
        notifs.push({ id: makeId("n"), employeeId, message, at: isoNow(), read: false });
        writeLS(NOTIF_KEY, notifs);
    }

    function signOut() {
        logout();
        nav("/login", { replace: true });
    }

    const roleLabel = useMemo(() => {
        if (roleKey === "TROOPER") return "Штурмовик";
        if (roleKey === "LORD") return "Лорд";
        if (roleKey === "EMPEROR") return "Імператор";
        return user?.roleTitle ?? "Роль";
    }, [roleKey, user]);

    const myHours = useMemo(() => {
        if (roleKey === "TROOPER") return { worked: 149, plan: 165 };
        return { worked: user?.hoursWorked ?? 165, plan: 165 };
    }, [roleKey, user]);

    const visibleRequests = useMemo(() => {
        if (roleKey === "TROOPER") return requests.filter((r) => r.employeeId === "u_trooper");
        if (roleKey === "LORD") return requests.filter((r) => r.stage === "LORD" && r.status === "PENDING");
        if (roleKey === "EMPEROR") return requests.filter((r) => r.stage === "EMPEROR" && r.status === "PENDING");
        return requests;
    }, [requests, roleKey]);

    function createRequest() {
        const days = calcDays(fromDate, toDate);

        if (days > remaining) {
            setToast("Недостатньо доступних днів для цього періоду.");
            setTimeout(() => setToast(null), 2500);
            return;
        }

        const r = {
            id: makeId("r"),
            employeeId: "u_trooper",
            employeeName: "TK-421",
            type,
            from: fromDate,
            to: toDate,
            days,
            comment: comment.trim(),
            status: "PENDING",
            stage: "LORD",
            createdAt: isoNow(),
            updatedAt: isoNow(),
            hoursWorked: 149,
            hoursPlan: 165,
            allowedTotal: 30,
            usedApproved: usedDays,
        };

        setRequests((prev) => [r, ...prev]);
        setComment("");
        addAudit(`🪖 TK-421 подав заявку: ${type} • ${days} дн. • ${fromDate} → ${toDate}`);
        setToast("Заявку подано. Очікується рішення Лорда.");
        setTimeout(() => setToast(null), 2200);
    }

    function decide(requestId, decision) {
        setRequests((prev) =>
            prev.map((r) => {
                if (r.id !== requestId) return r;

                if (decision === "REJECT") {
                    const updated = { ...r, status: "REJECTED", stage: r.stage, updatedAt: isoNow() };
                    pushNotif("u_trooper", `Заявку відхилено • ${updated.type} • ${updated.days} дн.`);
                    addAudit(`${roleLabel} відхилив заявку TK-421 • ${updated.type} • ${updated.days} дн.`);
                    return updated;
                }

                // Approve flow
                if (r.stage === "LORD") {
                    const updated = { ...r, stage: "EMPEROR", status: "PENDING", updatedAt: isoNow() };
                    pushNotif("u_trooper", `Лорд погодив • очікується підпис Імператора.`);
                    addAudit(`🛡️ Лорд погодив • заявка TK-421 передана Імператору.`);
                    return updated;
                }

                if (r.stage === "EMPEROR") {
                    const updated = { ...r, status: "APPROVED", stage: "DONE", updatedAt: isoNow() };
                    pushNotif("u_trooper", `Заявку підтверджено • APPROVED • ${updated.days} дн.`);
                    addAudit(`👑 Імператор підтвердив • TK-421 • ${updated.type} • ${updated.days} дн.`);
                    return updated;
                }

                return r;
            })
        );
    }

    function canApprove(r) {
        if (roleKey === "LORD") return r.stage === "LORD" && r.status === "PENDING";
        if (roleKey === "EMPEROR") return r.stage === "EMPEROR" && r.status === "PENDING";
        return false;
    }

    function canReject(r) {
        if (roleKey === "LORD") return r.stage === "LORD" && r.status === "PENDING";
        if (roleKey === "EMPEROR") return r.stage === "EMPEROR" && r.status === "PENDING";
        return false;
    }

    const progressPercent = Math.min(100, (usedDays / Math.max(1, allowed)) * 100);

    return (
        <div style={{ minHeight: "100vh", position: "relative" }}>
            <div className="bg" aria-hidden="true" />
            <div className="overlay" aria-hidden="true" />
            <div className="dust" aria-hidden="true" />
            <div className="scanlines" aria-hidden="true" />

            {toast ? <div className="toast">{toast}</div> : null}

            {/* TOP BAR */}
            <div style={{ padding: "20px 26px 10px", display: "flex", justifyContent: "space-between", gap: 16, alignItems: "center" }}>
                <div>
                    <div style={{ fontSize: 30, fontWeight: 980, letterSpacing: ".2px" }}>ImperialHR</div>
                    <div style={{ color: "rgba(255,255,255,.78)", fontSize: 13, marginTop: 6 }}>
                        Ви: <b>{user?.name}</b> • Роль: <b>{roleLabel}</b>
                    </div>
                </div>
                <button className="btn ghost" onClick={signOut}>Вийти</button>
            </div>

            {/* KPI CARDS */}
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 14, padding: "0 26px 18px" }}>
                <div className="glass" style={{ padding: 18 }}>
                    <div style={{ fontSize: 13, fontWeight: 950, color: "rgba(255,255,255,.84)" }}>Ліміт відпустки</div>
                    <div style={{ marginTop: 10, fontSize: 34, fontWeight: 980 }}>{allowed} днів</div>
                    <div style={{ marginTop: 8, color: "rgba(255,255,255,.72)", fontSize: 12 }}>
                        Використано: <b>{usedDays}</b> • Залишилось: <b>{remaining}</b>
                    </div>
                    <div style={{
                        marginTop: 12, height: 10, borderRadius: 999,
                        background: "rgba(255,255,255,.10)", overflow: "hidden",
                        border: "1px solid rgba(255,255,255,.14)"
                    }}>
                        <div style={{
                            height: "100%",
                            width: `${progressPercent}%`,
                            background: "linear-gradient(90deg, rgba(255,70,40,.96), rgba(255,140,40,.90))",
                            borderRadius: 999
                        }} />
                    </div>
                </div>

                <div className="glass" style={{ padding: 18 }}>
                    <div style={{ fontSize: 13, fontWeight: 950, color: "rgba(255,255,255,.84)" }}>Навантаження</div>
                    <div style={{ marginTop: 10, fontSize: 34, fontWeight: 980 }}>
                        {myHours.worked} <span style={{ fontSize: 16, color: "rgba(255,255,255,.70)" }}>/ {myHours.plan}</span>
                    </div>
                    <div style={{ marginTop: 8, color: "rgba(255,255,255,.72)", fontSize: 12 }}>
                        Показник для швидкого рішення щодо погодження.
                    </div>
                </div>

                <div className="glass" style={{ padding: 18 }}>
                    <div style={{ fontSize: 13, fontWeight: 950, color: "rgba(255,255,255,.84)" }}>
                        {roleKey === "TROOPER" ? "Подання заявки" : "Черга на рішення"}
                    </div>

                    {roleKey === "TROOPER" ? (
                        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 10, marginTop: 12 }}>
                            <div style={{ display: "grid", gap: 6 }}>
                                <div style={{ fontSize: 12, color: "rgba(255,255,255,.75)", fontWeight: 900 }}>З</div>
                                <input className="input" type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
                            </div>

                            <div style={{ display: "grid", gap: 6 }}>
                                <div style={{ fontSize: 12, color: "rgba(255,255,255,.75)", fontWeight: 900 }}>По</div>
                                <input className="input" type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
                            </div>

                            <div style={{ gridColumn: "1 / -1", display: "grid", gap: 6 }}>
                                <div style={{ fontSize: 12, color: "rgba(255,255,255,.75)", fontWeight: 900 }}>Тип</div>
                                <select className="select" value={type} onChange={(e) => setType(e.target.value)}>
                                    <option>Annual leave</option>
                                    <option>Sick leave</option>
                                    <option>Unpaid leave</option>
                                    <option>Study leave</option>
                                </select>
                            </div>

                            <div style={{ gridColumn: "1 / -1", display: "grid", gap: 6 }}>
                                <div style={{ fontSize: 12, color: "rgba(255,255,255,.75)", fontWeight: 900 }}>Коментар</div>
                                <input className="input" value={comment} onChange={(e) => setComment(e.target.value)} placeholder="Коротко причина" />
                            </div>

                            <button
                                className="btn primary"
                                type="button"
                                onClick={createRequest}
                                disabled={calcDays(fromDate, toDate) > remaining}
                                style={{ gridColumn: "1 / -1" }}
                            >
                                Подати заявку
                            </button>

                            <div style={{ gridColumn: "1 / -1", color: "rgba(255,255,255,.70)", fontSize: 12 }}>
                                Днів: <b>{calcDays(fromDate, toDate)}</b> • Ланцюг: Лорд → Імператор
                            </div>
                        </div>
                    ) : (
                        <>
                            <div style={{ marginTop: 10, fontSize: 34, fontWeight: 980 }}>
                                {visibleRequests.length} <span style={{ fontSize: 14, color: "rgba(255,255,255,.70)" }}>очікують</span>
                            </div>
                            <div style={{ marginTop: 8, color: "rgba(255,255,255,.72)", fontSize: 12 }}>
                                Approve/Reject працюють стабільно, без “порожніх” кліків.
                            </div>
                        </>
                    )}
                </div>
            </div>

            {/* TABLE + AUDIT */}
            <div style={{ display: "grid", gridTemplateColumns: "1.55fr .75fr", gap: 14, padding: "0 26px 26px" }}>
                <div className="glass" style={{ padding: 16 }}>
                    <div style={{ display: "flex", justifyContent: "space-between", gap: 14, alignItems: "center", marginBottom: 10 }}>
                        <div style={{ fontSize: 14, fontWeight: 980, color: "rgba(255,255,255,.90)" }}>
                            {roleKey === "TROOPER" ? "Мої заявки" : "Заявки на рішення"}
                        </div>
                        <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
                            <span className="badge pending">PENDING</span>
                            <span className="badge ok">APPROVED</span>
                            <span className="badge bad">REJECTED</span>
                        </div>
                    </div>

                    <div style={{ overflow: "auto", borderRadius: 16, border: "1px solid rgba(255,255,255,.12)" }}>
                        <table style={{ width: "100%", borderCollapse: "collapse", minWidth: 980 }}>
                            <thead>
                                <tr>
                                    <th style={thStyle}>Створено</th>
                                    <th style={thStyle}>Період</th>
                                    <th style={thStyle}>Днів</th>
                                    <th style={thStyle}>Тип</th>
                                    <th style={thStyle}>Статус</th>
                                    {roleKey !== "TROOPER" ? <th style={thStyle}>Хто</th> : <th style={thStyle}>Оновлено</th>}
                                    {roleKey !== "TROOPER" ? <th style={thStyle}>Години</th> : <th style={thStyle}>Коментар</th>}
                                    <th style={thStyle}>Дія</th>
                                </tr>
                            </thead>

                            <tbody>
                                {visibleRequests.length === 0 ? (
                                    <tr>
                                        <td colSpan={8} style={{ padding: 18, textAlign: "center", color: "rgba(255,255,255,.65)" }}>
                                            Немає записів
                                        </td>
                                    </tr>
                                ) : (
                                    visibleRequests.map((r) => {
                                        const st = statusLabel(r.status);
                                        return (
                                            <tr key={r.id}>
                                                <td style={tdStyle}>{fmtDate(r.createdAt)}</td>
                                                <td style={tdStyle}>{r.from} → {r.to}</td>
                                                <td style={tdStyle}><b>{r.days}</b></td>
                                                <td style={tdStyle}>{r.type}</td>
                                                <td style={tdStyle}>
                                                    <span className={`badge ${st.cls}`}>{st.text}</span>
                                                </td>

                                                {roleKey !== "TROOPER" ? (
                                                    <td style={tdStyle}>{r.employeeName}</td>
                                                ) : (
                                                    <td style={tdStyle}>{fmtDate(r.updatedAt)}</td>
                                                )}

                                                {roleKey !== "TROOPER" ? (
                                                    <td style={tdStyle}><b>{r.hoursWorked}</b> / {r.hoursPlan}</td>
                                                ) : (
                                                    <td style={tdStyleMuted}>{r.comment || "—"}</td>
                                                )}

                                                <td style={tdStyle}>
                                                    {roleKey === "TROOPER" ? (
                                                        <span style={{ color: "rgba(255,255,255,.62)" }}>—</span>
                                                    ) : (
                                                        <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
                                                            <button className="btn" style={smallOk} disabled={!canApprove(r)} onClick={() => decide(r.id, "APPROVE")}>
                                                                Approve
                                                            </button>
                                                            <button className="btn" style={smallBad} disabled={!canReject(r)} onClick={() => decide(r.id, "REJECT")}>
                                                                Reject
                                                            </button>
                                                        </div>
                                                    )}
                                                </td>
                                            </tr>
                                        );
                                    })
                                )}
                            </tbody>
                        </table>
                    </div>
                </div>

                <div className="glass" style={{ padding: 16 }}>
                    <div style={{ fontSize: 14, fontWeight: 980, color: "rgba(255,255,255,.90)", marginBottom: 10 }}>
                        Activity Log
                    </div>

                    <div style={{ display: "grid", gap: 10, maxHeight: 520, overflow: "auto" }}>
                        {audit.length === 0 ? (
                            <div style={{ color: "rgba(255,255,255,.65)", fontSize: 12 }}>
                                Подій поки немає.
                            </div>
                        ) : audit.map((a) => (
                            <div key={a.id} className="roleCard" style={{ padding: 12 }}>
                                <div style={{ fontSize: 12, color: "rgba(255,255,255,.70)", fontWeight: 900 }}>
                                    {fmtDate(a.at)}
                                </div>
                                <div style={{ marginTop: 6, fontSize: 12, color: "rgba(255,255,255,.86)", fontWeight: 900, lineHeight: 1.35 }}>
                                    {a.message}
                                </div>
                            </div>
                        ))}
                    </div>

                    <div style={{ marginTop: 12, color: "rgba(255,255,255,.60)", fontSize: 12 }}>
                        Локальне збереження стану — щоб презентація виглядала як реальна робота продукту.
                    </div>
                </div>
            </div>
        </div>
    );

    function canApprove(r) {
        if (roleKey === "LORD") return r.stage === "LORD" && r.status === "PENDING";
        if (roleKey === "EMPEROR") return r.stage === "EMPEROR" && r.status === "PENDING";
        return false;
    }
    function canReject(r) {
        if (roleKey === "LORD") return r.stage === "LORD" && r.status === "PENDING";
        if (roleKey === "EMPEROR") return r.stage === "EMPEROR" && r.status === "PENDING";
        return false;
    }
    function decide(requestId, decision) {
        setRequests((prev) =>
            prev.map((r) => {
                if (r.id !== requestId) return r;

                if (decision === "REJECT") {
                    const updated = { ...r, status: "REJECTED", updatedAt: isoNow() };
                    pushNotif("u_trooper", `Заявку відхилено • ${updated.type} • ${updated.days} дн.`);
                    addAudit(`${roleLabel} відхилив заявку TK-421 • ${updated.type} • ${updated.days} дн.`);
                    return updated;
                }

                if (r.stage === "LORD") {
                    const updated = { ...r, stage: "EMPEROR", status: "PENDING", updatedAt: isoNow() };
                    pushNotif("u_trooper", `Лорд погодив • очікується підпис Імператора.`);
                    addAudit(`🛡️ Лорд погодив • заявка TK-421 передана Імператору.`);
                    return updated;
                }

                if (r.stage === "EMPEROR") {
                    const updated = { ...r, status: "APPROVED", stage: "DONE", updatedAt: isoNow() };
                    pushNotif("u_trooper", `Заявку підтверджено • APPROVED • ${updated.days} дн.`);
                    addAudit(`👑 Імператор підтвердив • TK-421 • ${updated.type} • ${updated.days} дн.`);
                    return updated;
                }

                return r;
            })
        );
    }
}

const thStyle = {
    padding: "12px 12px",
    textAlign: "left",
    borderBottom: "1px solid rgba(255,255,255,.10)",
    fontSize: 12,
    color: "rgba(255,255,255,.72)",
    fontWeight: 980,
    background: "rgba(0,0,0,.18)",
    position: "sticky",
    top: 0,
};

const tdStyle = {
    padding: "12px 12px",
    textAlign: "left",
    borderBottom: "1px solid rgba(255,255,255,.10)",
    fontSize: 13,
    color: "rgba(255,255,255,.88)",
};

const tdStyleMuted = {
    ...tdStyle,
    color: "rgba(255,255,255,.72)",
};

const smallOk = {
    padding: "8px 10px",
    borderRadius: 12,
    fontSize: 12,
    borderColor: "rgba(80,255,170,.22)",
    background: "rgba(255,255,255,.08)",
};

const smallBad = {
    padding: "8px 10px",
    borderRadius: 12,
    fontSize: 12,
    borderColor: "rgba(255,90,90,.22)",
    background: "rgba(255,255,255,.08)",
};


