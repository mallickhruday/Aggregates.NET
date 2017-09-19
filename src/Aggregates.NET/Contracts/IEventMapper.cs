﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aggregates.Contracts
{
    public interface IEventMapper
    {
        Type GetMappedTypeFor(Type type);
    }
}
