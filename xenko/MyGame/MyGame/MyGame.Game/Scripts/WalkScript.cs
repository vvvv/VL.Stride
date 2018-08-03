using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xenko.Core.Mathematics;
using Xenko.Input;
using Xenko.Engine;

namespace MyGame
{
    public class WalkScript : StartupScript
    {
        // Declared public member fields and properties will show in the game studio

        public override void Start()
        {
            Entity.Get<AnimationComponent>().Play("Walk");
        }
    }
}
