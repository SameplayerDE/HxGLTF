# HxGLTF - GLTF Loader

**HxGLTF** is a lightweight and focused library for loading **GLTF** and **GLB** files.

---

> ⚠️ **Rendering is Your Responsibility**  
> HxGLTF’s main goal is to load GLTF/GLB files into structured, usable C# objects.  
> You are expected to take over from there and build your own renderer based on your specific needs.

> 🧪 **Using MonoGame?**  
> Check out the [HxGLTF.MonoGame](https://www.nuget.org/packages/H073.HxGLTF.MonoGame) package for a simple sample renderer.

## Purpose

HxGLTF is designed to:
- Parse GLTF / GLB files
- Provide structured data: scenes, nodes, meshes, skins, textures, animations

It is **not designed to**:
- Fully render models out of the box
- Handle all rendering or shader needs
- Abstract away rendering complexity

## Features

- Load `.gltf` and `.glb` files
- Animation, skinning, and texture info included

---

## Installation

```bash
dotnet add package H073.HxGLTF
```

## Understand the Format

To get the most out of HxGLTF, we recommend reading the [GLTF 2.0 Specification](https://github.com/KhronosGroup/glTF).

This will help you:
- Understand the structure of your models
- Build a proper rendering system
- Handle nodes, hierarchies, animations, skins, and materials correctly

---

## Contact

For feedback or help, reach out via Discord: **sameplayer**
