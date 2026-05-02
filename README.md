\# Prosthetic Hand Digital Twin

<img width="722" height="144" alt="image" src="https://github.com/user-attachments/assets/7108c63a-cc72-49bc-bda6-8ac2850fa60b" />

This project is a Unity-based prosthetic hand digital twin controlled from a web interface.



\## Current Features



\- Unity prosthetic hand model

\- Finger bone control

\- Web-based manual control

\- FastAPI backend

\- Gesture buttons:

&#x20; - open hand

&#x20; - fist

&#x20; - pinch

&#x20; - thumb up

&#x20; - grasp



\## System Flow



Web page  

→ FastAPI backend  

→ Unity command receiver  

→ Prosthetic hand bone controller  

→ Finger movement



\## Run Backend



```bash

cd backend

pip install fastapi uvicorn

uvicorn main:app --reload --host 127.0.0.1 --port 8000




