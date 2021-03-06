﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph.Modules
{
    /// <summary>
    /// 暦モジュール
    /// 
    ///    +---------+
    ///    |         |
    ///    |         +--> DayOfYearOut
    ///    |         |
    ///    |         +--> HourOut
    ///    |   暦M   |
    ///    |         +--> MinuteOut
    ///    |         |
    ///    |         +--> SecondOut
    ///    |         |
    ///    +-+--+--+-+
    ///         |  
    ///         +-- BeginDay 
    ///         +-- TotalDays
    ///         +-- TickSecond 
    /// </summary>
    public class CalendarModule : BaseModule
    {
        /// <summary>
        /// 計算間隔 [s]
        /// </summary>
        public int TickSecond { get; set; }

        /// <summary>
        /// 計算開始日
        /// </summary>
        public int BeginDay { get; set; }

        /// <summary>
        /// 計算日数
        /// </summary>
        public int TotalDays { get; set; }

        /// <summary>
        /// 年間積算日[日] (1始まり最大366)
        /// </summary>
        public IVariable<int> DayOfYearOut { get; private set; }

        /// <summary>
        /// 時刻の時間部分(0-23)
        /// </summary>
        public IVariable<int> HourOut { get; private set; }

        /// <summary>
        /// 時刻の分部分(0-59)
        /// </summary>
        public IVariable<int> MinuteOut { get; private set; }

        /// 時刻の秒部分(0-59)
        public IVariable<int> SecondOut { get; private set; }

        private int[] day, hour, minute, second;

        public CalendarModule()
        {
            Label = "暦M";

            DayOfYearOut = new Variable<int>(t => day[t]);
            HourOut = new Variable<int>(t => hour[t]);
            MinuteOut = new Variable<int>(t => minute[t]);
            SecondOut = new Variable<int>(t => second[t]);
        }

        public override void Init(FunctionFactory F)
        {
            if (TotalDays <= 0) throw new InvalidOperationException();
            if (TickSecond <= 0) throw new InvalidOperationException();

            //切りの悪い値が指定されていないか確認
            if( TickSecond < 60 )
            {
                if( 60 % TickSecond != 0 )
                {
                    throw new NotSupportedException($"計算間隔が60秒未満ですが、60秒の整数分の1ではないため処理を続行できません。");
                }
            }
            else if( TickSecond < 3600)
            {
                if (3600 % TickSecond != 0 || 60 % TickSecond != 0)
                {
                    throw new NotSupportedException($"計算間隔が60分未満ですが、60分の整数分の1ではないため処理を続行できません。");
                }
            }
            else if (TickSecond > 3600)
            {
                throw new NotSupportedException($"計算間隔の最大は60分(3600)です。");
            }

            //生成要素数
            int n = TotalDays * 24 * 60 * 60 / TickSecond;

            //メモリ確保
            day = new int[n];
            hour = new int[n];
            minute = new int[n];
            second = new int[n];

            //記録
            int t = BeginDay * (24 * 60 * 60 / TickSecond);
            if (TickSecond < 60)
            {
                //秒単位の計算

                for (int i = 0; i < TotalDays; i++)
                {
                    for (int h = 0; h < 24; h++)
                    {
                        for (int m = 0; m < 60; m++)
                        {
                            for (int s = 0; s < 60; s += TickSecond, t++)
                            {
                                day[t] = i + 1;
                                hour[t] = h;
                                minute[t] = m;
                                second[t] = s;
                            }
                        }
                    }
                }
            }
            else
            {
                //分単位の計算

                for (int i = 0; i < TotalDays; i++)
                {
                    for (int h = 0; h < 24; h++)
                    {
                        for (int m = 0; m < 60; m += TickSecond / 60, t++)
                        {
                            day[t] = i + 1;
                            hour[t] = h;
                            minute[t] = m;
                            second[t] = 0;
                        }
                    }
                }
            }
        }
    }
}
