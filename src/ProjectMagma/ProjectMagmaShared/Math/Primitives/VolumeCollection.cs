using System;
using System.Collections.Generic;

namespace ProjectMagma.Shared.Math.Primitives
{
    public class VolumeCollection
    {
        public VolumeCollection()
        {
            this.volumes = new Dictionary<VolumeType, Volume>();
        }

        public void AddVolume(Volume v)
        {
            if (volumes.ContainsKey(v.Type))
            {
                throw new ArgumentException(string.Format("collection already contains a volume of type {0}!", v.Type.ToString()));
            }

            volumes.Add(v.Type, v);
        }

        public void RemoveVolume(VolumeType type)
        {
            if (volumes.ContainsKey(type))
            {
                throw new ArgumentException(string.Format("collection does not contain a volume of type {0}!", type.ToString()));
            }

            volumes.Remove(type);
        }

        public Volume GetVolume(VolumeType type)
        {
            return volumes[type];
        }

        public bool ContainsVolume(VolumeType type)
        {
            return volumes.ContainsKey(type);
        }

        public Dictionary<VolumeType, Volume> Volumes
        {
            get { return volumes; }
            set { volumes = value; }
        }

        private Dictionary<VolumeType, Volume> volumes;
    }
}
