# Unofficial BLOCKADE Private Server

This is an open-source, custom-built private server for an older (circa 2019) version of the free-to-play game **BLOCKADE**. It is written from scratch in C# using the Godot Engine.

The project's primary goal is to document and reimplement the game's network protocol for educational purposes and to allow for custom game logic and modes.

> **Game Link (for context only):** [BLOCKADE on Steam](https://store.steampowered.com/app/1049800/BLOCKADE/)
>
> _Note: This server is **not compatible** with the current version of the game on Steam._

---

### ⚠️ Important Disclaimer

This project is not affiliated with, authorized, maintained, or endorsed by the original developers of BLOCKADE. It is an independent and unofficial project. All trademarks, service marks, and game names are the property of their respective owners.

This project is an independent reimplementation of the server application. While the core server logic is original work, to ensure network protocol compatibility, some data structures (primarily the large `enum` collections in `Shared.cs`) have been derived from the publicly available game client via decompilation.

No copyrighted game assets (models, textures, sounds, maps) are included in this repository. Use of this software to connect to a modified game client may violate the game's End-User License Agreement (EULA). Proceed at your own risk.

---

### ✨ Architecture & Features

This project uses a dual-server architecture to replicate the original game's behavior. The client first downloads the base map via HTTP and then connects to the main game server via TCP for real-time gameplay.

#### Map Distribution Server (HTTP)
*   A lightweight, custom HTTP server built to handle one specific task: serving map files.
*   When a client requests a map (e.g., `/1.map`), the server reads the corresponding file from the local `./maps/` directory and sends it.
*   This is a critical first step for the client before it can join the game server.

#### Game Server (TCP)
*   **Built with Godot:** Leverages the Godot Engine with .NET for scene management and game loop processing.
*   **Real-time Networking:** Handles low-level TCP connections, with a dedicated thread for each connected player.
*   **Player Session Management:** Manages up to 32 players, from initial connection and timeout handling to graceful disconnection.

---

### 🚀 Getting Started

#### Requirements
1.  **Godot Engine:** The .NET/C# compatible version of Godot (e.g., 4.x).
2.  **A compatible 2019 game client:** You must provide your own.
3.  **Map Files (`.map`):** The base world files that the server will distribute to clients. The original `.map` files are not included in this repository.
4.  **A modified client:** The game client must be patched to redirect its connections to your server's IP address and ports.

#### How to Run
1.  Clone this repository: `git clone https://github.com/penggrin12/blockade-classic-private-server`
2.  Create a `maps` directory in the project's root folder.
3.  Place your `.map` files inside the new directory (e.g., `./maps/1.map`, `./maps/2.map`, etc.). The name of the file must be the numerical ID of the map.
4.  Open the project in the Godot Engine.
5.  Run the main server scene by pressing `F5`. Both the HTTP and TCP servers will start automatically.
    *   The TCP Game Server will listen on port `7777`.
    *   The HTTP Map Server will listen on port `7778`.
6.  Launch your modified game client and connect.

---

### A Note on Acquiring and Modifying the Client

**This project does not and will not provide game client files or other game assets.**

A compatible, older version of the game client (circa 2019) is required. The only legitimate method for acquiring these files is through Steam's own distribution network, which requires that you have BLOCKADE in your Steam library.

It is widely known that Steam's platform allows users to download older versions of games they own via tools like the Steam Console or SteamCMD. You are solely responsible for understanding how to use these tools. **For more specific details, please see the project Wiki.**