using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTools
{
    class Log
    {
        public static bool ToFile(string message, string filepath = @"C:\Log")
        {
            try
            {
                if (!Directory.Exists(filepath)) {
                    Directory.CreateDirectory(filepath);
                }

                using (FileStream objFilestream = new FileStream(filepath, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream))
                    {
                        objStreamWriter.WriteLine($"{DateTime.Now}\t{message}\n");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
