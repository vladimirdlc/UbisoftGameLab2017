using System.Collections.Generic;

namespace Dissonance
{
    public static class RoomIdConversion
    {
#if DEBUG
        private static readonly Log Log = Logs.Create(LogCategory.Core, "Rooms");
        private static readonly Dictionary<ushort, string> RoomIdMappings = new Dictionary<ushort, string>();
#endif

        public static ushort ToRoomId(this string name)
        {
            var id = Hash16(name);

#if DEBUG
            string existing;
            if (RoomIdMappings.TryGetValue(id, out existing))
            {
                if (existing != name)
                    Log.Error("Hash collision between room names '{0}' and '{1}'. Please choose a different room name.", existing, name);
            }
            else
                RoomIdMappings[id] = name;
#endif

            return id;
        }

        private static ushort Hash16(string str)
        {
            unchecked
            {
                //dotnet string hashing is documented as not guaranteed stable between runtimes!
                //Implement our own hash to ensure stability (FNV-1a Hash http://isthe.com/chongo/tech/comp/fnv/#FNV-1a)
                var hash = 2166136261;

                if (str != null)
                {
                    for (var i = 0; i < str.Length; i++)
                    {
                        //FNV works on bytes, so split this char into 2 bytes
                        var c = str[i];
                        var b1 = (byte)(c >> 8);
                        var b2 = (byte)c;

                        hash = hash ^ b1;
                        hash = hash * 16777619;

                        hash = hash ^ b2;
                        hash = hash * 16777619;
                    }
                }

                //We now have a good 32 bit hash, but we want to mix this down into a 16 bit hash
                var upper = (ushort)(hash >> 16);
                var lower = (ushort)hash;
                return (ushort)(upper * 5791 + lower * 7639);
            }
        }
    }
}
