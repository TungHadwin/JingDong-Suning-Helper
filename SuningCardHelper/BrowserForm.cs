// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SuningCardHelper.Controls;
using CefSharp.WinForms;
using CefSharp;

namespace SuningCardHelper
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

            browser = new ChromiumWebBrowser("https://www.suning.com")
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
            SetCanReload(args.CanReload);

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

        private void SetCanReload(bool canGoReload)
        {
            this.InvokeOnUiThreadIfRequired(() => reloadButton.Enabled = canGoReload);
        }

        private void ReloadButton_Click(object sender, EventArgs e)
        {
            browser.Reload();
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

        private bool StringToTongGangString(string kami, out string tongbang)
        {
            bool result = true;
            tongbang = "";

            /*处理卡密 3种格式
                1、3B8B-CFA6-632C-701C
                2、卡号：8992-4276-1CBA-3848
                3、820G0H090MY8D6AD
            */

            //3、820G0H090MY8D6AD
            if (kami.Length == 16)
            {
                tongbang = kami;
            }
            else
            {
                foreach (char item in kami)
                {
                    if ((item >= '0' && item <= '9')|| (item >= 'a' && item <= 'z') || (item >= 'A' && item <= 'Z'))
                    {
                        tongbang += item;
                    }
                }

                if (tongbang.Length!=16)
                {
                    MessageBox.Show("卡密格式错误");
                    result = false;
                }
            }
            return result;
        }

        private void TongGangBangDing(int index)
        {
            if (kami_index < richTextBoxKaimi.Lines.Length)
            {
                var frame = browser.GetFocusedFrame();

                string tongbangcode;
                if (StringToTongGangString(richTextBoxKaimi.Lines[index], out tongbangcode))
                {
                    //输入卡密
                    frame.InputTongGangKami(tongbangcode).ContinueWith(task =>
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

                                frame.ReadTongGangKamiResult().ContinueWith(readtask =>  //确认绑定按钮
                                {
                                    if (readtask.Exception == null)
                                    {
                                        message = readtask.Result;

                                        // Now we're not on the main thread, perhaps the
                                        // Cef UI thread. It's not safe to work with
                                        // form UI controls or to block this thread.
                                        // Queue up a delegate to be executed on the
                                        // main thread.

                                        System.Threading.Thread.Sleep(1200 + (new Random().Next(200))); //延时1秒

                                        //成功了 
                                        if (message.IndexOf("抱歉") != -1)
                                        {
                                            //更新显示
                                            this.InvokeOnUiThreadIfRequired(() => UpdateLineState(index, message));

                                            System.Threading.Thread.Sleep(2500 + (new Random().Next(400))); //延时1秒
                                            TongGangNextBangDing();
                                        }
                                        else
                                        {
                                            //oktask.Result;

                                            //更新显示
                                            this.InvokeOnUiThreadIfRequired(() => UpdateLineState(index, message));

                                            System.Threading.Thread.Sleep(2500 + (new Random().Next(400))); //延时1秒
                                            TongGangNextBangDing();
                                            
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
        }

        private void TongGangNextBangDing()
        {
            //绑定下一个
            this.InvokeOnUiThreadIfRequired(() => TongGangBangDing(++kami_index));
        }

        private void start_bangding_Click(object sender, EventArgs e)
        {
            //打开页面
            page_isloading = true;
            browser.Load("https://ka.suning.com/mycard/query.htm");
            while (page_isloading)
                System.Threading.Thread.Sleep(1000);

            //初始化
            kami_index = 0;
            TongGangBangDing(kami_index);
        }

        private void UpdateLineState(int index, string msg)
        {
            if(index < richTextBoxKaimi.Lines.Length)
            {
                string text = richTextBoxKaimi.Lines[index]; //拿到此行文本
                int lineFirstCharIndex = richTextBoxKaimi.GetFirstCharIndexFromLine(index);//此行第一个char的索引

                //!!网络延迟,未知错误!!

                //抱歉，兑换码不存在
                //抱歉，兑换码已使用"

                //兑换码余额100.00\n手续费1.00\n本月已获取10次

                if (msg.IndexOf("抱歉") != -1)
                    text = richTextBoxKaimi.Lines[index] += " 失败:" + msg.Substring(msg.Length - 6, 6); // 修改此行文本
                else if(msg.IndexOf("兑换码余额") != -1)
                {
                    int total = msg.IndexOf("兑换码余额") + 5;
                    int total_end = msg.IndexOf('.', total);
                    int n = msg.IndexOf("手续费") + 3;
                    int n_end = msg.IndexOf('.', n);
                    int value = Convert.ToInt32(msg.Substring(total, total_end - total)) - Convert.ToInt32(msg.Substring(n, n_end - n));

                    text = richTextBoxKaimi.Lines[index] += (" 成功:" + value.ToString());
                }  
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
