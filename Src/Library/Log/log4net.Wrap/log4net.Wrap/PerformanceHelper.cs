using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using log4net;

namespace Qinjin.Library.Log.log4net.Wrap
{
    /// <summary>
    /// 性能计数辅助
    /// </summary>
    internal class PerformanceHelper
    {
        private readonly static ILog _performanceHelperLog;

        private readonly static Dictionary<string, PerformanceCounterHistoryInfo> _performanceCounterHistory;

        private readonly static Dictionary<string, PerformanceCounter> _performanceCounterCache;

        private readonly static Dictionary<string, PerformanceCounter> _performanceCounterCacheByCustomKey;

        internal static long FrequencyMilliseconds;

        static PerformanceHelper()
        {
            _performanceHelperLog = LogManager.GetLogger("Performance");
            FrequencyMilliseconds = (long)Math.Round(Stopwatch.Frequency / 1000d);
            _performanceCounterCache = new Dictionary<string, PerformanceCounter>();
            _performanceCounterHistory = new Dictionary<string, PerformanceCounterHistoryInfo>();
            _performanceCounterCacheByCustomKey = new Dictionary<string, PerformanceCounter>();
        }

        /// <summary>
        /// 按用户指定键值启动对应计时器
        /// </summary>
        /// <param name="key">用户自定义键</param>
        internal static void StartPerformance(string key)
        {
            if (_performanceCounterCacheByCustomKey.ContainsKey(key))
            {
                if (!_performanceCounterCacheByCustomKey[key].IsRunning)
                {
                    //已经停止，重新开始计时
                    _performanceCounterCacheByCustomKey[key].ReStart();
                }
                else
                {
                    //如果正在运行则抛弃，应为还未停止上一次计时就启动下一次了
                    _performanceHelperLog.Warn(string.Format("用户自定义计时器[{0}]被启动多次", key));
                }
            }
            else
            {
                _performanceCounterCacheByCustomKey.Add(key, new PerformanceCounter(key));
            }
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        /// <param name="filePath">来源的文件路径</param>
        /// <param name="methodName">方法名</param>
        /// <param name="extraElapsedTicks">额外消耗的性能计时</param>
        internal static void StartPerformance(string filePath, string methodName, long extraElapsedTicks = 0)
        {
            var name = string.Concat(filePath, filePath, Thread.CurrentThread.ManagedThreadId);

            if (_performanceCounterCache.ContainsKey(name))
            {
                if (!_performanceCounterCache[name].IsRunning)
                {
                    //已经停止，重新开始计时
                    _performanceCounterCache[name].ReStart();
                    _performanceCounterCache[name].ExtraElapsedTicks = extraElapsedTicks;
                }
                else
                {
                    //如果正在运行则抛弃，应为还未停止上一次计时就启动下一次了
                    _performanceHelperLog.Warn(
                        string.Format("{0}计时器被启动多次", _performanceCounterCache[name].ShortName));
                }
            }
            else
            {
                _performanceCounterCache.Add(name, new PerformanceCounter(filePath, methodName, Thread.CurrentThread.ManagedThreadId));
                _performanceCounterCache[name].ExtraElapsedTicks = extraElapsedTicks;
            }
        }

        /// <summary>
        /// 停止并输出用户指定的计时器
        /// </summary>
        /// <param name="key">用户自定义键</param>
        internal static void StopPerformance(string key)
        {
            if (_performanceCounterCacheByCustomKey.ContainsKey(key))
            {
                if (_performanceCounterCacheByCustomKey[key].IsRunning)
                {
                    var elapsedMilliseconds = _performanceCounterCacheByCustomKey[key].Stop();
                    var historyInfo = AddHistoryInfo(_performanceCounterCacheByCustomKey[key]);

                    _performanceHelperLog.Info(
                        string.Format(
                            "{0} 耗时：{1} 【总量统计】共调用次数：{2} 共消耗时间：{3}",
                            _performanceCounterCacheByCustomKey[key].ShortName,
                            elapsedMilliseconds / FrequencyMilliseconds,
                            historyInfo.NumberOfInvokeTimes,
                            historyInfo.TotalElapsedTicks / FrequencyMilliseconds));
                }
                else
                {
                    _performanceHelperLog.Warn(string.Format("用户自定义计时器[{0}]还未启动就停止", key));
                }
            }
            else
            {
                _performanceHelperLog.Warn(string.Format("用户自定义计时器[{0}]还未启动就停止", key));
            }
        }

        /// <summary>
        /// 停止计时
        /// </summary>
        /// <param name="filePath">来源的文件路径</param>
        /// <param name="methodName">方法名</param>
        /// <param name="extraElapsedTicks">额外消耗的性能计时</param>
        internal static void StopPerformance(string filePath, string methodName, long extraElapsedTicks = 0)
        {
            var name = string.Concat(filePath, filePath, Thread.CurrentThread.ManagedThreadId);

            if (_performanceCounterCache.ContainsKey(name))
            {
                if (_performanceCounterCache[name].IsRunning)
                {
                    _performanceCounterCache[name].ExtraElapsedTicks += extraElapsedTicks;
                    var elapsedMilliseconds = _performanceCounterCache[name].Stop();
                    var historyInfo = AddHistoryInfo(_performanceCounterCache[name]);

                    _performanceHelperLog.Info(
                        string.Format(
                            "{0} 耗时：{1} 【总量统计】共调用次数：{2} 共消耗时间：{3}",
                            string.Format("{0} {1}", _performanceCounterCache[name].FilePath, _performanceCounterCache[name].MethodName),
                            elapsedMilliseconds / FrequencyMilliseconds,
                            historyInfo.NumberOfInvokeTimes,
                            historyInfo.TotalElapsedTicks / FrequencyMilliseconds));
                }
                else
                {
                    _performanceHelperLog.Warn(string.Format("{0}还未启动就停止", name));
                }
            }
            else
            {
                _performanceHelperLog.Warn(string.Format("{0}还未启动就停止", name));
            }
        }

        private static PerformanceCounterHistoryInfo AddHistoryInfo(PerformanceCounter performanceCounter)
        {
            lock (_performanceCounterHistory)
            {
                if (_performanceCounterHistory.ContainsKey(performanceCounter.ShortName))
                {

                    _performanceCounterHistory[performanceCounter.ShortName].NumberOfInvokeTimes++;
                    _performanceCounterHistory[performanceCounter.ShortName].TotalElapsedTicks +=
                        performanceCounter.LastElapsedTicks;
                }
                else
                {
                    _performanceCounterHistory.Add(
                        performanceCounter.ShortName,
                        new PerformanceCounterHistoryInfo(
                            performanceCounter.ShortName,
                            performanceCounter.LastElapsedTicks));
                }

                return _performanceCounterHistory[performanceCounter.ShortName];
            }
        }
    }

    /// <summary>
    /// 性能统计实例，只会有单线程操作内部东西，安全
    /// </summary>
    internal class PerformanceCounter
    {
        /// <summary>
        /// 总时间计时器
        /// </summary>
        private Stopwatch TotalStopwatch { get; set; }

        /// <summary>
        /// 名称：文件名 + 方法名 + 线程Id，匹配单次调用的唯一性
        /// </summary>
        internal string Name { get; private set; }

        /// <summary>
        /// 短名称：除去线程Id的名称，匹配方法调用的唯一性
        /// </summary>
        internal string ShortName { get; private set; }

        /// <summary>
        /// 文件名
        /// </summary>
        internal string FilePath { get; private set; }

        /// <summary>
        /// 方法名
        /// </summary>
        internal string MethodName { get; private set; }

        /// <summary>
        /// 调用线程Id
        /// </summary>
        internal int ThreadId { get; private set; }

        /// <summary>
        /// 最后一次调用耗时
        /// </summary>
        internal long LastElapsedTicks
        {
            get
            {
                return TotalStopwatch.ElapsedTicks + ExtraElapsedTicks;
            }
        }

        internal long ExtraElapsedTicks { get; set; }

        /// <summary>
        /// 是否正在计时
        /// </summary>
        internal bool IsRunning
        {
            get
            {
                if (TotalStopwatch != null)
                {
                    return TotalStopwatch.IsRunning;
                }

                return false;
            }
        }

        private PerformanceCounter()
        {
            TotalStopwatch = new Stopwatch();
            TotalStopwatch.Start();
        }

        internal PerformanceCounter(string key)
            : this()
        {
            Name = key;
            ShortName = key;
        }

        internal PerformanceCounter(string filePath, string methodName, int threadId)
            : this()
        {
            FilePath = filePath;
            MethodName = methodName;
            ThreadId = threadId;
            Name = string.Concat(FilePath, MethodName, ThreadId);
            ShortName = string.Concat(FilePath, MethodName);
        }

        /// <summary>
        /// 重启计时
        /// </summary>
        internal void ReStart()
        {
            TotalStopwatch.Restart();
        }

        /// <summary>
        /// 停止计时并输出本次总时长
        /// </summary>
        /// <returns></returns>
        internal long Stop()
        {
            TotalStopwatch.Stop();
            return LastElapsedTicks;
        }
    }
}
