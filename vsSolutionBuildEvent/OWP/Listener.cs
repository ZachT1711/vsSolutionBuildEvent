﻿/* 
 * Boost Software License - Version 1.0 - August 17th, 2003
 * 
 * Copyright (c) 2013 Developed by reg [Denis Kuzmin] <entry.reg@gmail.com>
 * 
 * Permission is hereby granted, free of charge, to any person or organization
 * obtaining a copy of the software and accompanying documentation covered by
 * this license (the "Software") to use, reproduce, display, distribute,
 * execute, and transmit the Software, and to prepare derivative works of the
 * Software, and to permit third-parties to whom the Software is furnished to
 * do so, all subject to the following:
 * 
 * The copyright notices in the Software and this entire statement, including
 * the above license grant, this restriction and the following disclaimer,
 * must be included in all copies of the Software, in whole or in part, and
 * all derivative works of the Software, unless such copies or derivative
 * works are solely in the form of machine-executable object code generated by
 * a source language processor.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT
 * SHALL THE COPYRIGHT HOLDERS OR ANYONE DISTRIBUTING THE SOFTWARE BE LIABLE
 * FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER IN CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using System.Threading;

namespace net.r_eg.vsSBE.OWP
{
    /// <summary>
    /// Working with the OutputWindowsPane
    /// Must receive and send different data for own subscribers
    /// </summary>
    internal class Listener: SynchSubscribers<IListenerOWPL>
    {
        /// <summary>
        /// Keep events for any pane
        /// </summary>
        protected OutputWindowEvents evtOWP;

        //TODO: fix me. Prevent Duplicate Data / bug with OutputWindowPane
        protected SynchronizedCollection<string> dataList = new SynchronizedCollection<string>();
        protected System.Threading.Thread tUpdated;

        /// <summary>
        /// Used item by name
        /// </summary>
        protected string item;

        /// <summary>
        /// previous count of lines for EditPoint::GetLines
        /// </summary>
        private int _prevCountLines = 1;

        /// <summary>
        /// events handlers
        /// </summary>
        private _dispOutputWindowEvents_PaneUpdatedEventHandler _ePUpdated;
        private _dispOutputWindowEvents_PaneAddedEventHandler _ePAdded;
        private _dispOutputWindowEvents_PaneClearingEventHandler _ePClearing;

        private Object _eLock = new Object();

        public void attachEvents()
        {
            lock(_eLock) {
                detachEvents();
                evtOWP.PaneUpdated     += _ePUpdated;
                evtOWP.PaneAdded       += _ePAdded;
                evtOWP.PaneClearing    += _ePClearing;
            }
        }

        public void detachEvents()
        {
            lock(_eLock) {
                evtOWP.PaneUpdated     -= _ePUpdated;
                evtOWP.PaneAdded       -= _ePAdded;
                evtOWP.PaneClearing    -= _ePClearing;
            }
        }

        public Listener(IEnvironment env, string item)
        {
            this.item   = item;
            evtOWP      = env.DTE2.Events.get_OutputWindowEvents(item);
            _ePUpdated  = new _dispOutputWindowEvents_PaneUpdatedEventHandler(evtPaneUpdated);
            _ePAdded    = new _dispOutputWindowEvents_PaneAddedEventHandler(evtPaneAdded);
            _ePClearing = new _dispOutputWindowEvents_PaneClearingEventHandler(evtPaneClearing);
        }

        /// <summary>
        /// all collection must receive raw-data
        /// TODO: fix me. Prevent Duplicate Data / bug with OutputWindowPane
        /// </summary>
        protected virtual void notifyRaw()
        {
            if(dataList.Count < 1) {
                return;
            }

            lock(_eLock)
            {
                string envelope = "";
                while(dataList.Count > 0) {
                    envelope += dataList[0];
                    dataList.RemoveAt(0);
                }

                updateComponent(envelope);
                foreach(IListenerOWPL l in subscribers) {
                    l.raw(envelope);
                }
            }

            if(dataList.Count > 0) {
                notifyRaw();
            }
        }

        protected virtual void evtPaneUpdated(OutputWindowPane pane)
        {
            TextDocument textD   = pane.TextDocument;
            int countLines       = textD.EndPoint.Line;

            if(countLines <= 1 || countLines - _prevCountLines < 1) {
                return;
            }

            EditPoint point = textD.StartPoint.CreateEditPoint();

            // text between Start (inclusive) and ExclusiveEnd (exclusive)
            dataList.Add(point.GetLines(_prevCountLines, countLines)); // e.g. first line: 1, 2
            _prevCountLines = countLines;

            //TODO: fix me. Prevent Duplicate Data / bug with OutputWindowPane
            if(tUpdated == null || tUpdated.ThreadState == ThreadState.Unstarted || tUpdated.ThreadState == ThreadState.Stopped)
            {
                tUpdated = new System.Threading.Thread(() => { notifyRaw(); });
                try {
                    tUpdated.Start();
                }
                catch(Exception e) {
                    Log.nlog.Warn("notifyRaw() {0}", e.Message);
                }
            }
        }

        protected virtual void evtPaneAdded(OutputWindowPane pane)
        {
            _prevCountLines = 1;
            dataList.Clear();
        }

        protected virtual void evtPaneClearing(OutputWindowPane pane)
        {
            _prevCountLines = 1;
            dataList.Clear();
        }

        protected void updateComponent(string data)
        {
            switch(item) {
                case "Build": {
                    Items._.Build.updateRaw(data);
                    return;
                }
            }
        }
    }
}
