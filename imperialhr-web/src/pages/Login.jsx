import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export default function Login() {
    const { login } = useAuth();
    const nav = useNavigate();

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [err, setErr] = useState("");

    function submit(e) {
        e.preventDefault();
        setErr("");
        try {
            login(email, password);
            nav("/dashboard", { replace: true });
        } catch (ex) {
            setErr(ex?.message || "Помилка входу.");
        }
    }

    return (
        <div style={{ minHeight: "100vh", display: "grid", placeItems: "center", padding: 28 }}>
            <div className="bg" aria-hidden="true" />
            <div className="overlay" aria-hidden="true" />
            <div className="dust" aria-hidden="true" />
            <div className="scanlines" aria-hidden="true" />

            <div className="glass" style={{ width: "min(1020px, 100%)", padding: 22 }}>
                <div style={{ display: "flex", justifyContent: "space-between", gap: 14, alignItems: "flex-start" }}>
                    <div>
                        <div style={{ fontSize: 32, fontWeight: 980, letterSpacing: ".2px" }}>ImperialHR</div>
                        <div style={{ color: "rgba(255,255,255,.78)", fontSize: 13, marginTop: 6 }}>
                            Шлюз доступу • Погодження відпусток
                        </div>
                    </div>
                    <span className="badge pending" style={{ opacity: 0.92 }}>Secure</span>
                </div>

                <div style={{ height: 1, background: "rgba(255,255,255,.14)", margin: "16px 0" }} />

                <div style={{ display: "grid", gridTemplateColumns: "1.08fr .92fr", gap: 16 }}>
                    <div className="glass" style={{ padding: 16, boxShadow: "none", background: "rgba(255,255,255,.06)" }}>
                        <div style={{ fontWeight: 950, fontSize: 14, marginBottom: 10 }}>Вхід у систему</div>

                        <form onSubmit={submit} style={{ display: "grid", gap: 12 }}>
                            <div style={{ display: "grid", gap: 6 }}>
                                <div style={{ fontSize: 12, color: "rgba(255,255,255,.75)", fontWeight: 900 }}>Email</div>
                                <input
                                    className="input"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    placeholder="name@imperial.hr"
                                    autoComplete="username"
                                />
                            </div>

                            <div style={{ display: "grid", gap: 6 }}>
                                <div style={{ fontSize: 12, color: "rgba(255,255,255,.75)", fontWeight: 900 }}>Пароль</div>
                                <input
                                    className="input"
                                    type="password"
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    placeholder="••••"
                                    autoComplete="current-password"
                                />
                            </div>

                            {err ? (
                                <div style={{
                                    color: "#ffd2cc",
                                    background: "rgba(255,50,40,.12)",
                                    border: "1px solid rgba(255,80,70,.22)",
                                    padding: "10px 12px",
                                    borderRadius: 12,
                                    fontSize: 12,
                                    fontWeight: 900
                                }}>
                                    {err}
                                </div>
                            ) : null}

                            <button className="btn primary" type="submit">Увійти</button>

                            <div style={{ color: "rgba(255,255,255,.62)", fontSize: 12, lineHeight: 1.35 }}>
                                Доступ визначається роллю після входу. Дані облікових записів не показуються на екрані.
                            </div>
                        </form>
                    </div>

                    <div className="glass" style={{ padding: 16, boxShadow: "none", background: "rgba(0,0,0,.18)" }}>
                        <div style={{ fontWeight: 950, fontSize: 14, marginBottom: 10 }}>Ролі та повноваження</div>

                        <div style={{ display: "grid", gap: 10 }}>
                            {roleCard("👑 Імператор", "Палпатін", "Фінальний підпис заявок • контроль політик")}
                            {roleCard("🛡️ Лорд", "Дарт Вейдер", "Перший рівень погодження • оцінка навантаження")}
                            {roleCard("🪖 Штурмовик", "TK-421", "Подача заявок • статуси • сповіщення")}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

function roleCard(title, name, desc) {
    return (
        <div className="roleCard">
            <div style={{ display: "flex", justifyContent: "space-between", gap: 10, alignItems: "center" }}>
                <div style={{ fontWeight: 980 }}>{title}</div>
                <span className="badge pending" style={{ fontSize: 11, opacity: 0.9 }}>Access</span>
            </div>
            <div style={{ marginTop: 8, fontSize: 18, fontWeight: 980 }}>{name}</div>
            <div style={{ marginTop: 6, color: "rgba(255,255,255,.74)", fontSize: 12, lineHeight: 1.35 }}>
                {desc}
            </div>
        </div>
    );
}
