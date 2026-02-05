# Dynamic-Tank-Track-Renderer
A project that allows for the dynamic creation and manipulation of tank tracks in Godot.<br>
*TODO*
<img src="/Image-Examples/Main-Example.gif"/><br/>
## Features
*TODO*
<img src="/Image-Examples/Suspension-Example.gif"/><br/>
## Configuration
*TODO*
<img src="/Image-Examples/Track-Creation-Example.gif"/><br/>
## Further Developments
At the moment I am working on other projects so this project is paused, however I plan to return to this project later. However, I have the following planned:
<ul>
  <li>Reworking the code to support multimeshes for better performance</li>
  <li>Optimisation of the renderer. At the moment the script is always running, even if the tracks that are being rendered aren't visible</li>
  <li>Rewrite of the ground conformation code. The current implementation was written out of a concern for the performance efficiency of 30 raycasts per track. However, I've come to the conclusion this optimisation isn't worth the effect on the visual element of the track</li>
  <li>Bundling of the code project as an asset for godot so that this code can be added to existing projects</li>
</ul> <br/>

## Credits
The following resources were helpful in the creation of this project:<br>
https://www.dlab.ninja/2025/06/how-to-implement-gizmos-and-handles-in.html<br>
