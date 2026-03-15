URP and Edge Detection setup
============================

1. Open this project in Unity 6. Wait for the Universal RP package to finish importing (added via Packages/manifest.json).

2. In Unity menu: Tools > Setup URP for Edge Detection

   This will:
   - Create UniversalRenderPipelineAsset.asset and UniversalRenderPipelineAsset_Renderer.asset here
   - Add the Edge Detection renderer feature to the renderer
   - Assign the URP asset in Project Settings > Graphics (and Quality)

3. Enter Play mode to see the edge detection effect. Tweak settings on the renderer asset (Edge Detection Feature) if needed.
