using SiliconStudio.Xenko.Audio;
using SiliconStudio.Xenko.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Xenko.Utils
{
    public static class AudioUtils
    {
        public static AudioEmitterSoundController GetSoundController(Entity entity, string key)
        {
            return entity.Get<AudioEmitterComponent>()[key];
        }

        public static Entity RemoveAudioEmitter(Entity entity)
        {
            var emitter = entity.Get<AudioEmitterComponent>();
            entity.Components.Remove<AudioEmitterComponent>();
            return entity;
        }
    }
}
