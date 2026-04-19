# 🧠 LearnSphere

LearnSphere is an immersive **Virtual Reality (VR) educational platform** developed using Unity and the XR Interaction Toolkit. The project is part of the **6G-BRICKS initiative**, aiming to revolutionize education through real-time, interactive, and collaborative VR experiences powered by next-generation networking technologies.

This repository contains the results and implementation artifacts from Phase 1 of the LearnSphere project.

---

## 📌 Overview

LearnSphere enables users to explore **high-fidelity 3D environments** (e.g., cultural heritage sites) in an immersive VR setting. The platform supports:

- Real-time collaboration in virtual classrooms  
- Interactive learning with photorealistic 3D models  
- Scalable delivery of large datasets via server or local builds  
- Deployment across standalone VR devices (Meta Quest series)  

---

## 🏗️ Repository Structure
LearnSphere/
│
├── LS-Unity/ # Unity VR application source code (XR Toolkit)
├── backend/ # Backend services (server-side logic, APIs, Docker configs, app source code)
├── README.md # Project documentation
├── .gitignore
└── .gitattributes


---

## 🧩 Features

- 🎓 Immersive VR educational environments  
- 🌍 Real-world terrain integration (OpenStreetMap-based)  
- 🏛️ Photorealistic 3D models (photogrammetry-based assets)  
- ⚡ Support for dynamic content streaming via server  
- 🤝 Multi-user and collaborative learning scenarios  
- 📡 Designed for edge-cloud and 6G-ready architectures  

---

## 🕶️ Supported Devices

LearnSphere is tested and runs on:

- Meta Quest 2  
- Meta Quest 3  
- Meta Quest 3S  

---

## ⚙️ Technology Stack

- Engine: Unity  
- XR Framework: XR Interaction Toolkit  
- Backend: Docker-based services (for server builds)  
- Networking Concept: Edge-Cloud / 6G-ready architecture  

---

## 📦 Builds

Builds are hosted externally due to size limitations.

Builds Link:  
https://drive.google.com/drive/folders/1aVvuIHFMe1lq64HX_6YegIByUpRmbo6w?usp=sharing
---

### 🧱 Available Builds

There are **4 builds** organized into two main folders:

#### 📁 Windows
- Server Version (Windows)
- No-Server Version (Windows)

#### 📁 Android (Quest Devices)
- Server Version (Android APK)
- No-Server Version (Android APK)

---

### 🔄 Build Types Explained

#### 🌐 Server Version
- Requires a running backend server (Docker container)
- VR application dynamically downloads 3D models and assets
- User must manually enter the **server IP address** inside the application
- Enables:
  - Smaller application size  
  - Scalable content delivery  
  - Dynamic updates  

#### 💾 No-Server Version
- Fully standalone build
- All 3D models are **pre-rendered and embedded**
- No server or internet connection required
- Best suited for:
  - Offline usage  
  - Demonstrations  
  - Simpler deployment  

---

## 🚀 Running the Project

### Option 1: Use Prebuilt Applications
1. Download builds from the provided Drive link  
2. Choose platform (Windows or Android)  
3. Run or install:
   - Windows: launch executable  
   - Android: sideload APK to Quest device  

---

### Option 2: Run from Source

#### Requirements
- Unity (recommended version matching the project)
- XR Interaction Toolkit
- Android Build Support (for Quest deployment)

#### Steps
1. Clone the repository  
2. Open the `LS-Unity` project in Unity  
3. Configure XR Plugin Management  
4. Build for:
   - Windows  
   - Android (Quest devices)  

---

## 🖥️ Backend (For Server Builds)

The backend enables:
- Dynamic model streaming  
- Data communication between client and server  

### Setup
1. Navigate to `backend` folder  
2. Run: docker-compose up --build
3. Note your server IP  
4. Enter the IP in the VR application when prompted  

---

## 🎯 Project Context

LearnSphere is part of a research effort focused on:

- Leveraging advanced networking technologies (e.g., 6G concepts)  
- Supporting low-latency immersive applications  
- Enabling edge-cloud orchestration for VR environments  
- Advancing education through immersive technologies  

---

## 📊 Key Objectives

- Enhance immersive learning experiences  
- Enable remote and collaborative education  
- Optimize VR delivery using edge computing  
- Support large-scale experimentation environments  

---

## 📜 License

Source Code is licensed under the MIT License
3D Assets, Models, and Builds are provided for research and evaluation purposes only and are not permitted for redistribution or commercial use
---

## 🤝 Contributors

LearnSphere was developed within the **6G-BRICKS** project through the collaboration of the following institutions:

- **University of Ioannina (UoI)**  
  Department of Informatics and Telecommunications  
  Knowledge & Intelligent Computing Laboratory (KIC Lab)  
  Responsibilities: VR application development, immersive environments, and system integration.

- **University of Peloponnese (UoP)**  
  Department of Electrical and Computer Engineering  
  eBusiness and User Experience Laboratory (eBusiness Lab)  
  Responsibilities: 6G infrastructure integration, networking, edge-cloud deployment, and system optimization.

## 🏗️ Infrastructure and Testbed Support

- 6G-BRICKS ISI testbed (Athens, Greece)
- 6G-ready networking infrastructure
- Edge-cloud computing capabilities
- Performance monitoring and experimentation support

## 🙏 Acknowledgment

This work has received funding from the Smart Networks and Services Joint Undertaking (SNS JU) under the European Union’s Horizon Europe research and innovation programme (Grant Agreement No. 101096954), in the context of the 6G-BRICKS project.
