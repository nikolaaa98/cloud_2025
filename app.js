const gatewayUrl = "http://localhost:8949"; // GatewayService endpoint

async function uploadFile() {
    const fileInput = document.getElementById("fileInput");
    const file = fileInput.files[0];
    if (!file) return alert("Choose a file first");

    const formData = new FormData();
    formData.append("file", file);

    const res = await fetch(`${gatewayUrl}/document/upload`, {
        method: "POST",
        body: formData
    });

    const data = await res.json();
    document.getElementById("uploadResult").innerText = data.Message;
}

async function askChat() {
    const question = document.getElementById("questionInput").value;
    if (!question) return alert("Type a question");

    const res = await fetch(`${gatewayUrl}/chat/ask`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(question)
    });

    const answer = await res.text();
    document.getElementById("chatResult").innerText = answer;
}

async function getHistory() {
    const res = await fetch(`${gatewayUrl}/chat/history`);
    const history = await res.json();

    const list = document.getElementById("historyList");
    list.innerHTML = "";
    history.forEach(item => {
        const li = document.createElement("li");
        li.innerText = `${item.CreatedAt}: Q: ${item.Question} | A: ${item.Answer}`;
        list.appendChild(li);
    });
}
