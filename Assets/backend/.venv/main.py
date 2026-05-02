from fastapi import FastAPI
from fastapi.responses import HTMLResponse
from pydantic import BaseModel, Field
from fastapi.middleware.cors import CORSMiddleware
from typing import Literal

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class Fingers(BaseModel):
    thumb: float = Field(0, ge=0, le=90)
    index: float = Field(0, ge=0, le=90)
    middle: float = Field(0, ge=0, le=90)
    ring: float = Field(0, ge=0, le=90)
    little: float = Field(0, ge=0, le=90)

class HandCommand(BaseModel):
    gesture: str = "custom"
    bendLevel: str = "custom"
    fingers: Fingers

latest_command = {
    "id": 0,
    "gesture": "open",
    "bendLevel": "open",
    "fingers": {
        "thumb": 0,
        "index": 0,
        "middle": 0,
        "ring": 0,
        "little": 0
    }
}

@app.get("/", response_class=HTMLResponse)
def home():
    return """
<!DOCTYPE html>
<html>
<head>
    <title>Prosthetic Hand Controller</title>
    <style>
        body {
            font-family: Arial;
            background: #f4f4f4;
            padding: 30px;
        }

        .container {
            background: white;
            padding: 25px;
            max-width: 700px;
            margin: auto;
            border-radius: 12px;
            box-shadow: 0 0 10px #cccccc;
        }

        h1 {
            text-align: center;
        }

        .finger {
            margin-bottom: 20px;
        }

        label {
            font-weight: bold;
            display: block;
            margin-bottom: 5px;
        }

        input[type=range] {
            width: 100%;
        }

        .value {
            font-weight: bold;
        }

        button {
            padding: 12px 18px;
            margin: 5px;
            border: none;
            border-radius: 8px;
            background: #1f6feb;
            color: white;
            cursor: pointer;
            font-size: 15px;
        }

        button:hover {
            background: #0d419d;
        }

        pre {
            background: #222;
            color: #0f0;
            padding: 15px;
            border-radius: 8px;
            overflow-x: auto;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>Prosthetic Hand Web Controller</h1>

        <div class="finger">
            <label>Thumb: <span id="thumbValue">0</span> degrees</label>
            <input id="thumb" type="range" min="0" max="90" value="0">
        </div>

        <div class="finger">
            <label>Index: <span id="indexValue">0</span> degrees</label>
            <input id="index" type="range" min="0" max="90" value="0">
        </div>

        <div class="finger">
            <label>Middle: <span id="middleValue">0</span> degrees</label>
            <input id="middle" type="range" min="0" max="90" value="0">
        </div>

        <div class="finger">
            <label>Ring: <span id="ringValue">0</span> degrees</label>
            <input id="ring" type="range" min="0" max="90" value="0">
        </div>

        <div class="finger">
            <label>Little: <span id="littleValue">0</span> degrees</label>
            <input id="little" type="range" min="0" max="90" value="0">
        </div>

        <hr>

        <button onclick="openHand()">Open Hand</button>
        <button onclick="fist()">Fist</button>
        <button onclick="pinch()">Pinch</button>
        <button onclick="thumbUp()">Thumb Up</button>
        <button onclick="indexBend()">Index Bend</button>
        <button onclick="grasp()">Grasp</button>
        <button onclick="sendCustom()">Send Custom Angles</button>

        <h3>JSON sent to backend:</h3>
        <pre id="jsonOutput"></pre>
    </div>

    <script>
        const fingers = ["thumb", "index", "middle", "ring", "little"];

        fingers.forEach(finger => {
            const slider = document.getElementById(finger);
            const valueText = document.getElementById(finger + "Value");

            slider.addEventListener("input", () => {
                valueText.innerText = slider.value;
            });
        });

        function getSliderAngles() {
            return {
                thumb: Number(document.getElementById("thumb").value),
                index: Number(document.getElementById("index").value),
                middle: Number(document.getElementById("middle").value),
                ring: Number(document.getElementById("ring").value),
                little: Number(document.getElementById("little").value)
            };
        }

        function setSliders(thumb, index, middle, ring, little) {
            document.getElementById("thumb").value = thumb;
            document.getElementById("index").value = index;
            document.getElementById("middle").value = middle;
            document.getElementById("ring").value = ring;
            document.getElementById("little").value = little;

            document.getElementById("thumbValue").innerText = thumb;
            document.getElementById("indexValue").innerText = index;
            document.getElementById("middleValue").innerText = middle;
            document.getElementById("ringValue").innerText = ring;
            document.getElementById("littleValue").innerText = little;
        }

        async function sendCommand(gesture, bendLevel, fingers) {
            const command = {
                gesture: gesture,
                bendLevel: bendLevel,
                fingers: fingers
            };

            document.getElementById("jsonOutput").innerText =
                JSON.stringify(command, null, 2);

            const response = await fetch("/command", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(command)
            });

            const result = await response.json();
            console.log(result);
        }

        function sendCustom() {
            sendCommand("custom", "custom", getSliderAngles());
        }

        function openHand() {
            setSliders(0, 0, 0, 0, 0);
            sendCommand("open", "open", getSliderAngles());
        }

        function fist() {
            setSliders(70, 90, 90, 90, 90);
            sendCommand("fist", "full", getSliderAngles());
        }

        function pinch() {
            setSliders(70, 75, 0, 0, 0);
            sendCommand("pinch", "medium", getSliderAngles());
        }

        function thumbUp() {
            setSliders(0, 90, 90, 90, 90);
            sendCommand("thumb_up", "full", getSliderAngles());
        }

        function indexBend() {
            setSliders(0, 90, 0, 0, 0);
            sendCommand("index_bend", "full", getSliderAngles());
        }

        function grasp() {
            setSliders(60, 80, 80, 80, 80);
            sendCommand("grasp", "medium", getSliderAngles());
        }
    </script>
</body>
</html>
"""

@app.post("/command")
def receive_command(command: HandCommand):
    global latest_command

    latest_command["id"] += 1
    latest_command["gesture"] = command.gesture
    latest_command["bendLevel"] = command.bendLevel
    latest_command["fingers"] = command.fingers.model_dump()

    return {
        "status": "ok",
        "message": "Command received",
        "latest_command": latest_command
    }

@app.get("/latest-command")
def get_latest_command():
    return latest_command

@app.get("/health")
def health():
    return {"status": "backend running"}