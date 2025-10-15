import { useEffect, useState } from "react";
import { getDocuments } from "../api";

export default function Documents() {
  const [docs, setDocs] = useState([]);
  const [loading, setLoading] = useState(false);

  const loadDocuments = async () => {
    try {
      console.log("🔄 Loading documents...");
      const documents = await getDocuments();
      console.log("📄 Documents received:", documents);
      setDocs(documents || []);
    } catch (err) {
      console.error("Error loading documents:", err);
      setDocs([]);
    }
  };

  useEffect(() => { 
    loadDocuments(); 
  }, []);

  // 🎯 DODAJ: Automatsko osvežavanje svakih 3 sekunde
  useEffect(() => {
    const interval = setInterval(loadDocuments, 3000);
    return () => clearInterval(interval);
  }, []);

  const formatDate = (dateString) => {
    try {
      if (!dateString) return "Recently";
      const date = new Date(dateString);
      return isNaN(date.getTime()) ? "Recently" : date.toLocaleString();
    } catch (err) {
      return "Recently";
    }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '15px' }}>
        <h2>Documents</h2>
        <button onClick={loadDocuments} style={{ padding: '5px 10px' }}>
          Refresh
        </button>
      </div>

      {docs.length === 0 ? (
        <div style={{ 
          padding: '20px', 
          textAlign: 'center', 
          background: '#f8f9fa', 
          borderRadius: '8px',
          border: '1px dashed #dee2e6'
        }}>
          <p>📭 No documents yet</p>
          <p style={{ fontSize: '0.9em', color: '#6c757d' }}>
            Upload a file to get started
          </p>
        </div>
      ) : (
        <div style={{ 
          border: '1px solid #e9ecef', 
          borderRadius: '8px',
          overflow: 'hidden'
        }}>
          {docs.map(d => (
            <div key={d.Id} style={{
              padding: '15px',
              borderBottom: '1px solid #e9ecef',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              background: '#fff'
            }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                <span style={{ fontSize: '1.2em' }}>📄</span>
                <div>
                  <div style={{ fontWeight: 'bold' }}>{d.FileName}</div>
                  <div style={{ fontSize: '0.8em', color: '#6c757d' }}>
                    {formatDate(d.UploadedAt)}
                  </div>
                </div>
              </div>
              <span style={{ 
                fontSize: '0.8em', 
                color: '#28a745',
                background: '#d4edda',
                padding: '2px 8px',
                borderRadius: '12px'
              }}>
                ✅ Saved
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}