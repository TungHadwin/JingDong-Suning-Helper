// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CefSharp.MinimalExample.WinForms.Controls;
using CefSharp.WinForms;
using CefSharp;

namespace CefSharp.MinimalExample.WinForms
{
    public partial class BrowserForm : Form
    {
        private readonly ChromiumWebBrowser browser;

        private bool page_isloading = true;
        private int kami_index = 0;


        public BrowserForm()
        {
            InitializeComponent();

            Text = "购物卡密辅助软件";
            WindowState = FormWindowState.Maximized;

            browser = new ChromiumWebBrowser("https://u.jd.com/hUvc5c")  //https://coin.jd.com/#banklist  www.jd.com http://www.atool.org/canvas.php
            {
                //Dock = DockStyle.Fill,
            };
            toolStripContainer.ContentPanel.Controls.Add(browser);

            browser.IsBrowserInitializedChanged += OnIsBrowserInitializedChanged;
            browser.LoadingStateChanged += OnLoadingStateChanged;
            browser.ConsoleMessage += OnBrowserConsoleMessage;
            browser.StatusMessage += OnBrowserStatusMessage;
            browser.TitleChanged += OnBrowserTitleChanged;
            browser.AddressChanged += OnBrowserAddressChanged;
            browser.LifeSpanHandler = new OpenPageSelf();
            //browser.DocumentCompleted += OnDocumentCompleted;

            var bitness = Environment.Is64BitProcess ? "x64" : "x86";
            var version = String.Format("Chromium: {0}, CEF: {1}, CefSharp: {2}, Environment: {3}", Cef.ChromiumVersion, Cef.CefVersion, Cef.CefSharpVersion, bitness);
            DisplayOutput(version);
        }

        private void OnIsBrowserInitializedChanged(object sender, IsBrowserInitializedChangedEventArgs e)
        {
            if(e.IsBrowserInitialized)
            {
                var b = ((ChromiumWebBrowser)sender);

                this.InvokeOnUiThreadIfRequired(() => b.Focus());
            }
        }

        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args)
        {
            DisplayOutput(string.Format("Line: {0}, Source: {1}, Message: {2}", args.Line, args.Source, args.Message));
        }

        private void OnBrowserStatusMessage(object sender, StatusMessageEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => statusLabel.Text = args.Value);
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            SetCanGoBack(args.CanGoBack);
            SetCanGoForward(args.CanGoForward);

            this.InvokeOnUiThreadIfRequired(() => SetIsLoading(!args.CanReload));

            page_isloading = args.IsLoading;
        }

        private void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => Text = args.Title);
        }

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args)
        {
            //this.InvokeOnUiThreadIfRequired(() => urlTextBox.Text = args.Address);
        }

        private void SetCanGoBack(bool canGoBack)
        {
            this.InvokeOnUiThreadIfRequired(() => backButton.Enabled = canGoBack);
        }

        private void SetCanGoForward(bool canGoForward)
        {
            this.InvokeOnUiThreadIfRequired(() => forwardButton.Enabled = canGoForward);
        }

        private void SetIsLoading(bool isLoading)
        {
            /*goButton.Text = isLoading ?
                "Stop" :
                "Go";
            goButton.Image = isLoading ?
                Properties.Resources.nav_plain_red :
                Properties.Resources.nav_plain_green;
            
            HandleToolStripLayout();*/
        }

        public void DisplayOutput(string output)
        {
            this.InvokeOnUiThreadIfRequired(() => outputLabel.Text = output);
        }

        private void HandleToolStripLayout(object sender, LayoutEventArgs e)
        {
            //HandleToolStripLayout();
        }

        /*private void HandleToolStripLayout()
        {
            var width = toolStrip1.Width;
            foreach (ToolStripItem item in toolStrip1.Items)
            {
                if (item != urlTextBox)
                {
                    width -= item.Width - item.Margin.Horizontal;
                }
            }
            urlTextBox.Width = Math.Max(0, width - urlTextBox.Margin.Horizontal - 18);
        }*/

        private void ExitMenuItemClick(object sender, EventArgs e)
        {
            browser.Dispose();
            Cef.Shutdown();
            Close();
        }

        private void GoButtonClick(object sender, EventArgs e)
        {
            //LoadUrl(urlTextBox.Text);
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            browser.Back();
        }

        private void ForwardButtonClick(object sender, EventArgs e)
        {
            browser.Forward();
        }

        private void UrlTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            //LoadUrl(urlTextBox.Text);
        }

        private void LoadUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                browser.Load(url);
            }
        }

        private void ShowDevToolsMenuItemClick(object sender, EventArgs e)
        {
            browser.ShowDevTools();
        }

        //打开新窗口处理
        internal class OpenPageSelf : ILifeSpanHandler
        {
            public bool DoClose(IWebBrowser browserControl, IBrowser browser)
            {
                return false;
            }

            public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
            {

            }

            public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
            {

            }

            public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl,
                string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures,
                        IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
            {
                newBrowser = null;
                var chromiumWebBrowser = (ChromiumWebBrowser)browserControl;
                chromiumWebBrowser.Load(targetUrl);
                return true; //Return true to cancel the popup creation copyright by codebye.com.
            }
        }

        //显示调试信息
        static void ShowLabel(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
            //this.InvokeOnUiThreadIfRequired(() => label1.Text = msg);
        }

        //鼠标
        private void WebMouseMove(int x, int y)
        {
            var host = browser.GetBrowser().GetHost();
            host.SendMouseMoveEvent((int)x, (int)y, false, CefEventFlags.LeftMouseButton);//移动鼠标
        }

        private void WebMouseClick(int x, int y)
        {
            var host = browser.GetBrowser().GetHost();
            host.SendMouseClickEvent(x, y, MouseButtonType.Left, true, 1, CefEventFlags.None);//抬起鼠标左键
        }

        private void open_logic_page_Click(object sender, EventArgs e)
        {
            //browser.Load("https://passport.jd.com/new/login.aspx");

            var frame = browser.GetFocusedFrame();

            //Execute extension method
            frame.OpenLogicPage().ContinueWith(task =>
            {
                // Now we're not on the main thread, perhaps the
                // Cef UI thread. It's not safe to work with
                // form UI controls or to block this thread.
                // Queue up a delegate to be executed on the
                // main thread.
                BeginInvoke(new Action(() =>
                {
                    string message;
                    if (task.Exception == null)
                    {
                        message = task.Result.ToString();
                    }
                    else
                    {
                        message = string.Format("Script evaluation failed. {0}", task.Exception.Message);
                    }
                }));
            });

            System.Diagnostics.Trace.WriteLine("开始京东页面");
        }

        private void open_gangben_page_Click(object sender, EventArgs e)
        {
            browser.Load("https://coin.jd.com/#banklist");
        }

        private void GangBengBangDing(int index)
        {
            if (kami_index < richTextBoxKaimi.Lines.Length)
            {
                var frame = browser.GetFocusedFrame();

                frame.InputGangBengKami(richTextBoxKaimi.Lines[index]).ContinueWith(task =>
                {
                    // Now we're not on the main thread, perhaps the
                    // Cef UI thread. It's not safe to work with
                    // form UI controls or to block this thread.
                    // Queue up a delegate to be executed on the
                    // main thread.
                    string message;
                    if (task.Exception == null)
                    {
                        if (task.Result) //立即提交 点击成功
                        {
                            System.Threading.Thread.Sleep(1500 + (new Random().Next(300))); //延时1秒

                            frame.ReadGangBengKamiResult().ContinueWith(readtask =>  //确认绑定按钮
                            {
                                if (readtask.Exception == null)
                                {
                                    message = readtask.Result;

                                    // Now we're not on the main thread, perhaps the
                                    // Cef UI thread. It's not safe to work with
                                    // form UI controls or to block this thread.
                                    // Queue up a delegate to be executed on the
                                    // main thread.
                                   
                                    System.Threading.Thread.Sleep(500+(new Random().Next(300))); //延时1秒

                                    //成功了 
                                    if (message.IndexOf("实际到账") != -1)
                                    {
                                        //点击确定关闭按钮
                                        frame.GangBengKamiOk().ContinueWith(oktask =>
                                        {
                                            if (oktask.Exception == null)
                                            {
                                                //oktask.Result;

                                                System.Threading.Thread.Sleep(1500+(new Random().Next(200))); //延时1秒

                                                //更新显示
                                                this.InvokeOnUiThreadIfRequired(() => UpdateLineState(index, message));
                                                
                                                GangBengNextBangDing();
                                            }
                                        });
                                    }
                                    else
                                    {
                                        //更新显示
                                        this.InvokeOnUiThreadIfRequired(() => UpdateLineState(index, message));
                                        GangBengNextBangDing();
                                    }                                       
                                }
                                else
                                {
                                    message = string.Format("Script evaluation failed. {0}", readtask.Exception.Message);
                                }
                            });
                        }
                    }
                    else
                    {
                        message = string.Format("Script evaluation failed. {0}", task.Exception.Message);
                    }
                });
            }
        }

        private void GangBengNextBangDing()
        {
            //绑定下一个
            this.InvokeOnUiThreadIfRequired(() => GangBengBangDing(++kami_index));
        }

        private void start_bangding_Click(object sender, EventArgs e)
        {
            //打开钢镚页面
            page_isloading = true;
            browser.Load("https://coin.jd.com/#banklist");
            while (page_isloading)
                System.Threading.Thread.Sleep(1000);

            //初始化
            kami_index = 0;
            GangBengBangDing(kami_index);
        }

        private void UpdateLineState(int index, string msg)
        {
            if(index < richTextBoxKaimi.Lines.Length)
            {
                string text = richTextBoxKaimi.Lines[index]; //拿到此行文本
                int lineFirstCharIndex = richTextBoxKaimi.GetFirstCharIndexFromLine(index);//此行第一个char的索引
                                                                                           //System.Diagnostics.Debug.WriteLine(richTextBoxKaimi.Lines[index]);
                //!!网络延迟,未知错误!!

                //此卡密无效

                //"需绑定的卡密【6E4C-8CBC-7306-9004】为已绑定状态！"

                //"面额：200 京东钢镚
                //本月已兑换：27 次（每月免费10次）
                //手续费：2 钢镚 手续费收取规则
                //实际到账：198 钢镚"

                if (msg.IndexOf("已绑定") != -1)
                    text = richTextBoxKaimi.Lines[index] += " 失败:卡密已绑定"; // + msg.Substring(7,19);  //修改此行文本
                else if(msg.IndexOf("实际到账") != -1)
                    text = richTextBoxKaimi.Lines[index] += (" 成功:"+ msg.Substring(msg.Length-6,6));
                else if (msg.IndexOf("卡密无效") != -1)
                    text = richTextBoxKaimi.Lines[index] += " 失败:卡密无效";
                else
                    text = richTextBoxKaimi.Lines[index] += " 失败:网络延迟"; 

                richTextBoxKaimi.SelectionStart = lineFirstCharIndex;
                richTextBoxKaimi.SelectionLength = richTextBoxKaimi.Lines[index].Length;
                richTextBoxKaimi.SelectedText = text;//塞回文本..

                richTextBoxKaimi.Update();
            }
        }
    }
}
