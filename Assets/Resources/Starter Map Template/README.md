## Introduction
Welcome to map creation! This README will guide you through the process of setting up and modifying your custom map. 
Please follow the instructions carefully to ensure everything is properly configured.

## Getting Started
1. To rename your map, simply rename the map folder to your desired map name.
2. You can delete the map by deleting the map folder.
3. After configuring the map as necessary, you can enter the Map Creator via the Main Menu. From any preset, go to Edit Preset > Map Select > Edit Map.

## Folder Structure
Inside your map folder, you will find the following:
1. Textures/
2. Models/
3. Atlas/ (DO NOT MODIFY)
4. World/ (DO NOT MODIFY)
5. trials.csv (Manually modifying this file is not recommended)

## Textures
- Place image files (PNG, JPEG) directly in the "Textures" folder.
- In the Map Creator, go to Menu > Map Settings > Stitch Atlas to add new textures.
- These textures can be used to create blocks via Menu > Blocks.
- Note: Adding textures may change existing block indices. It's recommended to have all textures ready from the start.

## Models
- Add folders for each 3D model in the "Models" folder.
- Each model folder should contain:
  - A textures/ folder
  - A .bin file
  - A .gltf file (the only supported format)
  - Optional (but highly recommended): license.txt for attribution, if, for instance, using a Creative Commons licensed model from a site like Sketchfab
- Example structure: Models/Chair/
  - textures/
  - scene.bin
  - scene.gltf
  - license.txt
- Adjust models in the Map Creator via Menu > Objects.
- Models can be deleted by simply deleting the model folder, but make sure to refresh all trials after doing so, as otherwise the CSV save data will break and prevent you from saving.

## Atlas
- The "Atlas" folder contains critical texture data. You may look at it, but do not modify its contents.
- Textures are numbered 0-255. Index = Row * Column - 1. For instance: A1 = texture 0, A8 = texture 7, H16 = texture 255.
- A 16x16 UV Map is used for the texture atlas, though an 8x8 UV Map is provided as a smaller scale visual example.

## World
- The "World" folder contains critical save data. Do not modify or access its contents. In other words, don't touch it, don't even look at it.
- All world creation and modification should be done within the Map Creator.

## Trials
- Trial information is stored in the trials.csv file.
- It's recommended to modify trials using the Map Creator (Menu > Trials) rather than editing the CSV directly.

## The Map Creator
The Map Creator is your primary tool for modifying nearly every aspect of the world, including blocks, textures, trials, and models.

### Modes of Operation
1. **Bird's Eye View Mode**
   - Navigate using WASD and by panning the mouse to the edges of the screen. Hold [L Shift] for increased camera speed.
   - Rotate camera with [Q] and [E]. Change perspectives with [R]. Zoom with [F].
   - Access six sub-menus by pressing [L Alt].
   - Switch to First Person Mode by pressing [B].

2. **First Person View Mode**
   - Use standard WASD and mouse look controls.
   - Destroy blocks with left mouse button, place blocks with right mouse button.
   - Jump and fly using Spacebar. Increase speed with Shift.
   - Switch back to Bird's Eye View Mode using [B].

### Sub-Menus (accessed in Bird's Eye View Mode)
1. Map Settings - Meta data, like the world spawn point
2. Trials - Trial data, as specified above
3. Objects - Object/Model data, as specified above
4. Blocks - Block data, as specified above
5. Learning Stage - Arrow/Barrier data, which allows the circuit-style Learning Stage to to be 
6. Retracing Stage

## Drag and Drop
- Objects like Models, Arrows, Barriers, Deadzone Colliders, and Deadzone Barriers can be dragged and dropped around the map.
- When in Bird's Eye View, click an object with the left mouse button and continue holding down the button to move it, and release to let go of it.
- When in First Person View, press [E] on an object to toggle drag and drop and snap the object to your crosshair, pressing [E] again to release it.
- The scroll wheel has a number of additional functions available:
   - Scroll normally to move the selected object up and down vertically
   - Hold [Ctrl] to instead rotate the object
   - Hold [Spacebar] to instead scale the object
   - Hold [Shift] while performing any of the above actions to increase the speed of the operation

## Best Practices
1. Plan your textures and models before starting map creation to avoid index changes.
2. Regularly save your work in the Map Creator.
3. Test your map thoroughly after making significant changes.
4. Keep backups of your map folder before making major modifications.

For any additional support or questions, please refer to either me or the Hegarty Spatial Thinking Lab.
