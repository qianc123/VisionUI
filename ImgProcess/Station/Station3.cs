﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HalconDotNet;
using System.Threading;

namespace ImgProcess.Station
{
    public class Station3
    {
        private System.Threading.Thread _runThread;

        private event EventHandler<Event.EventArgsStation3> _ImgProcessStation3 = null;

        private System.Threading.AutoResetEvent _are = new System.Threading.AutoResetEvent(true);

        private Queue<ImgPackage> _ImgInfo = new Queue<ImgPackage>();

        private System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
        private int _index = 0;
        public void addImg(int id, HObject img)
        {
            this._ImgInfo.Enqueue(new ImgPackage(id, img.Clone()));
        }

        private void runProcess()
        {
            int mID = -1;
            int mStepID = 0;
            bool mResult = false;
            ImgPackage mImgInfo = null;

            HTuple hv_Height = new HTuple(), hv_Width = new HTuple();

            do
            {
                if (this._ImgInfo.Count > 0)
                {

                    try
                    {
                        this._are.WaitOne();
                        this._stopwatch.Restart();
                        this._index++;
                        mImgInfo = this._ImgInfo.Dequeue();

                        for (int i = 0; i < 1000000; i++)
                        {
                            int ii = i;
                        }


                        HOperatorSet.GetImageSize(mImgInfo.Img, out hv_Width, out hv_Height);

                        if (hv_Height.I > 100)
                        {
                            mResult = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        //mID = -1;
                        LogRecord.LogHelper.Error(typeof(Station3), string.Format("{0}", ex));
                    }

                    OnNewEvent(new Event.EventArgsStation3(mImgInfo.ID, mStepID, mResult));

                    if (mImgInfo != null)
                    {
                        mImgInfo.Img.Dispose();
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(10);
                }

            } while (true);


        }


        /// <summary>
        /// 事件触发方法
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnNewEvent(Event.EventArgsStation3 e)
        {
            EventHandler<Event.EventArgsStation3> handler = this._ImgProcessStation3;

            if (handler != null)
            {
                //获取所有注册了的委托事件
                Delegate[] m_handlers = handler.GetInvocationList();

                foreach (var item in m_handlers)
                {
                    //遍历判断事件列表，执行当前触发的事件
                    if (item.Target.ToString().Equals(handler.Target.ToString()))
                    {
                        EventHandler<Event.EventArgsStation3> mhandler = (EventHandler<Event.EventArgsStation3>)item;
                        if (mhandler != null)
                        {
                            AsyncCallback EventDone = new AsyncCallback(CallBack);
                            //异步执行
                            mhandler.BeginInvoke(this, e, EventDone, e);
                        }
                    }
                }
            }
        }
        private void CallBack(IAsyncResult result)
        {
            this._stopwatch.Stop();
            Console.WriteLine("ElapsedTime {0}  {1}", this._stopwatch.ElapsedMilliseconds, this._index);
            this._are.Set();
            GC.WaitForFullGCComplete();
            GC.Collect();
        }

        public Station3(EventHandler<Event.EventArgsStation3> callback)
        {
            this._ImgProcessStation3 = callback;
            this._runThread = new Thread(new ThreadStart(runProcess));
            this._runThread.IsBackground = true;
            this._runThread.Name = "Station3";
            this._runThread.Start();
        }

        ~Station3()
        {
            foreach (var item in this._ImgInfo)
            {
                item.Img.Dispose();
            }
        }
    }
}
