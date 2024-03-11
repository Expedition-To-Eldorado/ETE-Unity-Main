# Expedition-To-Eldorado-Engineering-Project
Engineering project for Computer Science Course at GUT in Interactive Intelligent Systems department


███████╗██╗░░░░░██████╗░░█████╗░██████╗░░█████╗░██████╗░░█████╗░
██╔════╝██║░░░░░██╔══██╗██╔══██╗██╔══██╗██╔══██╗██╔══██╗██╔══██╗
█████╗░░██║░░░░░██║░░██║██║░░██║██████╔╝███████║██║░░██║██║░░██║
██╔══╝░░██║░░░░░██║░░██║██║░░██║██╔══██╗██╔══██║██║░░██║██║░░██║
███████╗███████╗██████╔╝╚█████╔╝██║░░██║██║░░██║██████╔╝╚█████╔╝
╚══════╝╚══════╝╚═════╝░░╚════╝░╚═╝░░╚═╝╚═╝░░╚═╝╚═════╝░░╚════╝░

## Setting up project
Verstion of Unity: 2022.3.17  
Project Type: Built-in 3D  
When in Game mode you see whole program in turquise color (setted up in Edit->Preferences)  
### For working git repository:  
- Install: Git LFS (For large Unity files)
- Execute command: `git config --system core.longpaths true` (because of long file names in Unity)

## Generating Board
Parameters:
- Size: size of the board - radius of hexagonal board of hexes
- HexSize: size of one hex
- BoardPiece: Number of the board piece (A,B, .. in board game - 0, 1, .. in our implementation)
- TerrainTypes: List of existing terrain types, later used to create BoardPiece array to generate specific board piece  

Info:
1. For now 3D board looks good when 4K is turn on in Game view settings
2. Board is clickable but shown cords are sometimes wrong
3. Board prefabs are stored in /Prefabs folder, and their scriptable objects used in list are in /Scriptable Objects folder
