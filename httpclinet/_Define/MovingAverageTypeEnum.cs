using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpclinet._Define
{
    /// <summary>
    /// 均線類型
    /// </summary>
    public enum MovingAverageType
    {
        /// <summary> 5日均線（周線、5 MA） </summary>
        FiveDayMovingAverage,
        /// <summary>10日均線（10 MA） </summary>
        TenDayMovingAverage,
        /// <summary>20日均線（月線、20 MA） </summary>
        TwentyDayMovingAverage,
        /// <summary>60日均線（季線、60 MA） </summary>
        SixtyDayMovingAverage
    }
}
