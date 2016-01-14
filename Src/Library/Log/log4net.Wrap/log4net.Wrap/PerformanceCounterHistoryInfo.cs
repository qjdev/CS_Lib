namespace Qinjin.Library.Log.log4net.Wrap
{
    /// <summary>
    /// 性能统计的历史记录实体
    /// </summary>
    internal class PerformanceCounterHistoryInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        internal string Name { get; private set; }

        /// <summary>
        /// 总调用次数
        /// </summary>
        internal int NumberOfInvokeTimes { get; set; }

        /// <summary>
        /// 该方法的总消耗时间
        /// </summary>
        internal long TotalElapsedTicks { get; set; }


        internal PerformanceCounterHistoryInfo(string name, long elapsedTicks)
        {
            Name = name;
            TotalElapsedTicks = elapsedTicks;
            NumberOfInvokeTimes = 1;
        }
    }
}
