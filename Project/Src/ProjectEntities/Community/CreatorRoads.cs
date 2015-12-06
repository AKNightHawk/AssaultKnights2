// Copyright (C) Sergey Grigorev
// Web site: http://getdev.tk
// This addon Creator Of Roads V2.
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.SoundSystem;
using Engine.Renderer;
using Engine.Utils;
using Engine.FileSystem;
using ProjectCommon;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectEntities
{
    public class CreatorRoadsType : MapObjectType
    {
    }
    public class CreatorRoads : MapObject
    {
        bool first_init_mesh = false;
        bool end_make = false;
        Mesh mesh = null;
        string mesh_name;
        MapObjectAttachedMesh attached_mesh;
        int vertex_count = 0;
        int index_count = 0;
        List<Vec3> vertex_pos = new List<Vec3>();
        List<Vec3> vertex_norm = new List<Vec3>();
        List<Vec2> vertex_tc = new List<Vec2>();
        List<int> vertex_ind = new List<int>();
        [StructLayout(LayoutKind.Sequential)]
        struct Vertex
        {
            public Vec3 position;
            public Vec3 normal;
            public Vec2 texCoord;
        }

        CreatorRoadsType _type = null; public new CreatorRoadsType Type { get { return _type; } }

        protected override void OnPreCreate()
        {
            base.OnPreCreate();
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            SubscribeToTickEvent();

            if (EngineApp.Instance.ApplicationType != EngineApp.ApplicationTypes.ResourceEditor)
            {
                attached_mesh = this.AttachedObjects[0] as MapObjectAttachedMesh;
                if (attached_mesh == null)
                {
                    Log.Error("CreatorRoads: Not found attached road mesh.");
                    return;
                }
                else
                {
                    mesh_name = this.Name;
                    if (attached_mesh.MeshObject.Mesh.Save("Data\\" + mesh_name + ".mesh"))
                    {
                        attached_mesh.MeshName = mesh_name + ".mesh";
                        mesh = attached_mesh.MeshObject.Mesh;
                    }
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyMesh();
        }

        protected override void OnTick()
        {
            base.OnTick();
        }

        protected override void OnRenderFrame()
        {
            base.OnRenderFrame();

            if (mesh != null)
            {
                if (first_init_mesh == false)
                {
                    ParsingMesh();
                }
                MakeRoad();
            }
        }

        void ParsingMesh()
        {
            if (mesh != null)
            {
                mesh.SubMeshes[0].VertexData.GetSomeGeometry(ref vertex_pos, ref vertex_norm,
                    ref vertex_tc);
                mesh.SubMeshes[0].IndexData.GetIndices(ref vertex_ind);
                vertex_count = mesh.SubMeshes[0].VertexData.VertexCount;
                index_count = mesh.SubMeshes[0].IndexData.IndexCount;

                SubMesh sub_mesh = mesh.SubMeshes[0];
                sub_mesh.UseSharedVertices = false;
                HardwareBuffer.Usage usage = HardwareBuffer.Usage.DynamicWriteOnly;
                HardwareVertexBuffer vertexBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
                    Marshal.SizeOf(typeof(Vertex)), vertex_count, usage);
                sub_mesh.VertexData.VertexBufferBinding.SetBinding(0, vertexBuffer, true);
                sub_mesh.VertexData.VertexCount = vertex_count;
                HardwareIndexBuffer indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(
                    HardwareIndexBuffer.IndexType._16Bit, index_count, usage);
                sub_mesh.IndexData.SetIndexBuffer(indexBuffer, true);
                sub_mesh.IndexData.IndexCount = index_count;
            }
        }

        void DestroyMesh()
        {
            if (mesh != null)
            {
                mesh.Dispose();
                mesh = null;
                System.IO.File.Delete(Engine.FileSystem.VirtualFileSystem.GetRealPathByVirtual(attached_mesh.MeshName));
            }
        }

        unsafe void MakeRoad()
        {
            if (end_make == false && HeightmapTerrain.Instances[0] != null)
            {
                Vertex[] vertices = new Vertex[vertex_count];
                ushort[] indices = new ushort[index_count];

                for (int i = 0; i < index_count; i++)
                {
                    indices[i] = (ushort)(vertex_ind[i]);
                }

                for (int i = 0; i < vertex_count; i++)
                {
                    Vertex vertex = new Vertex();
                    Vec3 p = ((vertex_pos[i] * attached_mesh.ScaleOffset) * Rotation) +
                        (Position + attached_mesh.PositionOffset);
                    Vec2 mesh_vertex_pos = new Vec2(p.X, p.Y);
                    float terrain_height = HeightmapTerrain.Instances[0].Position.Z
                        + HeightmapTerrain.Instances[0].GetHeight(mesh_vertex_pos, false);
                    Vec3 terrain_norm = HeightmapTerrain.Instances[0].GetNormal(mesh_vertex_pos);
                    vertex.position = new Vec3(vertex_pos[i].X, vertex_pos[i].Y, terrain_height);
                    vertex.normal = terrain_norm;
                    vertex.texCoord = vertex_tc[i];
                    vertices[i] = vertex;
                }

                SubMesh sub_mesh = mesh.SubMeshes[0];
                {
                    HardwareVertexBuffer vertex_buffer = sub_mesh.VertexData.VertexBufferBinding.GetBuffer(0);

                    IntPtr buffer = vertex_buffer.Lock(HardwareBuffer.LockOptions.Discard);
                    fixed (Vertex* pvertices = vertices)
                    {
                        NativeUtils.CopyMemory(buffer, (IntPtr)pvertices, vertices.Length * sizeof(Vertex));
                    }
                    vertex_buffer.Unlock();
                }
                {
                    HardwareIndexBuffer index_buffer = sub_mesh.IndexData.IndexBuffer;
                    IntPtr buffer = index_buffer.Lock(HardwareBuffer.LockOptions.Discard);
                    fixed (ushort* pindices = indices)
                    {
                        NativeUtils.CopyMemory(buffer, (IntPtr)pindices, indices.Length * sizeof(ushort));
                    }
                    index_buffer.Unlock();
                }

                if (EngineApp.Instance.ApplicationType == EngineApp.ApplicationTypes.Simulation)
                {
                    end_make = true;
                    first_init_mesh = true;
                }
            }
        }
    }
}