using Business.Commons.Utils;
using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace FoneClube.DataAccess
{
    public class StatusAPIAccess
    {

        public string GetDatabaseName()
        {
            using (var ctx = new FoneClubeContext())
            {
                return ctx.Database.SqlQuery<string>("SELECT DB_NAME()").FirstOrDefault();
            }
        }

        public SystemInfo GetSystemInfo()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            var systemValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new SystemInfo
            {
                FreePhysicalMemory = (Double.Parse(mo["FreePhysicalMemory"].ToString()) / 1024) / 1024,
                TotalVisibleMemorySize = (Double.Parse(mo["TotalVisibleMemorySize"].ToString()) / 1024) / 1024
            }).FirstOrDefault();

            if (systemValues != null)
            {
                systemValues.MemoryPercentUsage = ((systemValues.TotalVisibleMemorySize - systemValues.FreePhysicalMemory) / systemValues.TotalVisibleMemorySize) * 100;
            }

            systemValues.CPUPercentUsage = getCPUCounter();

            return systemValues;
        }

        public string getCurrentCpuUsage()
        {
            return new PerformanceCounter("Processor", "% Processor Time", "_Total").NextValue() + "%";
        }

        public float getCPUCounter()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            dynamic firstValue = cpuCounter.NextValue();
            System.Threading.Thread.Sleep(1000);
            float secondValue = cpuCounter.NextValue();

            return secondValue;
        }

        public class SystemInfo
        {
            public double FreePhysicalMemory { get; set; }
            public double TotalVisibleMemorySize { get; set; }
            public double MemoryPercentUsage { get; set; }
            public double CPUPercentUsage { get; set; }
        }
    }
}
