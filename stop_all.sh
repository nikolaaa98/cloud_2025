#!/bin/bash
echo "🛑 Zaustavljam sve servise..."

if [ -f ".pids" ]; then
  while read pid; do
    kill $pid 2>/dev/null && echo "Ubijen proces $pid"
  done < .pids
  rm .pids
  echo "✅ Svi procesi zaustavljeni."
else
  echo "⚠️  Nema PID fajla — ništa nije pokrenuto."
fi
