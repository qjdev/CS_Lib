using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Fre.Library.Utility.Extensions
{
    /// <summary>
    /// 触发事件的扩展方法
    /// </summary>
    public static class EventArgExtensiongs
    {
        /// <summary>
        /// 线程安全的触发事件辅助方法
        /// </summary>
        /// <typeparam name="TEventArgs">事件参数类型</typeparam>
        /// <param name="e">事件参数对象</param>
        /// <param name="sender">事件触发者</param>
        /// <param name="eventDelegate">事件委托</param>
        public static void Raise<TEventArgs>(this TEventArgs e, object sender,
            ref EventHandler<TEventArgs> eventDelegate) where TEventArgs : EventArgs
        {
            EventHandler<TEventArgs> temp = Volatile.Read(ref eventDelegate);

            temp?.Invoke(sender, e);
        }
    }
}
