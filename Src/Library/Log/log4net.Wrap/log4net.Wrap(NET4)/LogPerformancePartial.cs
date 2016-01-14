using System.Runtime.CompilerServices;
using System.Diagnostics;
using Qinjin.Library.Log.log4net.Wrap;

namespace Fre.Library.Log.log4net.Wrap
{
    public static partial class Log
    {
        #region PerformanceRecord

        /// <summary>
        /// 性能计数开始
        /// <remarks>性能计数本身会消耗性能，在想统计性能的方法段的开始调用该方法，在末尾调用PerformanceStop()方法可输出日志，两者必须匹配</remarks>
        /// </summary>
        public static void PerformanceStart(object key)
        {
            if (key != null && !string.IsNullOrEmpty(key.ToString()))
            {
                PerformanceHelper.StartPerformance(key.ToString());
            }
        }

        /// <summary>
        /// 性能计数开始
        /// <remarks>性能计数本身会消耗性能，在想统计性能的方法段的开始调用该方法，在末尾调用PerformanceStop()方法可输出日志，两者必须匹配</remarks>
        /// </summary>
        public static void PerformanceStart()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var stackFrame = new StackFrame(1, true);
            var fileName = stackFrame.GetFileName();
            var methodName = stackFrame.GetMethod().Name;

            stopWatch.Stop();

            PerformanceHelper.StartPerformance(fileName, methodName, stopWatch.ElapsedTicks);
        }

        /// <summary>
        /// 性能计数结束
        /// <remarks>性能计数本身会消耗性能，在想统计性能的方法段的开始调用该方法，在末尾调用PerformanceStop()方法可输出日志，两者必须匹配</remarks>
        /// </summary>
        public static void PerformanceStop(object key)
        {
            if (key != null && !string.IsNullOrEmpty(key.ToString()))
            {
                PerformanceHelper.StopPerformance(key.ToString());
            }
        }

        /// <summary>
        /// 性能计数结束
        /// <remarks>性能计数本身会消耗性能，在想统计性能的方法段的开始调用该方法，在末尾调用PerformanceStop()方法可输出日志，两者必须匹配</remarks>
        /// </summary>
        public static void PerformanceStop()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var stackFrame = new StackFrame(1, true);
            var fileName = stackFrame.GetFileName();
            var methodName = stackFrame.GetMethod().Name;

            stopWatch.Stop();

            PerformanceHelper.StopPerformance(fileName, methodName, stopWatch.ElapsedTicks);
        }

        #endregion
    }
}
