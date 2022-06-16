using System;
using OpenXesNet.model;
using OpenXesNet.info;
using OpenXesNet.factory;

namespace OpenXesNet.extension.std
{
    public class XSoftwareTelemetryExtension : XExtension
    {
        public static readonly Uri EXTENSION_URI = new UriBuilder("http://www.xes-standard.org/swtelemetry.xesext").Uri;
        public static readonly String PREFIX = "swtelemetry";
        public static readonly String KEY_CPU_TOTAL_USER = "cpuTotalUser";
        public static readonly String KEY_CPU_TOTAL_KERNEL = "cpuTotalKernel";
        public static readonly String KEY_CPU_TOTAL_IDLE = "cpuTotalIdle";
        public static readonly String KEY_CPU_LOAD_USER = "cpuLoadUser";
        public static readonly String KEY_CPU_LOAD_KERNEL = "cpuLoadKernel";
        public static readonly String KEY_THREAD_TOTAL = "threadTotal";
        public static readonly String KEY_THREAD_DAEMON = "threadDaemon";
        public static readonly String KEY_MEMORY_USED = "memoryUsed";
        public static readonly String KEY_MEMORY_TOTAL = "memoryTotal";
        public static readonly String KEY_MEMORY_LOAD = "memoryLoad";
        public static IXAttributeDiscrete ATTR_CPU_TOTAL_USER;
        public static IXAttributeDiscrete ATTR_CPU_TOTAL_KERNEL;
        public static IXAttributeDiscrete ATTR_CPU_TOTAL_IDLE;
        public static IXAttributeContinuous ATTR_CPU_LOAD_USER;
        public static IXAttributeContinuous ATTR_CPU_LOAD_KERNEL;
        public static IXAttributeDiscrete ATTR_THREAD_TOTAL;
        public static IXAttributeDiscrete ATTR_THREAD_DAEMON;
        public static IXAttributeDiscrete ATTR_MEMORY_USED;
        public static IXAttributeDiscrete ATTR_MEMORY_TOTAL;
        public static IXAttributeContinuous ATTR_MEMORY_LOAD;
        static XSoftwareTelemetryExtension singleton = new XSoftwareTelemetryExtension();

        public static XSoftwareTelemetryExtension Instance
        {
            get { return singleton; }
        }


        XSoftwareTelemetryExtension() : base("Software Telemetry", "swtelemetry", EXTENSION_URI)
        {
            IXFactory factory = XFactoryRegistry.Instance.CurrentDefault;

            ATTR_CPU_TOTAL_USER = factory.CreateAttributeDiscrete(KEY_CPU_TOTAL_USER, -1L, this);
            this.eventAttributes.Add(KEY_CPU_TOTAL_USER, (XAttribute)ATTR_CPU_TOTAL_USER.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CPU_TOTAL_USER), "CPU usage - total time in user space, in milliseconds");

            ATTR_CPU_TOTAL_KERNEL = factory.CreateAttributeDiscrete(KEY_CPU_TOTAL_KERNEL, -1L, this);
            this.eventAttributes.Add(KEY_CPU_TOTAL_KERNEL, (XAttribute)ATTR_CPU_TOTAL_KERNEL.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CPU_TOTAL_KERNEL), "CPU usage - total time in kernel space, in milliseconds");

            ATTR_CPU_TOTAL_IDLE = factory.CreateAttributeDiscrete(KEY_CPU_TOTAL_IDLE, -1L, this);
            this.eventAttributes.Add(KEY_CPU_TOTAL_IDLE, (XAttribute)ATTR_CPU_TOTAL_IDLE.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CPU_TOTAL_IDLE), "CPU usage - total time spent idle, in milliseconds");

            ATTR_CPU_LOAD_USER = factory.CreateAttributeContinuous(KEY_CPU_LOAD_USER, -1L, this);
            this.eventAttributes.Add(KEY_CPU_LOAD_USER, (XAttribute)ATTR_CPU_LOAD_USER.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CPU_LOAD_USER), "CPU usage - load in user space");

            ATTR_CPU_LOAD_KERNEL = factory.CreateAttributeContinuous(KEY_CPU_LOAD_KERNEL, -1L, this);
            this.eventAttributes.Add(KEY_CPU_LOAD_KERNEL, (XAttribute)ATTR_CPU_LOAD_KERNEL.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CPU_LOAD_KERNEL), "CPU usage - load in kernel space");

            ATTR_THREAD_TOTAL = factory.CreateAttributeDiscrete(KEY_THREAD_TOTAL, -1L, this);
            this.eventAttributes.Add(KEY_THREAD_TOTAL, (XAttribute)ATTR_THREAD_TOTAL.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_THREAD_TOTAL), "Total number of threads");

            ATTR_THREAD_DAEMON = factory.CreateAttributeDiscrete(KEY_THREAD_DAEMON, -1L, this);
            this.eventAttributes.Add(KEY_THREAD_DAEMON, (XAttribute)ATTR_THREAD_DAEMON.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_THREAD_DAEMON), "Number of daemon threads");

            ATTR_MEMORY_USED = factory.CreateAttributeDiscrete(KEY_MEMORY_USED, -1L, this);
            this.eventAttributes.Add(KEY_MEMORY_USED, (XAttribute)ATTR_MEMORY_USED.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_MEMORY_USED), "Total memory used, measured in bytes");


            ATTR_MEMORY_TOTAL = factory.CreateAttributeDiscrete(KEY_MEMORY_TOTAL, -1L, this);
            this.eventAttributes.Add(KEY_MEMORY_TOTAL, (XAttribute)ATTR_MEMORY_TOTAL.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_MEMORY_TOTAL), "Total memory available, measured in bytes");

            ATTR_MEMORY_LOAD = factory.CreateAttributeContinuous(KEY_MEMORY_LOAD, -1L, this);
            this.eventAttributes.Add(KEY_MEMORY_LOAD, (XAttribute)ATTR_MEMORY_LOAD.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_MEMORY_LOAD), "Memory usage load");

        }

        public long ExtractCPUTotalUser(XEvent evt)
        {
            return Extract(evt, KEY_CPU_TOTAL_USER, -1L);
        }

        public XAttributeDiscrete AssignCPUTotalUser(XEvent evt, long cpuTotalUser)
        {
            return Assign(evt, (XAttributeDiscrete)ATTR_CPU_TOTAL_USER, cpuTotalUser);
        }

        public long ExtractCPUTotalKernel(XEvent evt)
        {
            return Extract(evt, KEY_CPU_TOTAL_KERNEL, -1L);
        }

        public XAttributeDiscrete AssignCPUTotalKernel(XEvent evt, long cpuTotalKernel)
        {
            return Assign(evt, (XAttributeDiscrete)ATTR_CPU_TOTAL_KERNEL, cpuTotalKernel);
        }

        public long ExtractCPUTotalIdle(XEvent evt)
        {
            return Extract(evt, KEY_CPU_TOTAL_IDLE, -1L);
        }

        public XAttributeDiscrete AssignCPUTotalIdle(XEvent evt, long cpuTotalIdle)
        {
            return Assign(evt, (XAttributeDiscrete)ATTR_CPU_TOTAL_IDLE, cpuTotalIdle);
        }

        public double ExtractCPULoadUser(XEvent evt)
        {
            return Extract(evt, KEY_CPU_LOAD_USER, -1.0D);
        }

        public XAttributeContinuous AssignCPULoadUser(XEvent evt, double cpuLoadUser)
        {
            return Assign(evt, (XAttributeContinuous)ATTR_CPU_LOAD_USER, cpuLoadUser);
        }

        public double ExtractCPULoadKernel(XEvent evt)
        {
            return Extract(evt, KEY_CPU_LOAD_KERNEL, -1.0D);
        }

        public XAttributeContinuous AssignCPULoadKernel(XEvent evt, double cpuLoadKernel)
        {
            return Assign(evt, (XAttributeContinuous)ATTR_CPU_LOAD_KERNEL, cpuLoadKernel);
        }

        public long ExtractThreadTotal(XEvent evt)
        {
            return Extract(evt, KEY_THREAD_TOTAL, -1L);
        }

        public XAttributeDiscrete AssignThreadTotal(XEvent evt, long threadTotal)
        {
            return Assign(evt, (XAttributeDiscrete)ATTR_THREAD_TOTAL, threadTotal);
        }

        public long ExtractThreadDaemon(XEvent evt)
        {
            return Extract(evt, KEY_THREAD_DAEMON, -1L);
        }

        public XAttributeDiscrete AssignThreadDaemon(XEvent evt, long threadDaemon)
        {
            return Assign(evt, (XAttributeDiscrete)ATTR_THREAD_DAEMON, threadDaemon);
        }

        public long ExtractMemoryUsed(XEvent evt)
        {
            return Extract(evt, KEY_MEMORY_USED, -1L);
        }

        public XAttributeDiscrete AssignMemoryUsed(XEvent evt, long memoryUsed)
        {
            return Assign(evt, (XAttributeDiscrete)ATTR_MEMORY_USED, memoryUsed);
        }

        public long ExtractMemoryTotal(XEvent evt)
        {
            return Extract(evt, KEY_MEMORY_TOTAL, -1L);
        }

        public XAttributeDiscrete AssignMemoryTotal(XEvent evt, long memoryTotal)
        {
            return Assign(evt, (XAttributeDiscrete)ATTR_MEMORY_TOTAL, memoryTotal);
        }

        public double ExtractMemoryLoad(XEvent evt)
        {
            return Extract(evt, KEY_MEMORY_LOAD, -1.0D);
        }

        public XAttributeContinuous AssignMemoryLoad(XEvent evt, double memoryLoad)
        {
            return Assign(evt, (XAttributeContinuous)ATTR_MEMORY_LOAD, memoryLoad);
        }

        long Extract(IXAttributable element, string definedAttribute, long defaultValue)
        {
            XAttribute attribute = element.GetAttributes()[definedAttribute];

            if (attribute == null)
            {
                return defaultValue;
            }
            return ((XAttributeDiscrete)attribute).Value;
        }

        XAttributeDiscrete Assign(IXAttributable element, XAttributeDiscrete definedAttribute, long value)
        {
            XAttributeDiscrete attr = (XAttributeDiscrete)definedAttribute.Clone();

            attr.Value = value;
            element.GetAttributes().Add(definedAttribute.Key, attr);
            return attr;
        }

        double Extract(IXAttributable element, string definedAttribute, double defaultValue)
        {
            XAttribute attribute = element.GetAttributes()[definedAttribute];

            if (attribute == null)
            {
                return defaultValue;
            }
            return ((XAttributeContinuous)attribute).Value;
        }

        XAttributeContinuous Assign(IXAttributable element, XAttributeContinuous definedAttribute, double value)
        {
            XAttributeContinuous attr = (XAttributeContinuous)definedAttribute.Clone();

            attr.Value = value;
            element.GetAttributes().Add(definedAttribute.Key, attr);
            return attr;
        }
    }
}
