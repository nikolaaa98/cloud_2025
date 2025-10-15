import { useState } from "react";
import { login } from "../api";

export default function Login({ onLogin }) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleLogin = async () => {
    if (!username || !password) {
      setError("Please enter both username and password");
      return;
    }

    setLoading(true);
    setError("");

    try {
      console.log("Attempting login with:", { username, password });
      const data = await login(username, password);
      console.log("Login response:", data);
      
      if (data.token) {
        console.log("Login successful, calling onLogin with token");
        onLogin(data.token);
      } else {
        setError("Login failed - no token received");
        console.error("No token in response:", data);
      }
    } catch (err) {
      setError(err.message || "Login failed");
      console.error("Login error:", err);
    } finally {
      setLoading(false);
    }
  };

  const handleKeyPress = (e) => {
    if (e.key === 'Enter') {
      handleLogin();
    }
  };

  // Testiraj direktno login bez API poziva
  const handleTestLogin = () => {
    console.log("Test login clicked");
    const testToken = "test-token-" + Date.now();
    onLogin(testToken);
  };

  return (
    <div className="login-form">
      <div className="input-group">
        <input 
          placeholder="Username" 
          value={username} 
          onChange={e => setUsername(e.target.value)}
          onKeyPress={handleKeyPress}
          disabled={loading}
        />
      </div>
      
      <div className="input-group">
        <input 
          type="password" 
          placeholder="Password" 
          value={password} 
          onChange={e => setPassword(e.target.value)}
          onKeyPress={handleKeyPress}
          disabled={loading}
        />
      </div>

      {error && <div className="error-message">{error}</div>}

      <button 
        onClick={handleLogin} 
        disabled={loading}
        className="login-button"
      >
        {loading ? "Logging in..." : "Login"}
      </button>

      {/* TEST BUTTON */}
      <button 
        onClick={handleTestLogin}
        style={{ 
          width: "100%", 
          padding: "12px", 
          backgroundColor: "#e67e22", 
          color: "white", 
          border: "none", 
          borderRadius: "5px", 
          fontSize: "16px", 
          cursor: "pointer",
          marginTop: "10px"
        }}
      >
        TEST LOGIN (Skip API)
      </button>

      <div className="demo-credentials">
        <p><strong>Debug Info:</strong></p>
        <p>Username: {username}</p>
        <p>Password: {password ? "***" : "empty"}</p>
        <p>Loading: {loading ? "Yes" : "No"}</p>
      </div>
    </div>
  );
}