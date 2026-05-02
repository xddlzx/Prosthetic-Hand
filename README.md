# Prosthetic Hand Digital Twin

A Unity-based prosthetic hand digital twin controlled from a web interface.

This project is part of an undergraduate graduation project focused on prosthetic hand finger movement control. The current version allows manual control of a rigged 3D prosthetic hand model through a browser-based interface. The future goal is to connect the same control system to an sEMG/NinaPro DB1-based AI model and eventually to servo motors on a physical prosthetic hand.

---

## Demo Screenshots

### Web Controller

<img width="722" height="144" alt="Web controller interface" src="https://github.com/user-attachments/assets/7108c63a-cc72-49bc-bda6-8ac2850fa60b" />

### Fist Command

<img width="1692" height="931" alt="Fist command in Unity" src="https://github.com/user-attachments/assets/c0c2deca-ad7e-4e1b-8f20-a3ad9d937a52" />

### Grasp Command

<img width="1650" height="936" alt="Grasp command in Unity" src="https://github.com/user-attachments/assets/5906b0f4-f0b6-4272-ba93-4a1b0ef0c7f1" />

### Custom Angle Control

<img width="1678" height="941" alt="Custom angle control in Unity" src="https://github.com/user-attachments/assets/a68d8cec-e3e4-448d-92f2-909afc000843" />

---

## Project Goal

The goal of this project is to create a clean digital twin system for a prosthetic hand.

The system currently supports manual web-based control of individual finger bending. Later, the same control pipeline can be connected to an AI model trained on sEMG data, such as NinaPro DB1.

Current control flow:

```text
Web Page
→ FastAPI Backend
→ Unity Command Receiver
→ Prosthetic Hand Bone Controller
→ Finger Movement
