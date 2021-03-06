﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Marshal;

namespace Common.Services
{
    public class ServiceManager
    {
        // Who said me that this was painfull? :D Oh wait, It was me...
        // Nevermind, forget that, I found a way of do this using reflection
        // So this will be faster and painless
        public PyObject ServiceCall(string service, string call, PyTuple data, object client)
        {
            MethodInfo method = GetType().GetMethod(service);

            if (method == null)
            {
                throw new ServiceDoesNotExistsException(service);
            }

            Service svc = (Service)(method.Invoke(this, null));

            method = svc.GetType().GetMethod(call);

            if (method == null)
            {
                throw new ServiceDoesNotContainCallException(service, call);
            }

            return (PyObject)(method.Invoke(svc, new object[] { data, client }));
        }
    }
}
