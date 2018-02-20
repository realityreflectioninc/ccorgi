using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface ICorgiLayer
{
    void Start(Corgi corgi);
    void Load(string url, int version, ResolveAction resolve, FallbackAction fallback);

}

