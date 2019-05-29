using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CefSharp.MinimalExample.WinForms
{
    public static class EvaluateScriptHelper
    {
        public static async Task<bool> OpenLogicPage(this IFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentException("An IFrame instance is required.", "frame");
            }

            // Scripts should be minified for production builds. The script
            // could also be read from a file...
            const string script =
                @"(function ()
                {
                    var success = true;
                    var logiclink = document.getElementsByClassName('link-login');
                    if((logiclink !=null)&&(logiclink[0]!=null))
                        logiclink[0].click();
                    return success;
                })();";

            var response = await frame.EvaluateScriptAsync(script);
            if (!response.Success)
            {
                throw new Exception(response.Message);
            }

            return (bool)response.Result;
        }

        public static async Task<bool> InputGangBengKami(this IFrame frame, string kami0, string kami1, string kami2, string kami3)
        {
            if (frame == null)
            {
                throw new ArgumentException("An IFrame instance is required.", "frame");
            }

            // Scripts should be minified for production builds. The script
            // could also be read from a file...
            string script =
                @"(function ()
                    {
                        var success = true;
                    
                        var cards = document.getElementsByName('cardpass');
                        cards[0].value = '##ID1##';
                        cards[1].value = '##ID2##';
                        cards[2].value = '##ID3##';
                        cards[3].value = '##ID4##';

                        //立即兑换
                        var lijiduihuanbtns = document.getElementsByClassName('ex-btn');
                        lijiduihuanbtns[0].click();

                        return success;
                    })();";

            // For simple inline scripts you could use String.Format() but
            // beware of braces in the javascript code. If reading from a file
            // it's probably safer to include tokens that can be replaced via
            // regex.
            script = Regex.Replace(script, "##ID1##", kami0);
            script = Regex.Replace(script, "##ID2##", kami1);
            script = Regex.Replace(script, "##ID3##", kami2);
            script = Regex.Replace(script, "##ID4##", kami3);

            var response = await frame.EvaluateScriptAsync(script);
            if (!response.Success)
            {
                throw new Exception(response.Message);
            }

            return (bool)response.Result;
        }

        public static async Task<string> ReadGangBengKamiResult(this IFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentException("An IFrame instance is required.", "frame");
            }

            // Scripts should be minified for production builds. The script
            // could also be read from a file...
            string script =
                @"(function ()
                    {
                        var Success = true;
                        var Message = null;

                        //查找确认兑换对话框
                        var mekesuremsgpopdlg = document.getElementById('J_cardMsgPop');
                        if((mekesuremsgpopdlg!=null)&&(mekesuremsgpopdlg.style.display=='block'))
                        {
                            var msgs = document.getElementsByClassName('kami-msg');
                            Message = msgs[0].innerText;

                            var surebtn = document.getElementById('J_cardBtn');
                            if(surebtn!= null)
                                surebtn.click();
                        }
                        else
                        {
                            var error = document.getElementsByClassName('error-font');
                            Message = error[0].innerText;
                            Success = false;
                        }

                        return Message;
                    })();";

            var response = await frame.EvaluateScriptAsync(script);
            if (!response.Success)
            {
                throw new Exception(response.Message);
            }
            return (string)(response.Result);
        }

        public static async Task<bool> GangBengKamiOk(this IFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentException("An IFrame instance is required.", "frame");
            }

            // Scripts should be minified for production builds. The script
            // could also be read from a file...
            string script =
                @"(function ()
                    {
                        var success = true;
                    
                        var okbtns = document.getElementsByClassName('ui-btn');
                        if(okbtns!= null)
                            okbtns[0].click();

                        return success;
                    })();";

            var response = await frame.EvaluateScriptAsync(script);
            if (!response.Success)
            {
                throw new Exception(response.Message);
            }

            return (bool)response.Result;
        }
    }
}
