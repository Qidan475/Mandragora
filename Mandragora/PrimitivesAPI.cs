using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features.Toys;
using UnityEngine;

namespace Mandragora
{
    public static class PrimitivesAPI
    {
        public static PrimitivesPool Pool { get; } = new PrimitivesPool();

        public static void DrawLineCut(Primitive primitive, Vector3 from, Vector3 to, float thickness = 0.005f, Color? color = null)
        {
            var scale = new Vector3(thickness, Vector3.Distance(from, to), thickness);
            var position = from + (to - from).normalized * 0.5f;
            var rotation = (Quaternion.LookRotation(to - from) * Quaternion.Euler(90, 0, 0)).normalized;

            DrawPrimitive(primitive, PrimitiveType.Cylinder, position, scale, rotation, color);
        }

        public static void DrawLine(Primitive primitive, Vector3 from, Vector3 to, float thickness = 0.005f, Color? color = null)
        {
            var scale = new Vector3(thickness, Vector3.Distance(from, to), thickness);
            var position = from;
            var rotation = (Quaternion.LookRotation(to - from) * Quaternion.Euler(90, 0, 0)).normalized;

            DrawPrimitive(primitive, PrimitiveType.Cylinder, position, scale, rotation, color);
        }

        public static void DrawPrimitive(Primitive primitive, PrimitiveType type, Vector3 position, Vector3 scale, Quaternion? rotation = null, Color? color = null)
        {
            primitive.Type = type;
            primitive.Rotation = rotation ?? default;
            primitive.Color = color ?? Color.red;
            primitive.Scale = scale;
            primitive.Collidable = false;
            primitive.Position = position;
        }
    }
}
