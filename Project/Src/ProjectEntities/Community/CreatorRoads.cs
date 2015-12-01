// Copyright (C) Sergey Grigorev
// Web site: http://getdev.tk
// This addon Creator Of Roads.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Engine;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;
using Engine.Utils;

namespace ProjectEntities
{
    public class CreatorRoadsType : MapObjectType
    {
        [FieldSerialize]
        private float heightStart = 0.1f;

        [DefaultValue(0.1f)]
        public float HeightStart
        {
            get { return heightStart; }
            set { heightStart = value; }
        }
    }

    public class CreatorRoads : MapObject
    {
        private bool first_init_mesh = false;
        private bool end_make = false;
        private Mesh mesh;
        private string mesh_name;
        private MapObjectAttachedMesh attached_mesh;
        private int vertex_count = 0;
        private int index_count = 0;
        private List<Vec3> original_vertex_pos = new List<Vec3>();
        private List<Vec3> original_vertex_norm = new List<Vec3>();
        private List<Vec2> original_vertex_tc = new List<Vec2>();
        private List<int> original_vertex_ind = new List<int>();
        private List<Vec3> changeable_vertex_pos = new List<Vec3>();
        private List<Vec3> changeable_vertex_norm = new List<Vec3>();

        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public Vec3 position;
            public Vec3 normal;
            public Vec2 texCoord;
        }

        private CreatorRoadsType _type = null; public new CreatorRoadsType Type { get { return _type; } }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            SubscribeToTickEvent();

            if (EngineApp.Instance.ApplicationType != EngineApp.ApplicationTypes.ResourceEditor)
            {
                attached_mesh = GetFirstAttachedObjectByAlias("road") as MapObjectAttachedMesh;
                if (attached_mesh == null)
                {
                    Log.Error("CreatorRoads: mesh by \"road\" alias not exists.");
                    return;
                }
                else
                {
                    mesh_name = this.Name;
                    attached_mesh.MeshObject.Mesh.Save("Data\\" + mesh_name + ".mesh");
                    attached_mesh.MeshName = mesh_name + ".mesh";
                    mesh = attached_mesh.MeshObject.Mesh;
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
            if (first_init_mesh == false)
                ParsingMesh();
            if (attached_mesh != null)
            {
                attached_mesh.MeshObject.Mesh.SubMeshes[0].VertexData.GetSomeGeometry(ref changeable_vertex_pos,
                    ref changeable_vertex_norm);
                if (changeable_vertex_pos.Count != 0)
                {
                    MakeRoad();
                }
            }
        }

        private void ParsingMesh()
        {
            if (mesh != null)
            {
                mesh.SubMeshes[0].VertexData.GetSomeGeometry(ref original_vertex_pos, ref original_vertex_norm,
                    ref original_vertex_tc);
                mesh.SubMeshes[0].IndexData.GetIndices(ref original_vertex_ind);
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

        private void DestroyMesh()
        {
            if (mesh != null)
            {
                mesh.Dispose();
                mesh = null;
                System.IO.File.Delete(Engine.FileSystem.VirtualFileSystem.GetRealPathByVirtual(attached_mesh.MeshName));
            }
        }

        private unsafe void MakeRoad()
        {
            Vertex[] vertices = new Vertex[vertex_count];
            ushort[] indices = new ushort[index_count];
            mesh.SubMeshes[0].VertexData.GetSomeGeometry(ref changeable_vertex_pos, ref changeable_vertex_norm);

            if (first_init_mesh == false)
            {
                for (int i = 0; i < vertex_count; i++)
                {
                    Vertex vertex = new Vertex();
                    vertex.position = original_vertex_pos[i];
                    vertex.normal = original_vertex_norm[i];
                    vertex.texCoord = original_vertex_tc[i];
                    vertices[i] = vertex;
                }

                for (int i = 0; i < index_count; i++)
                {
                    indices[i] = (ushort)(original_vertex_ind[i]);
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
                first_init_mesh = true;
            }
            else
            {
                if (end_make == false)
                {
                    for (int i = 0; i < vertex_count; i++)
                    {
                        Vertex vertex = new Vertex();
                        Vec3 p = ((original_vertex_pos[i] * attached_mesh.ScaleOffset) * Rotation) +
                            (Position + attached_mesh.PositionOffset);
                        Vec3 nvec = Vec3.Zero;
                        Vec3 nnorm = Vec3.Zero;

                        Ray ray = new Ray(p, new Vec3(changeable_vertex_norm[i].X, changeable_vertex_norm[i].Y, -(changeable_vertex_norm[i].Z * 2000.0f)));
                        if (!Single.IsNaN(ray.Direction.X) && !Single.IsNaN(ray.Origin.X))
                        {
                            RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                            ray, (int)ContactGroup.CastAll);

                            Vec3 collision_pos = Vec3.Zero;
                            Vec3 collision_nor = Vec3.Zero;
                            bool collision = false;

                            foreach (RayCastResult result in piercingResult)
                            {
                                collision = true;
                                collision_pos = result.Position;
                                collision_nor = result.Normal;
                                break;
                            }
                            if (collision)
                            {
                                nvec = new Vec3(changeable_vertex_pos[i].X, changeable_vertex_pos[i].Y, collision_pos.Z);
                                nnorm = changeable_vertex_norm[i];
                            }
                            else
                            {
                                nvec = changeable_vertex_pos[i];
                                nnorm = changeable_vertex_norm[i];
                            }
                        }
                        vertex.position = nvec;
                        vertex.normal = nnorm;
                        vertex.texCoord = original_vertex_tc[i];
                        vertices[i] = vertex;
                    }

                    for (int i = 0; i < index_count; i++)
                    {
                        indices[i] = (ushort)(original_vertex_ind[i]);
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
                        Position = new Vec3(Position.X, Position.Y, Type.HeightStart);
                    }
                }
            }
        }
    }
}