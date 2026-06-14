# Fluffy Smash
<img width="460" height="215" alt="header (2)" src="https://github.com/user-attachments/assets/8f37cfac-d5a5-458c-90f8-0aa6ff004d1d" />
Fluffy Smash is a multiplayer 3D arena combat game developed in Unity. Players select from a roster of unique characters and compete in fast-paced deathmatch encounters across a variety of combat arenas.

## Features

### Character Roster

* Three playable characters.
* Distinct abilities and movesets for each character.
* Multiple playstyles supporting different combat approaches.

### Arena-Based Combat

* Two fully playable arena maps.
* Designed for close-quarters multiplayer engagements.
* Optimized for repeated competitive matches.

### Multiplayer

* Real-time online multiplayer gameplay.
* Host-client architecture powered by Mirror Networking.
* Direct IP connection support.

## Technology Stack

* **Engine:** Unity
* **Networking Framework:** Mirror

## Networking

The current implementation utilizes Mirror's standard synchronization systems.

The project does not currently include:

* Server-authoritative movement
* Client-side prediction
* Lag compensation

For optimal gameplay experience, low-latency network connections are recommended.

## Running the Project

### Hosting a Session

1. Launch the game and create a host session.
2. Configure router port forwarding for **TCP/UDP port 25565**.
3. Provide the host machine's public IP address to connecting players.

### Joining a Session

1. Launch the game client.
2. Enter the host's public IP address.
3. Connect to the session.

## Current Content

| Category            | Count          |
| ------------------- | -------------- |
| Playable Characters | 3              |
| Arena Maps          | 2              |
| Game Modes          | 1 (Deathmatch) |

## Project Status

This project is currently in active development. Gameplay systems, networking infrastructure, content, and performance are subject to ongoing iteration and improvement.
