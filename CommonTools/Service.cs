using System;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools
{
    class Service
    {
        public static ServiceControllerStatus Restart(string serviceName, int timeoutMS = 60000)
        {
            var service = new ServiceController(serviceName);

            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMS);
                int millisec1 = Environment.TickCount;
                if (service.Status.Equals(ServiceControllerStatus.Running) || (service.Status.Equals(ServiceControllerStatus.StartPending)))
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    int millisec2 = Environment.TickCount;
                    timeout = TimeSpan.FromMilliseconds(timeoutMS - (millisec2 - millisec1));
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }
                else if (service.Status.Equals(ServiceControllerStatus.Stopped))
                {
                    int millisec2 = Environment.TickCount;
                    timeout = TimeSpan.FromMilliseconds(timeoutMS - (millisec2 - millisec1));
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }
                return service.Status;
            }
            catch (Exception ex)
            {
                return service.Status;
            }

        }
        public static ServiceControllerStatus Stop(string serviceName, int timeoutMS = 60000)
        {
            var service = new ServiceController(serviceName);

            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMS);
                if (!service.Status.Equals(ServiceControllerStatus.Stopped) || service.Status.Equals(ServiceControllerStatus.StopPending)) 
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }

                return service.Status;
            }
            catch (Exception ex)
            {
                return service.Status;
            }

        }
    }
}
