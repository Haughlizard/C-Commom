using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LSY
{
    /// <summary>
    /// INI配置文件帮助类。
    /// </summary>
    public static class IniHelper
    {

        /// <summary>
        /// 向Ini文件中写入值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="section">小节的名称</param>
        /// <param name="key">键的名称</param>
        /// <param name="value">键的值</param>
        /// <returns>执行成功为True，失败为False。</returns>
        public static long WriteIniKey(string filePath, string section, string key, string value)
        {
            if (section.Trim().Length <= 0 || key.Trim().Length <= 0 || value.Trim().Length <= 0)
                return 0;

            return NativeMethods.WritePrivateProfileString(section, key, value, filePath);
        }

        /// <summary>
        /// 删除指定的小节（包括这个小节中所有的键）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="section">小节的名称</param>
        /// <returns>执行成功为True，失败为False。</returns>
        public static long DeleteIniSection(string filePath, string section)
        {
            if (section.Trim().Length <= 0) return 0;

            return NativeMethods.WritePrivateProfileString(section, null, null, filePath);
        }

        /// <summary>
        /// 删除指定小节中的键
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="section">小节的名称</param>
        /// <param name="key">键的名称</param>
        /// <returns>执行成功为True，失败为False。</returns>
        public static long DeleteIniKey(string filePath, string section, string key)
        {
            if (section.Trim().Length <= 0 || key.Trim().Length <= 0) return 0;

            return NativeMethods.WritePrivateProfileString(section, key, null, filePath);
        }

        /// <summary>
        /// 获取指定区下的所有键值。
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="section">区</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="capacity">容量</param>
        /// <returns>键值对字符串</returns>
        public static string GetIniSectionValue(string filePath, string section, string defaultValue = "", int capacity = 1024)
        {
            if (section.Trim().Length <= 0) return defaultValue;

            IntPtr pReturnedString = Marshal.AllocCoTaskMem(capacity * sizeof(char));
            int bytesReturned = NativeMethods.GetPrivateProfileSection(section, pReturnedString, capacity, filePath);

            if ((bytesReturned == capacity - 2) || (bytesReturned == 0))
            {
                Marshal.FreeCoTaskMem(pReturnedString);
                return defaultValue;
            }

            //bytesReturned -1 to remove trailing \0

            // NOTE: Calling Marshal.PtrToStringAuto(pReturnedString) will 
            //       result in only the first pair being returned
            string returnedString = Marshal.PtrToStringAuto(pReturnedString, bytesReturned - 1);

            //section = returnedString.Split('\0');
            Marshal.FreeCoTaskMem(pReturnedString);
            return returnedString;
        }

        /// <summary>
        /// 获得指定小节中键的值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="section">小节的名称</param>
        /// <param name="key">键的名称</param>
        /// <param name="defaultValue">如果键值为空，或没找到，返回指定的默认值。</param>
        /// <param name="capacity">缓冲区初始化大小。</param>
        /// <returns>键的值</returns>
        public static string GetIniKeyValue(string filePath, string section, string key, string defaultValue = "", int capacity = 1024)
        {
            if (section.Trim().Length <= 0 || key.Trim().Length <= 0) return defaultValue;

            var strTemp = new StringBuilder(capacity);
            NativeMethods.GetPrivateProfileString(section, key, defaultValue, strTemp, capacity, filePath);

            return strTemp.ToString().Trim();
        }

        private static class NativeMethods
        {
            /// <summary>
            /// Windows API 对INI文件写方法
            /// </summary>
            /// <param name="lpApplicationName">要在其中写入新字串的小节名称。这个字串不区分大小写</param>
            /// <param name="lpKeyName">要设置的项名或条目名。这个字串不区分大小写。用null可删除这个小节的所有设置项</param>
            /// <param name="lpString">指定为这个项写入的字串值。用null表示删除这个项现有的字串</param>
            /// <param name="lpFileName">初始化文件的名字。如果没有指定完整路径名，则windows会在windows目录查找文件。如果文件没有找到，则函数会创建它</param>
            /// <returns></returns>
            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern long WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);

            /// <summary>
            /// Windows API 对INI文件读方法
            /// </summary>
            /// <param name="lpApplicationName">欲在其中查找条目的小节名称。这个字串不区分大小写。如设为null，就在lpReturnedString缓冲区内装载这个ini文件所有小节的列表</param>
            /// <param name="lpKeyName">欲获取的项名或条目名。这个字串不区分大小写。如设为null，就在lpReturnedString缓冲区内装载指定小节所有项的列表</param>
            /// <param name="lpDefault">指定的条目没有找到时返回的默认值。可设为空（""）</param>
            /// <param name="lpReturnedString">指定一个字串缓冲区，长度至少为nSize</param>
            /// <param name="nSize">指定装载到lpReturnedString缓冲区的最大字符数量</param>
            /// <param name="lpFileName">初始化文件的名字。如没有指定一个完整路径名，windows就在Windows目录中查找文件</param>
            /// 注意：如lpKeyName参数为null，那么lpReturnedString缓冲区会载入指定小节所有设置项的一个列表。
            /// 每个项都用一个NULL字符分隔，最后一个项用两个NULL字符中止。也请参考GetPrivateProfileInt函数的注解
            /// <returns></returns>
            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern int GetPrivateProfileString(string lpApplicationName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

            /// <summary>
            /// Windows API 对INI文件读方法
            /// </summary>
            /// <param name="lpAppName">欲在其中查找条目的小节名称。这个字串不区分大小写。</param>
            /// <param name="lpReturnedString">指定一个字串缓冲区，长度至少为nSize</param>
            /// <param name="nSize">指定装载到lpReturnedString缓冲区的最大字符数量</param>
            /// <param name="lpFileName">初始化文件的名字。如没有指定一个完整路径名，windows就在Windows目录中查找文件</param>
            /// <returns></returns>
            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern int GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, int nSize, string lpFileName);
        }
    }
}

