import subprocess
import os
import sys
from typing import Optional

class LocalChatModel:
    def __init__(self, model_name: str = "llama3.2"):
        self.model_name = model_name
        print(f"🔧 [LLM INIT] Model: {model_name}")

    def generate(self, prompt: str, timeout: int = 120) -> str:  # 🚀 POVEĆAJ NA 120 SEKUNDI
        """
        Poziva lokalni Ollama model.
        """
        try:
            cmd = ["ollama", "run", self.model_name]
            
            print(f"🔧 [LLM DEBUG] Command: {' '.join(cmd)}")
            print(f"🔧 [LLM DEBUG] Prompt: '{prompt}'")
            print(f"🔧 [LLM DEBUG] Timeout: {timeout}s")
            
            # Pokreni model
            proc = subprocess.run(
                cmd,
                input=prompt.encode("utf-8"),
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                timeout=timeout
            )
            
            out = proc.stdout.decode("utf-8", errors="ignore").strip()
            err = proc.stderr.decode("utf-8", errors="ignore").strip()
            
            print(f"🔧 [LLM DEBUG] Return code: {proc.returncode}")
            print(f"🔧 [LLM DEBUG] Stdout: '{out}'")
            
            if err:
                print(f"🔧 [LLM DEBUG] Stderr: '{err}'")
            
            if proc.returncode == 0 and out:
                return out
            else:
                return f"⚠️ Model error. Return code: {proc.returncode}, Stderr: {err}"
                
        except subprocess.TimeoutExpired:
            return "⚠️ Model timeout (predugo čekao). Pokušaj sa kraćim pitanjem."
        except Exception as e:
            return f"❌ Exception: {str(e)}"

def test_model():
    print("🧪 Testing model...")
    model = LocalChatModel("llama3.2")
    response = model.generate("Reci mi samo 'TEST USPESAN'")
    print(f"🧪 Test response: '{response}'")

if __name__ == "__main__":
    test_model()