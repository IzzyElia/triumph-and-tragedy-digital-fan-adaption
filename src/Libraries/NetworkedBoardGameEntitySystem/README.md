# Izzys Networked Board Game Entity System

A game entity system designed for developing online multiplayer digital board games

## Design

Each client maintains their own unique GameState object representing how they see the world.
The server maintains a master GameState that each client is synced from, however because of hidden
information etc the client's GameState may not look the same as the servers

Each game entity tracks its full history, from creation to where it is now.
This data can be used to reconstruct the game state at any point in its history