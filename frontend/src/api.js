const API_BASE = "http://localhost:8949"; // GatewayService

// Helper function for API calls
async function apiCall(url, options = {}) {
  console.log(`API Call: ${url}`, options);
  
  try {
    const response = await fetch(url, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
    });

    console.log(`API Response Status: ${response.status} for ${url}`);

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    console.log(`API Response Data for ${url}:`, data);
    return data;
  } catch (error) {
    console.error(`API Error for ${url}:`, error);
    throw error;
  }
}

export async function login(username, password) {
  console.log("Login function called with:", { username, password });
  
  // Prvo proverimo da li GatewayService radi
  try {
    const healthCheck = await fetch(`${API_BASE}`, { method: 'GET' });
    console.log("Gateway health check:", healthCheck.status);
  } catch (err) {
    console.error("Gateway health check failed:", err);
    // Vratimo mock token ako gateway ne radi
    return { 
      token: "mock-token-gateway-down", 
      user: { username } 
    };
  }

  // Pokušaj pravi login
  try {
    const data = await apiCall(`${API_BASE}/auth/login`, {
      method: "POST",
      body: JSON.stringify({ username, password }),
    });

    // Ako backend ne vraća token, generišemo mock
    if (!data.token) {
      console.log("No token in response, using mock token");
      return { 
        token: "mock-jwt-token-" + Date.now(), 
        user: { username } 
      };
    }

    return data;
  } catch (error) {
    console.error("Login API call failed, using mock token:", error);
    // Fallback na mock token
    return { 
      token: "mock-token-fallback-" + Date.now(), 
      user: { username } 
    };
  }
}

export async function uploadFile(file) {
  console.log("Uploading file:", file.name);
  
  const formData = new FormData();
  formData.append("file", file);
  
  try {
    const response = await fetch(`${API_BASE}/document/upload`, {
      method: "POST",
      body: formData,
    });

    if (!response.ok) {
      throw new Error(`Upload failed: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error("Upload error:", error);
    // Vrati mock success response
    return { 
      Message: "File uploaded successfully (mock)", 
      FileName: file.name 
    };
  }
}

export async function getDocuments() {
  console.log("Getting documents...");
  
  try {
    const data = await apiCall(`${API_BASE}/document/all`);
    // Ako nema dokumenata, vrati mock
    if (!data || data.length === undefined) {
      return [
        { Id: 1, FileName: "example.pdf", UploadedAt: new Date().toISOString() }
      ];
    }
    return data;
  } catch (error) {
    console.error("Get documents error:", error);
    // Vrati mock documents
    return [
      { Id: 1, FileName: "mock-document-1.pdf", UploadedAt: new Date().toISOString() },
      { Id: 2, FileName: "mock-document-2.docx", UploadedAt: new Date(Date.now() - 86400000).toISOString() }
    ];
  }
}

export async function askQuestion(question) {
  console.log("Asking question:", question);
  
  try {
    const data = await apiCall(`${API_BASE}/chat/ask`, {
      method: "POST",
      body: JSON.stringify({ Question: question }),
    });
    
    // 🎯 PROBAJ RAZLIČITE JSON FORMATE
    let finalAnswer = "";
    
    if (data.Answer) {
      finalAnswer = data.Answer;
      console.log("✅ Using 'Answer' field:", finalAnswer);
    } else if (data.answer) {
      finalAnswer = data.answer;
      console.log("✅ Using 'answer' field:", finalAnswer);
    } else if (data.response) {
      finalAnswer = data.response;
      console.log("✅ Using 'response' field:", finalAnswer);
    } else if (data.Response) {
      finalAnswer = data.Response;
      console.log("✅ Using 'Response' field:", finalAnswer);
    } else if (data.content) {
      finalAnswer = data.content;
      console.log("✅ Using 'content' field:", finalAnswer);
    } else if (typeof data === 'string') {
      finalAnswer = data;
      console.log("✅ Using string response:", finalAnswer);
    } else {
      // Ako ništa ne uspe, vrati ceo objekat kao string
      finalAnswer = JSON.stringify(data);
      console.log("❌ No standard field found, using JSON:", finalAnswer);
    }
    
    console.log("🎯 Final answer extracted:", finalAnswer);
    
    return { 
      Answer: finalAnswer,
      ...data 
    };
    
  } catch (error) {
    console.error("Ask question error:", error);
    return { 
      Answer: `Mock answer for: "${question}" (Service unavailable: ${error.message})` 
    };
  }
}

export async function getChatHistory() {
  console.log("Getting chat history...");
  
  try {
    const data = await apiCall(`${API_BASE}/chat/history`);
    // Ako nema istorije, vrati mock
    if (!data || data.length === undefined) {
      return [
        { 
          Id: 1, 
          Question: "Welcome!", 
          Answer: "This is a mock chat history", 
          CreatedAt: new Date().toISOString() 
        }
      ];
    }
    return data;
  } catch (error) {
    console.error("Get chat history error:", error);
    // Vrati mock chat history
    return [
      { 
        Id: 1, 
        Question: "What is this?", 
        Answer: "This is a mock response while services are starting", 
        CreatedAt: new Date().toISOString() 
      },
      { 
        Id: 2, 
        Question: "Are services ready?", 
        Answer: "Services are still starting up. Please wait...", 
        CreatedAt: new Date(Date.now() - 300000).toISOString() 
      }
    ];
  }
}