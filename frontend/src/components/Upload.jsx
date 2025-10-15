import { useState } from "react";
import { uploadFile } from "../api";

export default function Upload({ onUpload }) {
  const [file, setFile] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [message, setMessage] = useState("");

  const handleUpload = async () => {
    if (!file) {
      setMessage("Please select a file first");
      return;
    }

    setUploading(true);
    setMessage("");

    try {
      const res = await uploadFile(file);
      setMessage(res.Message || "File uploaded successfully!");
      setFile(null);
      // Reset file input
      document.querySelector('input[type="file"]').value = "";
      onUpload(res);
    } catch (err) {
      setMessage("Upload failed: " + err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="upload-section">
      <div style={{ marginBottom: "15px" }}>
        <input 
          type="file" 
          onChange={e => setFile(e.target.files[0])}
          disabled={uploading}
        />
      </div>
      
      <button onClick={handleUpload} disabled={uploading || !file}>
        {uploading ? "Uploading..." : "Upload Document"}
      </button>
      
      {message && (
        <div style={{ 
          marginTop: 15, 
          padding: "12px",
          borderRadius: "6px",
          backgroundColor: message.includes("failed") ? "#f8d7da" : "#d4edda",
          color: message.includes("failed") ? "#721c24" : "#155724",
          border: `1px solid ${message.includes("failed") ? "#f5c6cb" : "#c3e6cb"}`
        }}>
          <strong>Status:</strong> {message}
        </div>
      )}
      
      {/* REMOVED the "Document Service is starting up" message */}
    </div>
  );
}