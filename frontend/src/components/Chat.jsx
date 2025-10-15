import { useEffect, useState } from "react";
import { askQuestion, getChatHistory } from "../api";

export default function Chat() {
  const [question, setQuestion] = useState("");
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);

  const loadHistory = async () => {
    try {
      console.log("🔄 Loading chat history...");
      const history = await getChatHistory();
      console.log("💬 Chat history received:", history);
      
      // 🎯 OBEZBEDI DA SVE PORUKE IMAJU Answer POLJE
      const safeHistory = history.map(msg => ({
        ...msg,
        Answer: msg.Answer || msg.answer || msg.response || "No answer available"
      }));
      
      setMessages(safeHistory);
    } catch (error) {
      console.error("❌ Error loading chat history:", error);
    }
  };

  useEffect(() => { 
    console.log("🎯 Chat component mounted");
    loadHistory(); 
  }, []);

  const handleAsk = async () => {
    if (!question.trim()) return;
    
    console.log("🚀 Sending question:", question);
    setLoading(true);
    
    // Dodaj pitanje odmah u UI (optimistički update)
    const tempMessage = {
      Id: Date.now(),
      Question: question,
      Answer: "🤔 Thinking...",
      CreatedAt: new Date().toISOString(),
      Source: "AI"
    };
    
    setMessages(prev => [tempMessage, ...prev]);
    
    try {
      const response = await askQuestion(question);
      console.log("✅ AI Response received:", response);
      
      // Ažuriraj poruku sa pravim odgovorom
      setMessages(prev => 
        prev.map(msg => 
          msg.Id === tempMessage.Id 
            ? { 
                ...msg, 
                Answer: response.Answer || "No answer received",
                Source: response.Source || "AI"
              }
            : msg
        )
      );
      
      setQuestion("");
      
      // Osveži celu istoriju za svaki slučaj
      setTimeout(() => {
        loadHistory();
      }, 500);
      
    } catch (error) {
      console.error("❌ Error asking question:", error);
      
      // Ažuriraj poruku sa greškom
      setMessages(prev => 
        prev.map(msg => 
          msg.Id === tempMessage.Id 
            ? { ...msg, Answer: `❌ Error: ${error.message}` }
            : msg
        )
      );
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString) => {
    try {
      if (!dateString) return "";
      const date = new Date(dateString);
      return isNaN(date.getTime()) ? "" : date.toLocaleTimeString();
    } catch (err) {
      return "";
    }
  };

  const handleKeyPress = (e) => {
    if (e.key === 'Enter' && !loading) {
      handleAsk();
    }
  };

  // 🎯 SIGURNA PROVERA ZA Answer POLJE
  const getSafeAnswer = (message) => {
    const answer = message.Answer || message.answer || message.response || "";
    return answer || "No answer available";
  };

  // 🎯 SIGURNA PROVERA ZA STILOVE
  const getAnswerStyle = (message) => {
    const answer = getSafeAnswer(message);
    const hasError = answer.includes('❌');
    const isThinking = answer.includes('🤔');
    
    return {
      color: hasError ? '#d32f2f' : 'inherit',
      fontStyle: isThinking ? 'italic' : 'normal'
    };
  };

  console.log("📊 Current messages state:", messages);

  return (
    <div style={{ marginTop: 20 }}>
      <h3>AI Chat</h3>
      
      <div style={{ 
        border: '1px solid #ccc', 
        padding: 15, 
        height: 400, 
        overflowY: 'auto', 
        marginBottom: 15,
        borderRadius: '8px',
        background: '#fafafa'
      }}>
        {messages.length === 0 ? (
          <div style={{ textAlign: 'center', color: '#666', padding: '20px' }}>
            <p>💬 No messages yet</p>
            <p>Ask me anything!</p>
          </div>
        ) : (
          messages.map((m, index) => {
            const safeAnswer = getSafeAnswer(m);
            
            return (
              <div key={m.Id || index} style={{ 
                marginBottom: 15, 
                padding: 12, 
                background: m.Source === 'AI' ? '#e3f2fd' : '#f5f5f5',
                borderRadius: '8px',
                borderLeft: `4px solid ${m.Source === 'AI' ? '#2196f3' : '#ff9800'}`
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 5 }}>
                  <strong>🙂 You:</strong>
                  <span style={{ 
                    fontSize: '0.7em', 
                    background: m.Source === 'AI' ? '#2196f3' : '#ff9800',
                    color: 'white',
                    padding: '2px 6px',
                    borderRadius: '10px'
                  }}>
                    {m.Source || 'AI'}
                  </span>
                </div>
                <div style={{ marginBottom: 8, fontWeight: 'bold' }}>{m.Question}</div>
                
                <div style={{ marginBottom: 5 }}><strong>🤖 Assistant:</strong></div>
                <div style={getAnswerStyle(m)}>
                  {safeAnswer}
                </div>
                
                {formatDate(m.CreatedAt) && (
                  <div style={{ fontSize: '0.7em', color: '#666', marginTop: 8 }}>
                    {formatDate(m.CreatedAt)}
                  </div>
                )}
              </div>
            );
          })
        )}
      </div>
      
      <div style={{ display: 'flex', gap: '10px' }}>
        <input 
          value={question} 
          onChange={e => setQuestion(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder="Ask AI anything..." 
          style={{ 
            flex: 1, 
            padding: '10px', 
            border: '1px solid #ccc',
            borderRadius: '5px',
            fontSize: '16px'
          }}
          disabled={loading}
        />
        <button 
          onClick={handleAsk} 
          disabled={loading || !question.trim()}
          style={{
            padding: '10px 20px',
            background: loading ? '#ccc' : '#2196f3',
            color: 'white',
            border: 'none',
            borderRadius: '5px',
            cursor: loading ? 'not-allowed' : 'pointer',
            fontSize: '16px'
          }}
        >
          {loading ? "🤔 Thinking..." : "Ask AI"}
        </button>
      </div>

      {/* DEBUG INFO */}
      <div style={{ 
        marginTop: '10px', 
        padding: '10px', 
        background: '#fff3cd', 
        borderRadius: '5px',
        fontSize: '0.8em',
        border: '1px solid #ffeaa7'
      }}>
        <strong>Debug Info:</strong><br />
        Messages: {messages.length} | 
        Loading: {loading ? 'Yes' : 'No'} | 
        Question: "{question}"<br />
        Last update: {new Date().toLocaleTimeString()}
      </div>
    </div>
  );
}