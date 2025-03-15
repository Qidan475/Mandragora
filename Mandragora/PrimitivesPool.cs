using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features.Toys;
using MEC;

namespace Mandragora
{
    public class PrimitivesPool : Exiled.API.Features.Pools.IPool<Primitive>
    {
        private Queue<Primitive> _primitives = new Queue<Primitive>(128);

        public Primitive Get()
        {
            Primitive primitive;
            while (_primitives.Count > 0)
            {
                primitive = _primitives.Dequeue();
                if (AdminToy.List.Contains(primitive))
                    return primitive;
            }

            primitive = SpawnNew();
            return primitive;
        }

        public void Return(Primitive obj)
        {
            obj.Position = default;
            obj.Scale = default;
            obj.Rotation = default;
            obj.Visible = true;
            obj.Collidable = false;
            _primitives.Enqueue(obj);
        }

        public Primitive GetAndForget(float time)
        {
            var primitive = Get();
            Timing.CallDelayed(time, () =>
            {
                PrimitivesAPI.Pool.Return(primitive);
            });

            return primitive;
        }

        private Primitive SpawnNew()
        {
            var primitive = Primitive.Create(default, default, default, true);
            primitive.Collidable = false;
            return primitive;
        }
    }
}
