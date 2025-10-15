import { useState, useEffect } from "react";
import Login from "./components/Login";
import Upload from "./components/Upload";
import Documents from "./components/Documents";
import Chat from "./components/Chat";
import ServiceStatus from "./components/ServiceStatus";
import "./styles.css";

export default function App() {
  const [token, setToken] = useState(null);
  const [debugInfo, setDebugInfo] = useState("");

  useEffect(() => {
    const savedToken = localStorage.getItem('authToken');
    console.log("App loaded, savedToken:", savedToken);
    if (savedToken) {
      setToken(savedToken);
      setDebugInfo("Token found in localStorage");
    } else {
      setDebugInfo("No token in localStorage");
    }
  }, []);

  const handleLogin = (newToken) => {
    console.log("Login successful, token:", newToken);
    setToken(newToken);
    localStorage.setItem('authToken', newToken);
    setDebugInfo("Login successful, token saved");
  };

  const handleLogout = () => {
    console.log("Logging out");
    setToken(null);
    localStorage.removeItem('authToken');
    setDebugInfo("Logged out");
  };

  if (!token) {
    return (
      <div className="app-container">
        <div className="login-page">
          <h1>🔐 CloudApp Login</h1>
          <Login onLogin={handleLogin} />
        </div>
      </div>
    );
  }

  return (
    <div className="app-container">
      <div className="app-header">
        <h1>📄 Cloud Document Assistant</h1>
        <div className="user-info">
          <span>Logged in ✅</span>
          <button onClick={handleLogout} className="logout-btn">
            Logout
          </button>
        </div>
      </div>
      
      {/* Service Status */}
      <ServiceStatus />
      
      <div className="app-content">
        <section className="section">
          <h2>Upload Document</h2>
          <Upload onUpload={() => {}} />
        </section>

        <section className="section">
          <h2>Documents</h2>
          <Documents />
        </section>

        <section className="section">
          <Chat />
        </section>
      </div>
    </div>
  );
}