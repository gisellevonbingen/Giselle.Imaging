using System;
using System.Collections.Generic;
using System.Text;

namespace Giselle.Imaging.Codec.Riff
{
    public static class RiffChunkRegistry
    {
        private static readonly Dictionary<int, RiffChunkRegstration> Registrations = new Dictionary<int, RiffChunkRegstration>();

        static RiffChunkRegistry()
        {
            Register(KnownRiffTypeKeys.Riff, () => new RiffChunkFile());
            Register(KnownRiffTypeKeys.List, () => new RiffChunkList());
        }

        public static bool TryByKey(int key, out RiffChunkRegstration registration) => Registrations.TryGetValue(key, out registration);

        public static RiffChunkRegstration ByKey(int typeKey)
        {
            if (TryByKey(typeKey, out var registration) == true)
            {
                return registration;
            }
            else
            {
                throw GetNotRegisteredKey(typeKey);
            }

        }

        public static ArgumentException GetNotRegisteredKey(int typeKey) => new ArgumentException($"Not registered key : {typeKey}({typeKey.TypeKeyToString()})");

        public static RiffChunkRegstration Register(int typeKey, Func<RiffChunk> generator)
        {
            if (Registrations.TryGetValue(typeKey, out var registration) == true)
            {
                throw new ArgumentException($"Alreay registered key : {typeKey}({typeKey.TypeKeyToString()})");
            }
            else
            {
                registration = new RiffChunkRegstration(typeKey, generator);
                Registrations[typeKey] = registration;
                return registration;
            }

        }

    }

}
