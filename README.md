# planetoid_generation
Generating planetoid terrain in unity using a variety of noises.  This repo contains the 
planetoid generation scripts and dependences for use in a Unity project. Project for CIS 536 
at Kansas State University. 

![Image of Planetoid](https://github.com/telarson/planetoid_generation/blob/main/perlin-sub6-min0-max0.1-rough2-colorEarth.gif)

The above planetoid was generated using perlin noise on an icosphere subdivided six times.

# Overview
This project consists of a Unity implementation of noise derived spherical terrain for planetoids.
Plantoids are generated using an icosahedron subdivided into an icosphere and then its vertices
scaled based on samples from one of three noise functions. Perlin noise, Simplex noise, and
Fractional Brownian Motion (FBM) were used as the noise methods for creating the terrain
heightmap.

A total of eight different parameters are made available in the script’s inspector window to
control various attributes of the generated mesh. These include subdivisions, Max_Height,
Min_height, Redistribution, Roughness_Passes, Height Generation Method, Coloring, and
Offset Vector. The use of these parameters is discussed further in the methodology section.
Animated gifs of the generated mesh being rotated were to compare the various parameters
and noise methods. A folder containing all of these recordings will be included with this writeup.

# Methodology
The process of generating the planetoids was divided into three stages, defining an icosphere,
generating terrain, and coloring the terrain. The icosphere that acts as the base for the final
terrain is built out of an icosahedron that is then subdivided into an icosphere. To begin the
vertices and polygons that make up a regular icosahedron are added to lists that will eventually
contain the final set of polygons and vertices used to build the mesh. Next the polygons
contained in the list are subdivided up to n times as defined by the Subdivisions parameter. With
each subdivision a face of the icosahedron will be split into four smaller faces by adding vertices
to the midpoint of each edge and connecting them to form the new faces. After subdivision is
complete the lists of vertices and polygons now contain the icosphere which will act as the basis
of the terrain.

To generate the terrain on the icosphere each vertex will be scaled based on a value sampled
from either Perlin noise, Simplex noise, or Fractional Brownian Motion. The list of vertices is
iterated through and each is passed into the chosen noise function along with the Offset Vector
defined by the user. The offset vector and the vertex are added together to allow for sampling
different ranges of the noise functions as the offset vector is changed.
The amount that a vertex is scaled by is determined from three samples of the noise function at
different octaves which are added together to form a height. This height is then raised to the
power defined by the Redistribution parameter and restricted by the Max_Height and
Min_Height parameters. Finally the initial vertex from the icosphere is scaled by “1 + height” and
returned to replace the original in the list.

The above process is repeated for each roughness pass but rather than the scaled vertex
replacing the original it is scaled down by 90% and added to the original. This results in finer
grain detail being added to the original terrain with the surface becoming rougher with each
pass.

In the final step before the mesh is rendered each vertex on the terrain has a color assigned to it
using a Unity Gradient that is set by the user as the coloring parameter. To decide the color of a
vertex the distance between the final vertex and it’s pre-scaled position is divided by the
Max_Height to be used in the Gradient.Evaluate() function. With this method the terrain is
colored based on height to allow for visual distinction of the various elevations on the planetoid.

# Evaluation Criteria
Using the methods described above to produce a series of animated gifs the terrain was
evaluated based on how visually interesting it was and how well it approximated the terrain of a
planet from a solar system level viewpoint. The terrain generation was successful in producing
the desired effect with varied and interesting features on each planetoid. The methods used for
coloring were less successful as they failed to work entirely on terrain generated using simplex
noise and the others were not as detailed in their gradients as desired. This lack of detail in
color likely stems from the differences in height being too small to be a good metric for deciding
color at higher elevations.
Further work can be done to fine tune the terrain generation including adding additional
parameters and allowing for the use of multiple noise methods in concert. For example the small
peaks made when FBM is used could be used to add texture to some of the smoother regions
produced by Perlin and Simplex noise. Colorings could also be improved by finding a better
method of evaluation and incorporating colorings based on metrics other than height.

# Background & Related Work
Flick, J. (2018). Noise derivatives, a Unity C# TUTORIAL. Retrieved May 13, 2021, from
https://catlikecoding.com/unity/tutorials/noise-derivatives/

Lague, S. (2019, April 8). Procedural planet generation. Retrieved May 13, 2021, from
https://youtube.com/playlist?list=PLFt_AvWsXl0cONs3T0By4puYy6GM22ko8

Patel, A. (2020, May). Making maps with noise functions. Retrieved May 13, 2021, from
https://www.redblobgames.com/maps/terrain-from-noise/

Standen, J. (2016, September 6). Simplex Noise in C# for Unity3D. Retrieved May 13,
2021, from https://gist.github.com/jstanden/1489447

Takahashi, K. (2015, December 10). Keijiro/perlinnoise. Retrieved May 13, 2021, from
https://github.com/keijiro/PerlinNoise

Winslow, P. (2021, January 26). Creating procedural planets in unity - part 1. Retrieved
May 13, 2021, from
https://peter-winslow.medium.com/creating-procedural-planets-in-unity-part-1-df83ecb12e91
