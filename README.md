\# Prosthetic Hand Digital Twin

<img width="722" height="144" alt="image" src="https://github.com/user-attachments/assets/7108c63a-cc72-49bc-bda6-8ac2850fa60b" />

fist command
<img width="1692" height="931" alt="image" src="https://github.com/user-attachments/assets/c0c2deca-ad7e-4e1b-8f20-a3ad9d937a52" />

grasp command
<img width="1650" height="936" alt="image" src="https://github.com/user-attachments/assets/5906b0f4-f0b6-4272-ba93-4a1b0ef0c7f1" />

custom angle
<img width="1678" height="941" alt="image" src="https://github.com/user-attachments/assets/a68d8cec-e3e4-448d-92f2-909afc000843" />



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




