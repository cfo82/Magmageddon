﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Framework
{
    public interface Property
    {
        void OnAttached(Entity entity);
        void OnDetached(Entity entity);
    }
}
